using Grand.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grand.Core.Plugins
{
    /// <summary>
    /// Sets the application up for the plugin referencing
    /// </summary>
    public static class PluginManager
    {
        #region Const

        public const string InstalledPluginsFilePath = "~/App_Data/InstalledPlugins.txt";
        public const string PluginsPath = "~/Plugins";
        public const string ShadowCopyPath = "~/Plugins/bin";

        #endregion

        #region Fields

        private static DirectoryInfo _shadowCopyFolder;
        private static DirectoryInfo _pluginFolder;
        private static GrandConfig _config;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a collection of all referenced plugin assemblies that have been shadow copied
        /// </summary>
        public static IEnumerable<PluginDescriptor> ReferencedPlugins { get; set; }

        /// <summary>
        /// Returns a collection of all plugin which are not compatible with the current version
        /// </summary>
        public static IEnumerable<string> IncompatiblePlugins { get; set; }

        /// <summary>
        /// Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Initialize(IMvcCoreBuilder mvcCoreBuilder, GrandConfig config)
        {
            if (mvcCoreBuilder == null)
                throw new ArgumentNullException("mvcCoreBuilder");

            _config = config ?? throw new ArgumentNullException("config");

            _pluginFolder = new DirectoryInfo(CommonHelper.MapPath(PluginsPath));
            _shadowCopyFolder = new DirectoryInfo(CommonHelper.MapPath(ShadowCopyPath));

            var referencedPlugins = new List<PluginDescriptor>();
            var incompatiblePlugins = new List<string>();

            try
            {
                var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());

                Log.Information("Creating shadow copy folder and querying for dlls");
                //ensure folders are created
                Directory.CreateDirectory(_pluginFolder.FullName);
                Directory.CreateDirectory(_shadowCopyFolder.FullName);

                //get list of all files in bin
                var binFiles = _shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);
                if (config.ClearPluginShadowDirectoryOnStartup)
                {
                    //clear out shadow copied plugins
                    foreach (var f in binFiles)
                    {
                        Log.Information($"Deleting {f.Name}");
                        try
                        {
                            //ignore index.htm
                            var fileName = Path.GetFileName(f.FullName);
                            if (fileName.Equals("index.htm", StringComparison.OrdinalIgnoreCase))
                                continue;

                            File.Delete(f.FullName);
                        }
                        catch (Exception exc)
                        {
                            Log.Error(exc, "PluginManager");
                        }
                    }
                }

                //load description files
                foreach (var pluginDescriptor in GetDescriptions())
                {
                    //ensure that version of plugin is valid
                    if (!pluginDescriptor.SupportedVersions.Contains(GrandVersion.SupportedPluginVersion, StringComparer.OrdinalIgnoreCase))
                    {
                        incompatiblePlugins.Add(pluginDescriptor.SystemName);
                        continue;
                    }

                    //some validation
                    if (string.IsNullOrWhiteSpace(pluginDescriptor.SystemName))
                        throw new Exception(string.Format("A plugin '{0}' has no system name. Try assigning the plugin a unique name and recompiling.", pluginDescriptor.SystemName));
                    if (referencedPlugins.Contains(pluginDescriptor))
                        throw new Exception(string.Format("A plugin with '{0}' system name is already defined", pluginDescriptor.SystemName));

                    //set 'Installed' property
                    pluginDescriptor.Installed = installedPluginSystemNames
                        .FirstOrDefault(x => x.Equals(pluginDescriptor.SystemName, StringComparison.OrdinalIgnoreCase)) != null;

                    try
                    {
                        if (!config.PluginShadowCopy)
                        {
                            //remove deps.json files 
                            var depsFiles = pluginDescriptor.OriginalAssemblyFile.Directory.GetFiles("*.deps.json", SearchOption.TopDirectoryOnly);
                            foreach (var f in depsFiles)
                            {
                                try
                                {
                                    File.Delete(f.FullName);
                                }
                                catch (Exception exc)
                                {
                                    Log.Error(exc, "PluginManager");
                                }
                            }
                        }

                        //main plugin file
                        AddApplicationPart(mvcCoreBuilder, pluginDescriptor.ReferencedAssembly, pluginDescriptor.SystemName, pluginDescriptor.PluginFileName);

                        //init plugin type (only one plugin per assembly is allowed)
                        foreach (var t in pluginDescriptor.ReferencedAssembly.GetTypes())
                            if (typeof(IPlugin).IsAssignableFrom(t))
                                if (!t.GetTypeInfo().IsInterface)
                                    if (t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                                    {
                                        pluginDescriptor.PluginType = t;
                                        break;
                                    }

                        referencedPlugins.Add(pluginDescriptor);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        //add a plugin name. this way we can easily identify a problematic plugin
                        var msg = string.Format("Plugin '{0}'. ", pluginDescriptor.FriendlyName);
                        foreach (var e in ex.LoaderExceptions)
                            msg += e.Message + Environment.NewLine;

                        var fail = new Exception(msg, ex);
                        throw fail;
                    }
                    catch (Exception ex)
                    {
                        //add a plugin name. this way we can easily identify a problematic plugin
                        var msg = string.Format("Plugin '{0}'. {1}", pluginDescriptor.FriendlyName, ex.Message);

                        var fail = new Exception(msg, ex);
                        throw fail;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = string.Empty;
                for (var e = ex; e != null; e = e.InnerException)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                throw fail;
            }

            ReferencedPlugins = referencedPlugins;
            IncompatiblePlugins = incompatiblePlugins;

        }

        /// <summary>
        /// Mark plugin as installed
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static async Task MarkPluginAsInstalled(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            var alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.OrdinalIgnoreCase)) != null;
            if (!alreadyMarkedAsInstalled)
                installedPluginSystemNames.Add(systemName);
            await PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static async Task MarkPluginAsUninstalled(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            var alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.OrdinalIgnoreCase)) != null;
            if (alreadyMarkedAsInstalled)
                installedPluginSystemNames.Remove(systemName);
            await PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        public static void MarkAllPluginsAsUninstalled()
        {
            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Find a plugin descriptor by some type which is located into the same assembly as plugin
        /// </summary>
        /// <param name="typeInAssembly">Type</param>
        /// <returns>Plugin descriptor if exists; otherwise null</returns>
        public static PluginDescriptor FindPlugin(Type typeInAssembly)
        {
            if (typeInAssembly == null)
                throw new ArgumentNullException("typeInAssembly");

            if (ReferencedPlugins == null)
                return null;

            return ReferencedPlugins.FirstOrDefault(plugin => plugin.ReferencedAssembly != null
                && plugin.ReferencedAssembly.FullName.Equals(typeInAssembly.GetTypeInfo().Assembly.FullName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get description files
        /// </summary>
        /// <param name="pluginFolder">Plugin directory info</param>
        /// <returns>Original and parsed description files</returns>
        private static IList<PluginDescriptor> GetDescriptions()
        {
            if (_pluginFolder == null)
                throw new ArgumentNullException("pluginFolder");

            //create list (<file info, parsed plugin descritor>)
            var result = new List<PluginDescriptor>();
            //add display order and path to list
            foreach (var pluginFile in _pluginFolder.GetFiles("*.dll", SearchOption.AllDirectories))
            {
                if (!IsPackagePluginFolder(pluginFile.Directory))
                    continue;

                if (!string.IsNullOrEmpty(_config.PluginSkipLoadingPattern)
                    && Matches(pluginFile.Name, _config.PluginSkipLoadingPattern))
                    continue;

                //prepare plugin descriptor
                var pluginDescriptor = PreparePluginDescriptor(pluginFile);
                if (pluginDescriptor == null)
                    continue;

                //populate list
                result.Add(pluginDescriptor);
            }

            //sort list by display order.
            result = result.OrderBy(x => x.DisplayOrder).ToList();
            return result;
        }

        private static PluginDescriptor PreparePluginDescriptor(FileInfo pluginFile)
        {
            var _plug = _config.PluginShadowCopy ? ShadowCopyFile(pluginFile, Directory.CreateDirectory(_shadowCopyFolder.FullName)) : pluginFile;

            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_plug.FullName);

            var pluginInfo = assembly.GetCustomAttribute<PluginInfoAttribute>();
            if (pluginInfo == null)
            {
                return null;
            }

            var descriptor = new PluginDescriptor {
                FriendlyName = pluginInfo.FriendlyName,
                Group = pluginInfo.Group,
                SystemName = pluginInfo.SystemName,
                Version = pluginInfo.Version,
                SupportedVersions = new List<string> { pluginInfo.SupportedVersion },
                Author = pluginInfo.Author,
                PluginFileName = _plug.Name,
                OriginalAssemblyFile = pluginFile,
                ReferencedAssembly = assembly
            };

            var cfgfile = Path.Combine(pluginFile.Directory.FullName, "config.cfg");
            if (File.Exists(cfgfile))
            {
                var pluginConfiguration = JsonConvert.DeserializeObject<PluginConfiguration>(File.ReadAllText(cfgfile));
                if (!string.IsNullOrEmpty(pluginConfiguration.FriendlyName))
                    descriptor.FriendlyName = pluginConfiguration.FriendlyName;
                descriptor.DisplayOrder = pluginConfiguration.DisplayOrder;
                descriptor.LimitedToStores = pluginConfiguration.LimitedToStore;
            }

            return descriptor;
        }

        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo ShadowCopyFile(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            //check if a shadow copied file already exists and if it does, check if it's updated, if not don't copy
            if (shadowCopiedPlug.Exists)
            {
                //it's better to use LastWriteTimeUTC, but not all file systems have this property
                //maybe it is better to compare file hash?
                var areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    Log.Information($"Not copying; files appear identical: {shadowCopiedPlug.Name}");
                    shouldCopy = false;
                }
                else
                {
                    //delete an existing file
                    Log.Information($"New plugin found; Deleting the old file: {shadowCopiedPlug.Name}");
                    try
                    {
                        File.Delete(shadowCopiedPlug.FullName);
                    }
                    catch (Exception ex)
                    {
                        shouldCopy = false;
                        Log.Error(ex, "PluginManager");
                    }
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    Log.Information($"{shadowCopiedPlug.FullName} is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException exc)
                    {
                        throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                    }
                    //ok, we've made it this far, now retry the shadow copy
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
        }

        private static bool Matches(string fullName, string pattern)
        {
            return Regex.IsMatch(fullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private static void AddApplicationPart(IMvcCoreBuilder mvcCoreBuilder,
            Assembly assembly, string systemName, string filename)
        {
            try
            {
                //we can now register the plugin definition
                Log.Information("Adding to ApplicationParts: '{0}'", systemName);
                mvcCoreBuilder.AddApplicationPart(assembly);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PluginManager");
                throw new InvalidOperationException($"The plugin directory for the {systemName} file exists in a folder outside of the allowed grandnode folder hierarchy - exception because of {filename} - exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder == null) return false;
            if (folder.Parent == null) return false;
            if (!folder.Parent.Name.Equals("Plugins", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        /// <summary>
        /// Gets the full path of InstalledPlugins.txt file
        /// </summary>
        /// <returns></returns>
        private static string GetInstalledPluginsFilePath()
        {
            return CommonHelper.MapPath(InstalledPluginsFilePath);
        }

        #endregion
    }
}
