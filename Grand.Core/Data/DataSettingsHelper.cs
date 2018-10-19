using System;

namespace Grand.Core.Data
{
    /// <summary>
    /// Data settings helper
    /// </summary>
    public static class DataSettingsHelper
    {
        private static bool? _databaseIsInstalled;
        private static string _connectionString;
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
                _databaseIsInstalled = settings != null && !String.IsNullOrEmpty(settings.DataConnectionString);
                if (!String.IsNullOrEmpty(settings.DataConnectionString))
                    _connectionString = settings.DataConnectionString;
            }
            return _databaseIsInstalled.Value;
        }
        public static void InitConnectionString()
        {
            var manager = new DataSettingsManager();
            var settings = manager.LoadSettings();
            if (!String.IsNullOrEmpty(settings.DataConnectionString))
                _connectionString = settings.DataConnectionString;
        }
        public static string ConnectionString()
        {
            return _connectionString;
        }

        //Reset information cached in the "DatabaseIsInstalled" method
        public static void ResetCache()
        {
            _databaseIsInstalled = false;
        }
       
    }
}
