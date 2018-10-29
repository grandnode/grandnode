using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Plugins;
using Grand.Framework.Security;
using Grand.Services.Installation;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Infrastructure.Installation;
using Grand.Web.Models.Install;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Grand.Web.Controllers
{
    public partial class InstallController : Controller
    {
        #region Fields

        private readonly IInstallationLocalizationService _locService;
        private readonly GrandConfig _config;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public InstallController(IInstallationLocalizationService locService, GrandConfig config, ICacheManager cacheManager)
        {
            this._locService = locService;
            this._config = config;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// A value indicating whether we use MARS (Multiple Active Result Sets)
        /// </summary>
        protected bool UseMars
        {
            get { return false; }
        }


        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            var model = new InstallModel
            {
                AdminEmail = "admin@yourstore.com",
                InstallSampleData = false,
                DatabaseConnectionString = "",
                DataProvider = "mongodb",
            };
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code,
                });
            }
            //prepare collation list
            foreach (var col in _locService.GetAvailableCollations())
            {
                model.AvailableCollation.Add(new SelectListItem
                {
                    Value = col.Value,
                    Text = col.Name,
                    Selected = _locService.GetCurrentLanguage().Code == col.Value,
                });
            }
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Index(InstallModel model)
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            if (model.DatabaseConnectionString != null)
                model.DatabaseConnectionString = model.DatabaseConnectionString.Trim();

            string connectionString = "";

            if (model.MongoDBConnectionInfo)
            {
                if (String.IsNullOrEmpty(model.DatabaseConnectionString))
                {
                    ModelState.AddModelError("", _locService.GetResource("ConnectionStringRequired"));
                }
                else
                {
                    connectionString = model.DatabaseConnectionString;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(model.MongoDBDatabaseName))
                {
                    ModelState.AddModelError("", _locService.GetResource("DatabaseNameRequired"));
                }
                if (String.IsNullOrEmpty(model.MongoDBServerName))
                {
                    ModelState.AddModelError("", _locService.GetResource("MongoDBServerNameRequired"));
                }
                string userNameandPassword = "";
                if (!(String.IsNullOrEmpty(model.MongoDBUsername)))
                {
                    userNameandPassword = model.MongoDBUsername + ":" + model.MongoDBPassword + "@";
                }

                connectionString = "mongodb://" + userNameandPassword + model.MongoDBServerName + "/" + model.MongoDBDatabaseName;
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var client = new MongoClient(connectionString);
                    var databaseName = new MongoUrl(connectionString).DatabaseName;
                    var database = client.GetDatabase(databaseName);
                    database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

                    var filter = new BsonDocument("name", "GrandNodeVersion");
                    var found = database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter }).Result;

                    if (found.Any())
                        ModelState.AddModelError("", _locService.GetResource("AlreadyInstalled"));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
            }
            else
                ModelState.AddModelError("", _locService.GetResource("ConnectionStringRequired"));

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();

            //validate permissions
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
            foreach (string dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                    ModelState.AddModelError("", string.Format(_locService.GetResource("ConfigureDirectoryPermissions"), WindowsIdentity.GetCurrent().Name, dir));

            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (string file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                    ModelState.AddModelError("", string.Format(_locService.GetResource("ConfigureFilePermissions"), WindowsIdentity.GetCurrent().Name, file));

            if (ModelState.IsValid)
            {
                var settingsManager = new DataSettingsManager();
                try
                {
                    //save settings
                    var settings = new DataSettings
                    {
                        DataProvider = "mongodb",
                        DataConnectionString = connectionString
                    };
                    settingsManager.SaveSettings(settings);

                    var dataProviderInstance = EngineContext.Current.Resolve<BaseDataProviderManager>().LoadDataProvider();
                    dataProviderInstance.InitDatabase();

                    var dataSettingsManager = new DataSettingsManager();
                    var dataProviderSettings = dataSettingsManager.LoadSettings(reloadSettings: true);

                    var installationService = EngineContext.Current.Resolve<IInstallationService>();
                    installationService.InstallData(model.AdminEmail, model.AdminPassword, model.Collation, model.InstallSampleData);

                    //reset cache
                    DataSettingsHelper.ResetCache();

                    //install plugins
                    PluginManager.MarkAllPluginsAsUninstalled();
                    var pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
                    var plugins = pluginFinder.GetPlugins<IPlugin>(LoadPluginsMode.All)
                        .ToList()
                        .OrderBy(x => x.PluginDescriptor.Group)
                        .ThenBy(x => x.PluginDescriptor.DisplayOrder)
                        .ToList();

                    var pluginsIgnoredDuringInstallation = String.IsNullOrEmpty(_config.PluginsIgnoredDuringInstallation) ?
                        new List<string>() :
                        _config.PluginsIgnoredDuringInstallation
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    foreach (var plugin in plugins)
                    {
                        if (pluginsIgnoredDuringInstallation.Contains(plugin.PluginDescriptor.SystemName))
                            continue;

                        try
                        {
                            plugin.Install();
                        }
                        catch (Exception ex)
                        {
                            var _logger = EngineContext.Current.Resolve<ILogger>();
                            _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Error during installing plugin " + plugin.PluginDescriptor.SystemName,
                                ex.Message + " " + ex.InnerException?.Message);
                        }
                    }

                    //register default permissions
                    var permissionProviders = new List<Type>();
                    permissionProviders.Add(typeof(StandardPermissionProvider));
                    foreach (var providerType in permissionProviders)
                    {
                        var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                        EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);
                    }

                    //restart application
                    if (Core.OperatingSystem.IsWindows())
                    {
                        webHelper.RestartAppDomain();
                        //Redirect to home page
                        return RedirectToRoute("HomePage");
                    }
                    else
                    {
                        return View(new InstallModel() { Installed = true });
                    }
                }
                catch (Exception exception)
                {
                    //reset cache
                    DataSettingsHelper.ResetCache();
                    _cacheManager.Clear();

                    System.IO.File.Delete(CommonHelper.MapPath("~/App_Data/Settings.txt"));

                    ModelState.AddModelError("", string.Format(_locService.GetResource("SetupFailed"), exception.Message + " " + exception.InnerException?.Message));
                }
            }

            //prepare language list
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code,
                });
            }

            //prepare collation list
            foreach (var col in _locService.GetAvailableCollations())
            {
                model.AvailableCollation.Add(new SelectListItem
                {
                    Value = col.Value,
                    Text = col.Name,
                    Selected = _locService.GetCurrentLanguage().Code == col.Value,
                });
            }

            return View(model);
        }

        public virtual IActionResult ChangeLanguage(string language)
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            _locService.SaveCurrentLanguage(language);

            //Reload the page
            return RedirectToAction("Index", "Install");
        }

        public virtual IActionResult RestartInstall()
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            //restart application
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            webHelper.RestartAppDomain();

            //Redirect to home page
            return RedirectToRoute("HomePage");
        }

        #endregion
    }
}
