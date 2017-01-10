using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Payments;
using Grand.Core;
using Grand.Core.Domain.Payments;
using Grand.Core.Plugins;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Services.Customers;
using Grand.Services.Shipping;

namespace Grand.Admin.Controllers
{
    public partial class PaymentController : BaseAdminController
	{
		#region Fields

        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
	    private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IShippingService _shippingService;
        private readonly IPluginFinder _pluginFinder;
	    private readonly IWebHelper _webHelper;
	    private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public PaymentController(IPaymentService paymentService,
            PaymentSettings paymentSettings,
            ISettingService settingService, 
            IPermissionService permissionService,
            ICountryService countryService,
            ICustomerService customerService,
            IShippingService shippingService,
            IPluginFinder pluginFinder,
            IWebHelper webHelper,
            ILocalizationService localizationService)
		{
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._countryService = countryService;
            this._customerService = customerService;
            this._shippingService = shippingService;
            this._pluginFinder = pluginFinder;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public ActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult Methods(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var paymentMethodsModel = new List<PaymentMethodModel>();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            foreach (var paymentMethod in paymentMethods)
            {
                var tmp1 = paymentMethod.ToModel();
                tmp1.IsActive = paymentMethod.IsPaymentMethodActive(_paymentSettings);
                tmp1.LogoUrl = paymentMethod.PluginDescriptor.GetLogoUrl(_webHelper);
                paymentMethodsModel.Add(tmp1);
            }
            paymentMethodsModel = paymentMethodsModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = paymentMethodsModel,
                Total = paymentMethodsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult MethodUpdate([Bind(Exclude = "ConfigurationRouteValues")] PaymentMethodModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var pm = _paymentService.LoadPaymentMethodBySystemName(model.SystemName);
            if (pm.IsPaymentMethodActive(_paymentSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_paymentSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_paymentSettings);
                }
            }
            var pluginDescriptor = pm.PluginDescriptor;
            pluginDescriptor.FriendlyName = model.FriendlyName;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        public ActionResult ConfigureMethod(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);
            if (pm == null)
                //No payment method found with the specified id
                return RedirectToAction("Methods");

            var model = pm.ToModel();
            string actionName, controllerName;
            RouteValueDictionary routeValues;
            pm.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        public ActionResult MethodRestrictions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new PaymentMethodRestrictionModel();

            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            var countries = _countryService.GetAllCountries(showHidden: true);
            var customerroles = _customerService.GetAllCustomerRoles(showHidden: true);
            var shippings = _shippingService.GetAllShippingMethods();

            foreach (var pm in paymentMethods)
            {
                model.AvailablePaymentMethods.Add(pm.ToModel());
            }
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(c.ToModel());
            }
            foreach (var r in customerroles)
            {
                model.AvailableCustomerRoles.Add(r.ToModel());
            }
            foreach (var s in shippings)
            {
                model.AvailableShippingMethods.Add(new Models.Shipping.ShippingMethodModel() {
                     Id = s.Id,
                     Name = s.Name
                });
            }

            foreach (var pm in paymentMethods)
            {
                var restictedCountries = _paymentService.GetRestrictedCountryIds(pm);
                foreach (var c in countries)
                {
                    bool resticted = restictedCountries.Contains(c.Id);
                    if (!model.Resticted.ContainsKey(pm.PluginDescriptor.SystemName))
                        model.Resticted[pm.PluginDescriptor.SystemName] = new Dictionary<string, bool>();
                    model.Resticted[pm.PluginDescriptor.SystemName][c.Id] = resticted;
                }

                var restictedRoles = _paymentService.GetRestrictedRoleIds(pm);
                foreach (var r in customerroles)
                {
                    bool resticted = restictedRoles.Contains(r.Id);
                    if (!model.RestictedRole.ContainsKey(pm.PluginDescriptor.SystemName))
                        model.RestictedRole[pm.PluginDescriptor.SystemName] = new Dictionary<string, bool>();
                    model.RestictedRole[pm.PluginDescriptor.SystemName][r.Id] = resticted;
                }

                var restictedShipping = _paymentService.GetRestrictedShippingIds(pm);
                foreach (var s in shippings)
                {
                    bool resticted = restictedShipping.Contains(s.Name);
                    if (!model.RestictedShipping.ContainsKey(pm.PluginDescriptor.SystemName))
                        model.RestictedShipping[pm.PluginDescriptor.SystemName] = new Dictionary<string, bool>();
                    model.RestictedShipping[pm.PluginDescriptor.SystemName][s.Name] = resticted;
                }

            }

            return View(model);
        }

        [HttpPost, ActionName("MethodRestrictions")]
        public ActionResult MethodRestrictionsSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            var countries = _countryService.GetAllCountries(showHidden: true);
            var customerroles = _customerService.GetAllCustomerRoles(showHidden: true);
            var shippings = _shippingService.GetAllShippingMethods();

            foreach (var pm in paymentMethods)
            {
                string formKey = "restrict_" + pm.PluginDescriptor.SystemName;
                var countryIdsToRestrict = (form[formKey] != null ? form[formKey].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newCountryIds = new List<string>();
                foreach (var c in countries)
                {
                    if (countryIdsToRestrict.Contains(c.Id))
                    {
                        newCountryIds.Add(c.Id);
                    }
                }
                _paymentService.SaveRestictedCountryIds(pm, newCountryIds);


                formKey = "restrictrole_" + pm.PluginDescriptor.SystemName;
                var roleIdsToRestrict = (form[formKey] != null ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newRoleIds = new List<string>();
                foreach (var r in customerroles)
                {
                    if (roleIdsToRestrict.Contains(r.Id))
                    {
                        newRoleIds.Add(r.Id);
                    }
                }
                _paymentService.SaveRestictedRoleIds(pm, newRoleIds);

                formKey = "restrictship_" + pm.PluginDescriptor.SystemName;
                var shipIdsToRestrict = (form[formKey] != null ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newShipIds = new List<string>();
                foreach (var s in shippings)
                {
                    if (shipIdsToRestrict.Contains(s.Name))
                    {
                        newShipIds.Add(s.Name);
                    }
                }
                _paymentService.SaveRestictedShippingIds(pm, newShipIds);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Payment.MethodRestrictions.Updated"));
            //selected tab
            SaveSelectedTabIndex();
            return RedirectToAction("MethodRestrictions");
        }

        #endregion
    }
}
