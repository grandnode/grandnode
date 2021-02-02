using Grand.Core;
using Grand.Core.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Grand.Framework.Themes
{
    public partial class ThemeProvider : IThemeProvider
    {
        #region Fields

        private readonly IList<ThemeConfiguration> _themeConfigurations = new List<ThemeConfiguration>();
        private readonly string _basePath = string.Empty;

        #endregion

        #region Constructors

        public ThemeProvider()
        {
            _basePath = CommonHelper.MapPath("~/Themes/");
            LoadConfigurations();
        }

        #endregion

        #region Utility

        private void LoadConfigurations()
        {
            foreach (string themeName in Directory.GetDirectories(_basePath))
            {
                var configuration = CreateThemeConfiguration(themeName);
                if (configuration != null)
                {
                    _themeConfigurations.Add(configuration);
                }
            }
        }

        private ThemeConfiguration CreateThemeConfiguration(string themePath)
        {
            var themeDirectory = new DirectoryInfo(themePath);
            var themeConfigFile = new FileInfo(Path.Combine(themeDirectory.FullName, "theme.cfg"));

            if (themeConfigFile.Exists)
            {
                var themeConfiguration = JsonConvert.DeserializeObject<ThemeConfiguration>(File.ReadAllText(themeConfigFile.FullName));
                if (themeConfiguration != null)
                {
                    themeConfiguration.Name = themeDirectory.Name;
                    return themeConfiguration;
                }
            }
            return null;
        }

        #endregion

        #region Methods

        public ThemeConfiguration GetThemeConfiguration(string themeName)
        {
            return _themeConfigurations
                .SingleOrDefault(x => x.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        }

        public IList<ThemeConfiguration> GetThemeConfigurations()
        {
            return _themeConfigurations;
        }

        public bool ThemeConfigurationExists(string themeName)
        {
            return GetThemeConfigurations().Any(configuration => configuration.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        }

        public ThemeDescriptor GetThemeDescriptorFromText(string text)
        {
            var themeDescriptor = new ThemeDescriptor();
            try
            {
                var themeConfiguration = JsonConvert.DeserializeObject<ThemeConfiguration>(text);
                themeDescriptor.FriendlyName = themeConfiguration.Title;
            }
            catch { }

            return themeDescriptor;
        }

        #endregion
    }
}
