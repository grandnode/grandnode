using MongoDB.Driver;
using System;
using System.Security.Authentication;

namespace Grand.Core.Data
{
    /// <summary>
    /// Data settings helper
    /// </summary>
    public static class DataSettingsHelper
    {
        private static bool? _databaseIsInstalled;
        private static MongoClientSettings _mongoClientSettings;
        /// <summary>
        /// Returns a value indicating whether database is already installed
        /// </summary>
        /// <returns></returns>
        public static bool DatabaseIsInstalled()
        {
            if (!_databaseIsInstalled.HasValue)
            {
                var manager = new DataSettingsManager();
                var settings = manager.LoadSettings();
                _databaseIsInstalled = settings != null && settings.IsValid() && settings.Installed;
                if (settings.IsValid()) InitMongoClientSettings();
            }
            return _databaseIsInstalled.Value;
        }
        public static void InitConnectionString()
        {
            var manager = new DataSettingsManager();
            var settings = manager.LoadSettings();
            if (settings.IsValid()) InitMongoClientSettings();
        }

        public static MongoClient MongoClient()
        {
            return new MongoClient(MongoClientSettings());
        }
        public static void InitMongoClientSettings()
        {
            var dataProviderSettings = new DataSettingsManager().LoadSettings();

            if (!dataProviderSettings.IsValid())
            {
                throw new Exception("MongoDb is not correctly setup.");
            }
            _mongoClientSettings = new MongoClientSettings();
            _mongoClientSettings.Server = new MongoServerAddress(dataProviderSettings.MongoDBServerName, dataProviderSettings.MongoDBServerPort);

            if (dataProviderSettings.SslProtocol != SslProtocols.None)
            {
                _mongoClientSettings.UseSsl = true;
                _mongoClientSettings.SslSettings = new SslSettings();
                _mongoClientSettings.SslSettings.EnabledSslProtocols = dataProviderSettings.SslProtocol;
            }

            MongoIdentity identity = new MongoInternalIdentity(dataProviderSettings.MongoDBDatabaseName, dataProviderSettings.MongoDBUsername);
            MongoIdentityEvidence evidence = new PasswordEvidence(dataProviderSettings.MongoDBPassword);

            _mongoClientSettings.Credential = new MongoCredential(dataProviderSettings.MongoCredentialMechanism, identity, evidence);
        }

        public static MongoClientSettings MongoClientSettings()
        {
            if (_mongoClientSettings == null) InitMongoClientSettings();
            return _mongoClientSettings;
        }
        //Reset information cached in the "DatabaseIsInstalled" method
        public static void ResetCache()
        {
            _databaseIsInstalled = false;
            _mongoClientSettings = null;
        }

        public static IMongoDatabase GetDatabase(this IMongoClient client)
        {
            return client.GetDatabase(client.Settings.Credential.Identity.Source);
        }
    }
}
