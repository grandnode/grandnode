/* Copyright 2010-2015 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace Grand.Services.Tests
{
    public static class CoreTestConfiguration
    {
        #region static
        // static fields
        private static Lazy<ICluster> __cluster = new Lazy<ICluster>(CreateCluster, isThreadSafe: true);
        private static ConnectionString __connectionString = GetConnectionString();
        private static DatabaseNamespace __databaseNamespace = GetDatabaseNamespace();
        private static MessageEncoderSettings __messageEncoderSettings = new MessageEncoderSettings();
        private static TraceSource __traceSource;

        // static properties
        public static ICluster Cluster
        {
            get { return __cluster.Value; }
        }

        public static ConnectionString ConnectionString
        {
            get { return __connectionString; }
        }

        public static DatabaseNamespace DatabaseNamespace
        {
            get { return __databaseNamespace; }
        }

        public static MessageEncoderSettings MessageEncoderSettings
        {
            get { return __messageEncoderSettings; }
        }

        public static SemanticVersion ServerVersion
        {
            get
            {
                var server = __cluster.Value.SelectServer(WritableServerSelector.Instance, CancellationToken.None);
                return server.Description.Version;
            }
        }

        public static TraceSource TraceSource
        {
            get { return __traceSource; }
        }

        // static methods
        public static ClusterBuilder ConfigureCluster()
        {
            return ConfigureCluster(new ClusterBuilder());
        }

        public static ClusterBuilder ConfigureCluster(ClusterBuilder builder)
        {
            var serverSelectionTimeoutString = Environment.GetEnvironmentVariable("MONGO_SERVER_SELECTION_TIMEOUT_MS");
            if (serverSelectionTimeoutString == null)
            {
                serverSelectionTimeoutString = "30000";
            }

            builder = builder
                .ConfigureWithConnectionString(__connectionString)
                .ConfigureCluster(c => c.With(serverSelectionTimeout: TimeSpan.FromMilliseconds(int.Parse(serverSelectionTimeoutString))));

            if (__connectionString.Ssl.HasValue && __connectionString.Ssl.Value)
            {
                var certificateFilename = Environment.GetEnvironmentVariable("MONGO_SSL_CERT_FILE");
                if (certificateFilename != null)
                {
                    builder.ConfigureSsl(ssl =>
                    {
                        var password = Environment.GetEnvironmentVariable("MONGO_SSL_CERT_PASS");
                        X509Certificate cert;
                        if (password == null)
                        {
                            cert = new X509Certificate2(certificateFilename);
                        }
                        else
                        {
                            cert = new X509Certificate2(certificateFilename, password);
                        }
                        return ssl.With(
                            clientCertificates: new[] { cert });
                    });
                }
            }

            return ConfigureLogging(builder);
        }

        public static ClusterBuilder ConfigureLogging(ClusterBuilder builder)
        {
            var environmentVariable = Environment.GetEnvironmentVariable("MONGO_LOGGING");
            if (environmentVariable == null)
            {
                return builder;
            }

            SourceLevels defaultLevel;
            if (!Enum.TryParse<SourceLevels>(environmentVariable, ignoreCase: true, result: out defaultLevel))
            {
                return builder;
            }

            __traceSource = new TraceSource("mongodb-tests", defaultLevel);
            __traceSource.Listeners.Clear();
            return builder.TraceWith(__traceSource);
        }

        public static ICluster CreateCluster()
        {
            var hasWritableServer = 0;
            var builder = ConfigureCluster();
            var cluster = builder.BuildCluster();
            cluster.DescriptionChanged += (o, e) =>
            {
                var anyWritableServer = e.NewClusterDescription.Servers.Any(
                    description => description.Type.IsWritable());
                if (__traceSource != null)
                {
                    __traceSource.TraceEvent(TraceEventType.Information, 0, $"CreateCluster: DescriptionChanged event handler called.");
                    __traceSource.TraceEvent(TraceEventType.Information, 0, $"CreateCluster: anyWritableServer = {anyWritableServer}.");
                    __traceSource.TraceEvent(TraceEventType.Information, 0, $"CreateCluster: new description: {e.NewClusterDescription.ToString()}.");
                }
                Interlocked.Exchange(ref hasWritableServer, anyWritableServer ? 1 : 0);
            };
            if (__traceSource != null)
            {
                __traceSource.TraceEvent(TraceEventType.Information, 0, "CreateCluster: initializing cluster.");
            }
            cluster.Initialize();

            // wait until the cluster has connected to a writable server
            SpinWait.SpinUntil(() => Interlocked.CompareExchange(ref hasWritableServer, 0, 0) != 0, TimeSpan.FromSeconds(30));
            if (Interlocked.CompareExchange(ref hasWritableServer, 0, 0) == 0)
            {
                var message = string.Format(
                    "Test cluster has no writable server. Client view of the cluster is {0}.",
                    cluster.Description.ToString());
                throw new Exception(message);
            }

            if (__traceSource != null)
            {
                __traceSource.TraceEvent(TraceEventType.Information, 0, "CreateCluster: writable server found.");
            }

            return cluster;
        }

        public static CollectionNamespace GetCollectionNamespaceForTestFixture()
        {
            var testFixtureType = GetTestFixtureTypeFromCallStack();
            var collectionName = TruncateCollectionNameIfTooLong(__databaseNamespace, testFixtureType.Name);
            return new CollectionNamespace(__databaseNamespace, collectionName);
        }

        public static CollectionNamespace GetCollectionNamespaceForTestMethod()
        {
            var testMethodInfo = GetTestMethodInfoFromCallStack();
            var collectionName = TruncateCollectionNameIfTooLong(__databaseNamespace, testMethodInfo.DeclaringType.Name + "-" + testMethodInfo.Name);
            return new CollectionNamespace(__databaseNamespace, collectionName);
        }

        private static ConnectionString GetConnectionString()
        {
            return new ConnectionString(Environment.GetEnvironmentVariable("MONGO_URI") ?? "mongodb://localhost");
        }

        private static DatabaseNamespace GetDatabaseNamespace()
        {
            if (!string.IsNullOrEmpty(__connectionString.DatabaseName))
            {
                return new DatabaseNamespace(__connectionString.DatabaseName);
            }

            var timestamp = DateTime.Now.ToString("MMddHHmm");
            return new DatabaseNamespace("Tests" + timestamp);
        }

        public static DatabaseNamespace GetDatabaseNamespaceForTestFixture()
        {
            var testFixtureType = GetTestFixtureTypeFromCallStack();
            var databaseName = TruncateDatabaseNameIfTooLong(__databaseNamespace.DatabaseName + "-" + testFixtureType.Name);
            if (databaseName.Length >= 64)
            {
                databaseName = databaseName.Substring(0, 63);
            }
            return new DatabaseNamespace(databaseName);
        }

        
        

        private static Type GetTestFixtureTypeFromCallStack()
        {
            var stackTrace = new StackTrace();
            for (var index = 0; index < stackTrace.FrameCount; index++)
            {
                var frame = stackTrace.GetFrame(index);
                var methodInfo = frame.GetMethod();
                var declaringType = methodInfo.DeclaringType;
                var testFixtureAttribute = declaringType.GetCustomAttribute<TestFixtureAttribute>(inherit: false);
                if (testFixtureAttribute != null)
                {
                    return declaringType;
                }
            }

            throw new Exception("No [TestFixture] found on the call stack.");
        }

        private static MethodInfo GetTestMethodInfoFromCallStack()
        {
            var stackTrace = new StackTrace();
            for (var index = 0; index < stackTrace.FrameCount; index++)
            {
                var frame = stackTrace.GetFrame(index);
                var methodInfo = frame.GetMethod() as MethodInfo;
                if (methodInfo != null)
                {
                    var testAttribute = methodInfo.GetCustomAttribute<TestAttribute>(inherit: false);
                    if (testAttribute != null)
                    {
                        return methodInfo;
                    }
                    var testCaseAttribute = methodInfo.GetCustomAttribute<TestCaseAttribute>(inherit: false);
                    if (testCaseAttribute != null)
                    {
                        return methodInfo;
                    }
                }
            }

            throw new Exception("No [TestFixture] found on the call stack.");
        }

        private static string TruncateCollectionNameIfTooLong(DatabaseNamespace databaseNamespace, string collectionName)
        {
            var fullNameLength = databaseNamespace.DatabaseName.Length + 1 + collectionName.Length;
            if (fullNameLength < 123)
            {
                return collectionName;
            }
            else
            {
                var maxCollectionNameLength = 123 - (databaseNamespace.DatabaseName.Length + 1);
                return collectionName.Substring(0, maxCollectionNameLength);
            }
        }

        private static string TruncateDatabaseNameIfTooLong(string databaseName)
        {
            if (databaseName.Length < 64)
            {
                return databaseName;
            }
            else
            {
                return databaseName.Substring(0, 63);
            }
        }
        #endregion


        public static void TearDown()
        {
            if (__cluster.IsValueCreated)
            {
                // TODO: DropDatabase
                //DropDatabase();
                __cluster.Value.Dispose();
                __cluster = new Lazy<ICluster>(CreateCluster, isThreadSafe: true);
            }
        }
    }
}