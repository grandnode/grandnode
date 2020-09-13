using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Cms;
using Grand.Domain.Customers;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Security.Authorization;
using Grand.Framework.Themes;
using Grand.Services.Authentication.External;
using Grand.Services.Cms;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Plugins;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Plugins)]
    public partial class PluginController : BaseAdminController
    {
        #region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreService _storeService;
        private readonly IThemeProvider _themeProvider;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region Constructors

        public PluginController(IPluginFinder pluginFinder,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ILanguageService languageService,
            ISettingService settingService,
            ICustomerActivityService customerActivityService,
            IStoreService storeService,
            IThemeProvider themeProvider,
            IMediator mediator,
            ICacheManager cacheManager,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            WidgetSettings widgetSettings)
        {
            _pluginFinder = pluginFinder;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _languageService = languageService;
            _settingService = settingService;
            _customerActivityService = customerActivityService;
            _storeService = storeService;
            _themeProvider = themeProvider;
            _mediator = mediator;
            _cacheManager = cacheManager;
            _paymentSettings = paymentSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual async Task<PluginModel> PreparePluginModel(PluginDescriptor pluginDescriptor,
            bool prepareLocales = true, bool prepareStores = true)
        {
            var pluginModel = pluginDescriptor.ToModel();
            //logo
            pluginModel.LogoUrl = pluginDescriptor.GetLogoUrl(_webHelper);

            if (prepareLocales)
            {
                //locales
                await AddLocales(_languageService, pluginModel.Locales, (locale, languageId) =>
                {
                    locale.FriendlyName = pluginDescriptor.Instance(_pluginFinder.ServiceProvider).GetLocalizedFriendlyName(_localizationService, languageId, false);
                });
            }
            if (prepareStores)
            {
                //stores
                pluginModel.AvailableStores = (await _storeService
                    .GetAllStores())
                    .Select(s => new StoreModel() { Id = s.Id, Name = s.Shortcut })
                    .ToList();
                pluginModel.SelectedStoreIds = pluginDescriptor.LimitedToStores.ToArray();
                pluginModel.LimitedToStores = pluginDescriptor.LimitedToStores.Count > 0;
            }


            //configuration URLs

            if (pluginDescriptor.Installed)
            {
                //display configuration URL only when a plugin is already installed
                var pluginInstance = pluginDescriptor.Instance(_pluginFinder.ServiceProvider);
                pluginModel.ConfigurationUrl = pluginInstance.GetConfigurationPageUrl();


                //enabled/disabled (only for some plugin types)
                if (pluginInstance is IPaymentMethod)
                {
                    //payment plugin
                    pluginModel.CanChangeEnabled = true;
                    pluginModel.IsEnabled = ((IPaymentMethod)pluginInstance).IsPaymentMethodActive(_paymentSettings);
                }
                else if (pluginInstance is IShippingRateComputationMethod)
                {
                    //shipping rate computation method
                    pluginModel.CanChangeEnabled = true;
                    pluginModel.IsEnabled = ((IShippingRateComputationMethod)pluginInstance).IsShippingRateComputationMethodActive(_shippingSettings);
                }
                else if (pluginInstance is ITaxProvider)
                {
                    //tax provider
                    pluginModel.CanChangeEnabled = true;
                    pluginModel.IsEnabled = pluginDescriptor.SystemName.Equals(_taxSettings.ActiveTaxProviderSystemName, StringComparison.OrdinalIgnoreCase);
                }
                else if (pluginInstance is IExternalAuthenticationMethod)
                {
                    //external auth method
                    pluginModel.CanChangeEnabled = true;
                    pluginModel.IsEnabled = ((IExternalAuthenticationMethod)pluginInstance).IsMethodActive(_externalAuthenticationSettings);
                }
                else if (pluginInstance is IWidgetPlugin)
                {
                    //Misc plugins
                    pluginModel.CanChangeEnabled = true;
                    pluginModel.IsEnabled = ((IWidgetPlugin)pluginInstance).IsWidgetActive(_widgetSettings);
                }

            }
            return pluginModel;
        }

        /// <summary>
        ///  Depth-first recursive delete, with handling for descendant directories open in Windows Explorer.
        /// </summary>
        /// <param name="path">Directory path</param>
        protected void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(path);

            //find more info about directory deletion
            //and why we use this approach at https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
        #endregion

        #region Methods

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new PluginListModel {
                //load modes
                AvailableLoadModes = LoadPluginsMode.All.ToSelectList(HttpContext, false).ToList()
            };
            //groups
            model.AvailableGroups.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var g in _pluginFinder.GetPluginGroups())
                model.AvailableGroups.Add(new SelectListItem { Text = g, Value = g });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ListSelect(DataSourceRequest command, PluginListModel model)
        {
            var loadMode = (LoadPluginsMode)model.SearchLoadModeId;
            var pluginDescriptors = _pluginFinder.GetPluginDescriptors(loadMode, "", model.SearchGroup).ToList();
            var items = new List<PluginModel>();
            foreach (var item in pluginDescriptors.OrderBy(x => x.Group))
            {
                items.Add(await PreparePluginModel(item, false, false));
            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = pluginDescriptors.Count()
            };
            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "install-plugin-link-")]

        public async Task<IActionResult> Install(IFormCollection form)
        {
            try
            {
                //get plugin system name
                string systemName = null;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("install-plugin-link-", StringComparison.OrdinalIgnoreCase))
                        systemName = formValue.Substring("install-plugin-link-".Length);

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is not installed
                if (pluginDescriptor.Installed)
                    return RedirectToAction("List");

                //install plugin
                var plugin = pluginDescriptor.Instance(_pluginFinder.ServiceProvider);
                await plugin.Install();
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Installed"));

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "uninstall-plugin-link-")]
        public async Task<IActionResult> Uninstall(IFormCollection form)
        {
            try
            {
                //get plugin system name
                string systemName = null;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("uninstall-plugin-link-", StringComparison.OrdinalIgnoreCase))
                        systemName = formValue.Substring("uninstall-plugin-link-".Length);

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is installed
                if (!pluginDescriptor.Installed)
                    return RedirectToAction("List");

                //uninstall plugin
                await pluginDescriptor.Instance(_pluginFinder.ServiceProvider).Uninstall();
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Uninstalled"));

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "remove-plugin-link-")]
        public IActionResult Remove(IFormCollection form)
        {
            try
            {
                //get plugin system name
                string systemName = null;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("remove-plugin-link-", StringComparison.OrdinalIgnoreCase))
                        systemName = formValue.Substring("remove-plugin-link-".Length);

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                var pluginsPath = CommonHelper.MapPath(PluginManager.PluginsPath);

                foreach (var folder in Directory.GetDirectories(pluginsPath))
                {
                    if (Path.GetFileName(folder) != "bin" && Directory.GetFiles(folder).Select(x => Path.GetFileName(x)).Contains(pluginDescriptor.PluginFileName))
                    {
                        DeleteDirectory(folder);
                    }
                }

                //uninstall plugin
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Removed"));

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            return RedirectToAction("List");
        }

        public IActionResult ReloadList()
        {
            //restart application
            _webHelper.RestartAppDomain();
            return RedirectToAction("List");
        }


        [HttpPost]
        public async Task<IActionResult> UploadPlugin(IFormFile zippedFile)
        {
            if (zippedFile == null || zippedFile.Length == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }

            string zipFilePath = "";
            PluginDescriptor descriptor = new PluginDescriptor();
            try
            {
                if (!Path.GetExtension(zippedFile.FileName)?.Equals(".zip", StringComparison.InvariantCultureIgnoreCase) ?? true)
                    throw new Exception("Only zip archives are supported");

                //ensure that temp directory is created
                var tempDirectory = CommonHelper.MapPath("~/App_Data/TempUploads");
                Directory.CreateDirectory(new DirectoryInfo(tempDirectory).FullName);

                //copy original archive to the temp directory
                zipFilePath = Path.Combine(tempDirectory, zippedFile.FileName);
                using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
                    zippedFile.CopyTo(fileStream);

                descriptor = (PluginDescriptor)UploadSingleItem(zipFilePath);

                await _customerActivityService.InsertActivity("UploadNewPlugin", "",
                           string.Format(_localizationService.GetResource("ActivityLog.UploadNewPlugin"), descriptor.FriendlyName));

                await _mediator.Publish(new PluginUploadedEvent(descriptor));

                var message = _localizationService.GetResource("Admin.Configuration.Plugins.Uploaded");
                SuccessNotification(message);
            }
            finally
            {
                //delete temporary file
                if (!string.IsNullOrEmpty(zipFilePath))
                    System.IO.File.Delete(zipFilePath);
            }

            //restart application
            _webHelper.RestartAppDomain();

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> UploadTheme(IFormFile zippedFile)
        {
            if (zippedFile == null || zippedFile.Length == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                return RedirectToAction("GeneralCommon", "Setting");
            }

            string zipFilePath = "";

            ThemeDescriptor descriptor = new ThemeDescriptor();
            try
            {
                if (!Path.GetExtension(zippedFile.FileName)?.Equals(".zip", StringComparison.InvariantCultureIgnoreCase) ?? true)
                    throw new Exception("Only zip archives are supported");

                //ensure that temp directory is created
                var tempDirectory = CommonHelper.MapPath("~/App_Data/TempUploads");
                System.IO.Directory.CreateDirectory(new DirectoryInfo(tempDirectory).FullName);

                //copy original archive to the temp directory
                zipFilePath = Path.Combine(tempDirectory, zippedFile.FileName);
                using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
                    zippedFile.CopyTo(fileStream);

                descriptor = (ThemeDescriptor)UploadSingleItem(zipFilePath);
                var configs = _themeProvider.GetThemeConfigurations();
                var b = _themeProvider.ThemeConfigurationExists(descriptor.FriendlyName);

                await _customerActivityService.InsertActivity("UploadNewTheme", "",
                           string.Format(_localizationService.GetResource("ActivityLog.UploadNewTheme"), descriptor.FriendlyName));

                await _mediator.Publish(new ThemeUploadedEvent(descriptor));

                var message = _localizationService.GetResource("Admin.Configuration.Themes.Uploaded");
                SuccessNotification(message);
            }
            catch (Exception ex)
            {
                var message = _localizationService.GetResource("Admin.Configuration.Themes.Failed");
                ErrorNotification(message + "\r\n" + ex.Message);
            }
            finally
            {
                //delete temporary file
                if (!string.IsNullOrEmpty(zipFilePath))
                    System.IO.File.Delete(zipFilePath);
            }

            return RedirectToAction("GeneralCommon", "Setting");
        }

        private IDescriptor UploadSingleItem(string archivePath)
        {
            //get path to the plugins directory
            var pluginsDirectory = CommonHelper.MapPath(PluginManager.PluginsPath);

            var uploadedItemDirectoryName = "";
            IDescriptor descriptor = null;
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                //the archive should contain only one root directory (the plugin one or the theme one)
                var rootDirectories = archive.Entries.Where(entry => entry.FullName.Count(ch => ch == '/') == 1 && entry.FullName.EndsWith("/")).ToList();
                if (rootDirectories.Count != 1)
                {
                    throw new Exception($"The archive should contain only one root plugin or theme directory. " +
                        $"For example, Payments.PayPalDirect or DefaultClean. ");
                }

                //get directory name (remove the ending /)
                uploadedItemDirectoryName = rootDirectories.First().FullName.TrimEnd('/');

                var pluginDescriptorEntry = archive.Entries.Where(x => x.FullName.Contains("Description.txt")).FirstOrDefault();
                if (pluginDescriptorEntry != null)
                {
                    using (var unzippedEntryStream = pluginDescriptorEntry.Open())
                    {
                        using (var reader = new StreamReader(unzippedEntryStream))
                        {
                            {
                                descriptor = GetPluginDescriptorFromText(reader.ReadToEnd());

                                //ensure that the plugin current version is supported
                                if (!(descriptor as PluginDescriptor).SupportedVersions.Contains(GrandVersion.SupportedPluginVersion))
                                    throw new Exception($"This plugin doesn't support the current version - {GrandVersion.SupportedPluginVersion}");
                            }
                        }
                    }
                }

                var themeDescriptorEntry = archive.Entries.Where(x => x.FullName.Contains("theme.config")).FirstOrDefault();
                if (themeDescriptorEntry != null)
                {
                    using (var unzippedEntryStream = themeDescriptorEntry.Open())
                    {
                        using (var reader = new StreamReader(unzippedEntryStream))
                        {
                            descriptor = _themeProvider.GetThemeDescriptorFromText(reader.ReadToEnd());
                        }
                    }
                }
            }

            if (descriptor == null)
                throw new Exception("No descriptor file is found. It should be in the root of the archive.");

            if (string.IsNullOrEmpty(uploadedItemDirectoryName))
                throw new Exception($"Cannot get the {(descriptor is PluginDescriptor ? "plugin" : "theme")} directory name");

            var directoryPath = descriptor is PluginDescriptor ? pluginsDirectory : CommonHelper.MapPath("~/Themes");
            var pathToUpload = Path.Combine(directoryPath, uploadedItemDirectoryName);

            //ensure it's a new directory (e.g. some old files are not required when re-uploading a plugin)
            //furthermore, zip extract functionality cannot override existing files
            //but there could deletion issues (related to file locking, etc). In such cases the directory should be deleted manually
            try
            {
                if (System.IO.Directory.Exists(pathToUpload))
                    DeleteDirectory(pathToUpload);
            }
            catch { }

            //unzip archive (pluginsDirectory instead of pathToUpload because .zip includes folder that includes plugin contents)
            ZipFile.ExtractToDirectory(archivePath, directoryPath);

            return descriptor;
        }

        public PluginDescriptor GetPluginDescriptorFromText(string text)
        {
            PluginDescriptor descriptor = new PluginDescriptor();

            if (string.IsNullOrEmpty(text))
                return descriptor;

            try
            {
                string line = text.Split("\n").Where(x => x.Contains("SupportedVersions")).First();
                var versions = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Split(",");
                for (int i = 0; i < versions.Length; i++)
                {
                    versions[i] = versions[i].Trim();
                }

                Array.ForEach(versions, x => descriptor.SupportedVersions.Add(x));
            }
            catch { }

            return descriptor;
        }

        public IActionResult ConfigureMiscPlugin(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IMiscPlugin>(systemName);
            if (descriptor == null || !descriptor.Installed)
                return Redirect("List");

            var plugin = descriptor.Instance<IMiscPlugin>(_pluginFinder.ServiceProvider);
            var model = new MiscPluginModel {
                FriendlyName = descriptor.FriendlyName,
                ConfigurationUrl = plugin.GetConfigurationPageUrl()
            };

            return View(model);
        }

        //edit
        public async Task<IActionResult> EditPopup(string systemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            var model = await PreparePluginModel(pluginDescriptor);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditPopup(string btnId, string formId, PluginModel model)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(model.SystemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //we allow editing of 'friendly name', 'display order', store mappings
                pluginDescriptor.FriendlyName = model.FriendlyName;
                pluginDescriptor.DisplayOrder = model.DisplayOrder;
                pluginDescriptor.LimitedToStores.Clear();
                if (model.LimitedToStores && model.SelectedStoreIds != null)
                {
                    pluginDescriptor.LimitedToStores = model.SelectedStoreIds.ToList();
                }
                PluginFileParser.SavePluginConfigFile(pluginDescriptor);
                //reset plugin cache
                _pluginFinder.ReloadPlugins();
                //locales
                foreach (var localized in model.Locales)
                {
                    await pluginDescriptor.Instance(_pluginFinder.ServiceProvider).SaveLocalizedFriendlyName(_localizationService, localized.LanguageId, localized.FriendlyName);
                }
                //enabled/disabled
                if (pluginDescriptor.Installed)
                {
                    var pluginInstance = pluginDescriptor.Instance(_pluginFinder.ServiceProvider);
                    //payment plugin
                    if (pluginInstance is IPaymentMethod pm)
                    {
                        if (pm.IsPaymentMethodActive(_paymentSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_paymentSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_paymentSettings);
                            }
                        }
                    }
                    else if (pluginInstance is IShippingRateComputationMethod srcm) //shipping rate computation method
                    {
                        if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_shippingSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_shippingSettings);
                            }
                        }
                    }
                    else if (pluginInstance is ITaxProvider)
                    {
                        //tax provider
                        if (model.IsEnabled)
                        {
                            _taxSettings.ActiveTaxProviderSystemName = model.SystemName;
                            await _settingService.SaveSetting(_taxSettings);
                        }
                        else
                        {
                            _taxSettings.ActiveTaxProviderSystemName = "";
                            await _settingService.SaveSetting(_taxSettings);
                        }
                    }
                    else if (pluginInstance is IExternalAuthenticationMethod eam) //external auth method
                    {
                        if (eam.IsMethodActive(_externalAuthenticationSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                    }
                    else if (pluginInstance is IWidgetPlugin widget) //Misc plugins
                    {
                        if (widget.IsWidgetActive(_widgetSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_widgetSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
                                await _settingService.SaveSetting(_widgetSettings);
                            }
                        }
                    }
                }
                await _cacheManager.Clear();
                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion
    }
}
