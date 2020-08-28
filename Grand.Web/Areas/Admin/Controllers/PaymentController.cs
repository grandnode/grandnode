using Grand.Core;
using Grand.Domain.Payments;
using Grand.Core.Plugins;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Security.Authorization;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.PaymentMethods)]
    public partial class PaymentController : BaseAdminController
	{
		#region Fields

        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ISettingService _settingService;
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
            ICountryService countryService,
            ICustomerService customerService,
            IShippingService shippingService,
            IPluginFinder pluginFinder,
            IWebHelper webHelper,
            ILocalizationService localizationService)
		{
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _settingService = settingService;
            _countryService = countryService;
            _customerService = customerService;
            _shippingService = shippingService;
            _pluginFinder = pluginFinder;
            _webHelper = webHelper;
            _localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public IActionResult Methods() => View();

        [HttpPost]
        public async Task<IActionResult> Methods(DataSourceRequest command)
        {
            var paymentMethodsModel = new List<PaymentMethodModel>();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            foreach (var paymentMethod in paymentMethods)
            {
                var tmp1 = await paymentMethod.ToModel();
                tmp1.IsActive = paymentMethod.IsPaymentMethodActive(_paymentSettings);
                tmp1.LogoUrl = paymentMethod.PluginDescriptor.GetLogoUrl(_webHelper);
                tmp1.ConfigurationUrl = paymentMethod.GetConfigurationPageUrl();
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
        public async Task<IActionResult> MethodUpdate( PaymentMethodModel model)
        {
            var pm = _paymentService.LoadPaymentMethodBySystemName(model.SystemName);
            if (pm.IsPaymentMethodActive(_paymentSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_paymentSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_paymentSettings);
                }
            }
            var pluginDescriptor = pm.PluginDescriptor;
            pluginDescriptor.FriendlyName = model.FriendlyName;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginConfigFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        public async Task<IActionResult> ConfigureMethod(string systemName)
        {
            var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);
            if (pm == null)
                //No payment method found with the specified id
                return RedirectToAction("Methods");

            var model = await pm.ToModel();
            model.LogoUrl = pm.PluginDescriptor.GetLogoUrl(_webHelper);
            model.ConfigurationUrl = pm.GetConfigurationPageUrl();

            return View(model);
        }

        public async Task<IActionResult> MethodRestrictions()
        {
            var model = new PaymentMethodRestrictionModel();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            var countries = await _countryService.GetAllCountries(showHidden: true);
            var customerroles = await _customerService.GetAllCustomerRoles(showHidden: true);
            var shippings = await _shippingService.GetAllShippingMethods();

            foreach (var pm in paymentMethods)
            {
                model.AvailablePaymentMethods.Add(await pm.ToModel());
            }
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(c.ToModel());
            }
            foreach (var r in customerroles)
            {
                model.AvailableCustomerRoles.Add(new CustomerRoleModel() { Id  = r.Id, Name = r.Name });
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
        [RequestFormLimits(ValueCountLimit = 2048)]
        public async Task<IActionResult> MethodRestrictionsSave(IFormCollection form)
        {
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            var countries = await _countryService.GetAllCountries(showHidden: true);
            var customerroles = await _customerService.GetAllCustomerRoles(showHidden: true);
            var shippings = await _shippingService.GetAllShippingMethods();

            foreach (var pm in paymentMethods)
            {
                string formKey = "restrict_" + pm.PluginDescriptor.SystemName;
                var countryIdsToRestrict = (form[formKey].ToString() != null ? form[formKey].ToString().Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newCountryIds = new List<string>();
                foreach (var c in countries)
                {
                    if (countryIdsToRestrict.Contains(c.Id))
                    {
                        newCountryIds.Add(c.Id);
                    }
                }
                await _paymentService.SaveRestictedCountryIds(pm, newCountryIds);

                formKey = "restrictrole_" + pm.PluginDescriptor.SystemName;
                var roleIdsToRestrict = (form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newRoleIds = new List<string>();
                foreach (var r in customerroles)
                {
                    if (roleIdsToRestrict.Contains(r.Id))
                    {
                        newRoleIds.Add(r.Id);
                    }
                }
                await _paymentService.SaveRestictedRoleIds(pm, newRoleIds);

                formKey = "restrictship_" + pm.PluginDescriptor.SystemName;
                var shipIdsToRestrict = (form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .Select(x => x).ToList();

                var newShipIds = new List<string>();
                foreach (var s in shippings)
                {
                    if (shipIdsToRestrict.Contains(s.Name))
                    {
                        newShipIds.Add(s.Name);
                    }
                }
                await _paymentService.SaveRestictedShippingIds(pm, newShipIds);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Payment.MethodRestrictions.Updated"));
            //selected tab
            await SaveSelectedTabIndex();
            return RedirectToAction("MethodRestrictions");
        }
        #endregion
    }
}
