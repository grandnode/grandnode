using Grand.Core;
using Grand.Core.Domain.Cms;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Services.Authentication.External;
using Grand.Services.Cms;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class PluginController : BaseAdminController
	{
		#region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
	    private readonly ISettingService _settingService;
	    private readonly IStoreService _storeService;
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
            IPermissionService permissionService, 
            ILanguageService languageService,
            ISettingService settingService, 
            IStoreService storeService,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings, 
            ExternalAuthenticationSettings externalAuthenticationSettings, 
            WidgetSettings widgetSettings)
		{
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._widgetSettings = widgetSettings;
		}

		#endregion 

        #region Utilities

        [NonAction]
        protected virtual PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, 
            bool prepareLocales = true, bool prepareStores = true)
        {
            var pluginModel = pluginDescriptor.ToModel();
            //logo
            pluginModel.LogoUrl = pluginDescriptor.GetLogoUrl(_webHelper);

            if (prepareLocales)
            {
                //locales
                AddLocales(_languageService, pluginModel.Locales, (locale, languageId) =>
                {
                    locale.FriendlyName = pluginDescriptor.Instance().GetLocalizedFriendlyName(_localizationService, languageId, false);
                });
            }
            if (prepareStores)
            {
                //stores
                pluginModel.AvailableStores = _storeService
                    .GetAllStores()
                    .Select(s => s.ToModel())
                    .ToList();
                pluginModel.SelectedStoreIds = pluginDescriptor.LimitedToStores.ToArray();
                pluginModel.LimitedToStores = pluginDescriptor.LimitedToStores.Count > 0;
            }


            //configuration URLs

            if (pluginDescriptor.Installed)
            {
                //display configuration URL only when a plugin is already installed
                var pluginInstance = pluginDescriptor.Instance();
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

        #endregion

        #region Methods

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new PluginListModel();
            //load modes
            model.AvailableLoadModes = LoadPluginsMode.All.ToSelectList(false).ToList();
            //groups
            model.AvailableGroups.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var g in _pluginFinder.GetPluginGroups())
                model.AvailableGroups.Add(new SelectListItem { Text = g, Value = g });
            return View(model);
        }

	    [HttpPost]
        public IActionResult ListSelect(DataSourceRequest command, PluginListModel model)
	    {
	        if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
	            return AccessDeniedView();

	        var loadMode = (LoadPluginsMode) model.SearchLoadModeId;
            var pluginDescriptors = _pluginFinder.GetPluginDescriptors(loadMode, "", model.SearchGroup).ToList();
	        var gridModel = new DataSourceResult
            {
                Data = pluginDescriptors.Select(x => PreparePluginModel(x, false, false))
                .OrderBy(x => x.Group)
                .ToList(),
                Total = pluginDescriptors.Count()
            };
	        return Json(gridModel);
	    }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "install-plugin-link-")]
        
        public IActionResult Install(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

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
                pluginDescriptor.Instance().Install();
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
        public IActionResult Uninstall(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

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
                pluginDescriptor.Instance().Uninstall();
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

        public IActionResult ReloadList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //restart application
            _webHelper.RestartAppDomain();
            return RedirectToAction("List");
        }
        
        public IActionResult ConfigureMiscPlugin(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();


            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IMiscPlugin>(systemName);
            if (descriptor == null || !descriptor.Installed)
                return Redirect("List");

            var plugin  = descriptor.Instance<IMiscPlugin>();
            var model = new MiscPluginModel();
            model.FriendlyName = descriptor.FriendlyName;
            model.ConfigurationUrl = plugin.GetConfigurationPageUrl();

            return View(model);
        }

        //edit
        public IActionResult EditPopup(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            var model = PreparePluginModel(pluginDescriptor);

            return View(model);
        }
        [HttpPost]
        public IActionResult EditPopup(string btnId, string formId, PluginModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

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
                PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
                //reset plugin cache
                _pluginFinder.ReloadPlugins();
                //locales
                foreach (var localized in model.Locales)
                {
                    pluginDescriptor.Instance().SaveLocalizedFriendlyName(_localizationService, localized.LanguageId, localized.FriendlyName);
                }
                //enabled/disabled
                if (pluginDescriptor.Installed)
                {
                    var pluginInstance = pluginDescriptor.Instance();
                    if (pluginInstance is IPaymentMethod)
                    {
                        //payment plugin
                        var pm = (IPaymentMethod)pluginInstance;
                        if (pm.IsPaymentMethodActive(_paymentSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_paymentSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_paymentSettings);
                            }
                        }
                    }
                    else if (pluginInstance is IShippingRateComputationMethod)
                    {
                        //shipping rate computation method
                        var srcm = (IShippingRateComputationMethod)pluginInstance;
                        if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_shippingSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_shippingSettings);
                            }
                        }
                    }
                    else if (pluginInstance is ITaxProvider)
                    {
                        //tax provider
                        if (model.IsEnabled)
                        {
                            _taxSettings.ActiveTaxProviderSystemName = model.SystemName;
                            _settingService.SaveSetting(_taxSettings);
                        }
                        else
                        {
                            _taxSettings.ActiveTaxProviderSystemName = "";
                            _settingService.SaveSetting(_taxSettings);
                        }
                    }
                    else if (pluginInstance is IExternalAuthenticationMethod)
                    {
                        //external auth method
                        var eam = (IExternalAuthenticationMethod)pluginInstance;
                        if (eam.IsMethodActive(_externalAuthenticationSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                    }
                    else if (pluginInstance is IWidgetPlugin)
                    {
                        //Misc plugins
                        var widget = (IWidgetPlugin)pluginInstance;
                        if (widget.IsWidgetActive(_widgetSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_widgetSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_widgetSettings);
                            }
                        }
                    }
                }

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
