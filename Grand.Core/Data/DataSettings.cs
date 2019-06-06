using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace Grand.Core.Data
{
    /// <summary>
    /// Data settings (connection string information)
    /// </summary>
    public partial class DataSettings
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public DataSettings()
        {
            RawDataSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Data provider
        /// </summary>
        public string DataProvider { get; set; } = "mongodb";

        /// <summary>
        /// Connection string
        /// </summary>
        public string DataConnectionString { get; set; }

        /// <summary>
        /// Raw settings file
        /// </summary>
        public IDictionary<string, string> RawDataSettings { get; private set; }

        public string MongoDBServerName {
            get => GetRawDataSetting("MongoDBServerName");
            set => RawDataSettings["MongoDBServerName"] = value;
        }

        public int MongoDBServerPort {
            get => string.IsNullOrEmpty(GetRawDataSetting("MongoDBServerPort")) ? 10255 : int.Parse(GetRawDataSetting("MongoDBServerPort"));
            set => RawDataSettings["MongoDBServerPort"] = value.ToString();
        }
        public string MongoDBDatabaseName {
            get => GetRawDataSetting("MongoDBDatabaseName");
            set => RawDataSettings["MongoDBDatabaseName"] = value;
        }
        public string MongoDBUsername {
            get => GetRawDataSetting("MongoDBUsername");
            set => RawDataSettings["MongoDBUsername"] = value;
        }

        public string MongoDBPassword {
            get => GetRawDataSetting("MongoDBPassword");
            set => RawDataSettings["MongoDBPassword"] = value;
        }

        public string MongoCredentialMechanism {
            get => GetRawDataSetting("MongoCredentialMechanism");
            set => RawDataSettings["MongoCredentialMechanism"] = value;
        }
        public string ReplicaSet {
            get => GetRawDataSetting("ReplicaSet");
            set => RawDataSettings["ReplicaSet"] = value;
        }
        public string Collation {
            get => GetRawDataSetting("Collation");
            set => RawDataSettings["Collation"] = value;
        }
        public SslProtocols SslProtocol {
            get => string.IsNullOrEmpty(GetRawDataSetting("SslProtocol")) ? SslProtocols.None : (SslProtocols)Enum.Parse(typeof(SslProtocols), RawDataSettings["SslProtocol"], true);
            set => RawDataSettings["SslProtocol"] = value.ToString();
        }
        public bool Installed {
            get => GetBoolenValue("Installed");
            set => RawDataSettings["Installed"] = value.ToString();
        }
        public bool DatabaseInstalled {
            get => GetBoolenValue("DatabaseInstalled");
            set => RawDataSettings["DatabaseInstalled"] = value.ToString();
        }
        public bool PluginsInstalled {
            get => GetBoolenValue("PluginsInstalled");
            set => RawDataSettings["PluginsInstalled"] = value.ToString();
        }
        public bool PermissionInstalled {
            get => GetBoolenValue("PermissionInstalled");
            set => RawDataSettings["PermissionInstalled"] = value.ToString();
        }

        public bool UseConnectionString {
            get => GetBoolenValue("UseConnectionString");
            set => RawDataSettings["UseConnectionString"] = value.ToString();
        }
        public bool InstallSampleData {
            get => GetBoolenValue("InstallSampleData");
            set => RawDataSettings["InstallSampleData"] = value.ToString();
        }
        public string InstallMessage {
            get => GetRawDataSetting("InstallMessage");
            set => RawDataSettings["InstallMessage"] = value;
        }

        public string AdminEmail {
            get => GetRawDataSetting("AdminEmail") ?? "uubuyer@outlook.com";
            set => RawDataSettings["AdminEmail"] = value;
        }

        public string AdminPassword {
            get => GetRawDataSetting("AdminPassword") ?? "123456";
            set => RawDataSettings["AdminPassword"] = value;
        }

        private bool GetBoolenValue(string key)
        {
            return !string.IsNullOrEmpty(GetRawDataSetting(key)) && bool.Parse(GetRawDataSetting(key));
        }
        private string GetRawDataSetting(string key)
        {
            if (!RawDataSettings.ContainsKey(key))
            {
                return null;
            }

            return RawDataSettings[key];
        }

        /// <summary>
        /// A value indicating whether entered information is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(this.DataProvider)
                && !String.IsNullOrEmpty(this.DataConnectionString);
        }
    }
}
