using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Grand.Core;
using Grand.Core.Plugins;

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
            var themeConfigFile = new FileInfo(Path.Combine(themeDirectory.FullName, "theme.config"));

            if (themeConfigFile.Exists)
            {
                var doc = new XmlDocument();
                doc.Load(File.OpenRead(themeConfigFile.FullName));
                return new ThemeConfiguration(themeDirectory.Name, themeDirectory.FullName, doc);
            }

            return null;
        }

        #endregion

        #region Methods

        public ThemeConfiguration GetThemeConfiguration(string themeName)
        {
            return _themeConfigurations
                .SingleOrDefault(x => x.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        }

        public IList<ThemeConfiguration> GetThemeConfigurations()
        {
            return _themeConfigurations;
        }

        public bool ThemeConfigurationExists(string themeName)
        {
            return GetThemeConfigurations().Any(configuration => configuration.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        }

        public ThemeDescriptor GetThemeDescriptorFromText(string text)
        {
            ThemeDescriptor themeDescriptor = new ThemeDescriptor();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(text);

                XmlNodeList Tags = doc.GetElementsByTagName("Theme");
                var name = Tags[0].Attributes["title"].Value;
                themeDescriptor.FriendlyName = name;
            }
            catch { }

            return themeDescriptor;
        }

        #endregion
    }
}
