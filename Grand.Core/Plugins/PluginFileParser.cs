using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
