using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Grand.Core.Plugins
{
    /// <summary>
    /// Plugin files parser
    /// </summary>
    public static class PluginFileParser
    {
        public static IList<string> ParseInstalledPluginsFile(string filePath)
        {
            //read and parse the file
            if (!File.Exists(filePath))
                return new List<string>();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return new List<string>();

            var lines = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    lines.Add(str.Trim());
                }
            }
            return lines;
        }

        public static async Task SaveInstalledPluginsFile(IList<String> pluginSystemNames, string filePath)
        {
            string result = "";
            foreach (var sn in pluginSystemNames)
                result += string.Format("{0}{1}", sn, Environment.NewLine);

            await File.WriteAllTextAsync(filePath, result);
            await Task.CompletedTask;
        }

        public static PluginDescriptor PreparePluginDescriptor(FileInfo pluginFile)
        {
            var descriptor = new PluginDescriptor();
            var assembly = Assembly.LoadFrom(pluginFile.FullName);
            var pluginInfo = assembly.GetCustomAttribute<PluginInfoAttribute>();
            if (pluginInfo == null)
                return null;

            descriptor.FriendlyName = pluginInfo.FriendlyName;
            descriptor.Group = pluginInfo.Group;
            descriptor.SystemName = pluginInfo.SystemName;
            descriptor.Version = pluginInfo.Version;
            descriptor.SupportedVersions = new List<string> { pluginInfo.SupportedVersion };
            descriptor.Author = pluginInfo.Author;
            descriptor.PluginFileName = pluginInfo.FileName;
            descriptor.OriginalAssemblyFile = pluginFile;

            var cfgfile = Path.Combine(pluginFile.Directory.FullName, "config.cfg");
            if (File.Exists(cfgfile))
            {
                var config = JsonConvert.DeserializeObject<PluginConfiguration>(File.ReadAllText(cfgfile));
                if (!string.IsNullOrEmpty(config.FriendlyName))
                    descriptor.FriendlyName = config.FriendlyName;
                descriptor.DisplayOrder = config.DisplayOrder;
                descriptor.LimitedToStores = config.LimitedToStore;
            }

            return descriptor;
        }

        public static void SavePluginConfigFile(PluginDescriptor plugin)
        {
            if (plugin == null)
                throw new ArgumentException("plugin");
            var filePath = Path.Combine(plugin.OriginalAssemblyFile.Directory.FullName, "config.cfg");

            var config = new PluginConfiguration() {
                FriendlyName = plugin.FriendlyName,
                DisplayOrder = plugin.DisplayOrder,
                LimitedToStore = plugin.LimitedToStores
            };

            var content = JsonConvert.SerializeObject(config);
            File.WriteAllText(filePath, content);
        }
    }
}
