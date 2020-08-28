using Grand.Core;
using Grand.Domain.Directory;
using Grand.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Web.Areas.Admin.Models.Shipping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ShippingSettings)]
    public partial class ShippingController : BaseAdminController
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ISettingService _settingService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Constructors

        public ShippingController(IShippingService shippingService,
            ShippingSettings shippingSettings,
            ISettingService settingService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IPluginFinder pluginFinder,
            IWebHelper webHelper,
            IStoreService storeService,
            ICustomerService customerService)
        {
            _shippingService = shippingService;
            _shippingSettings = shippingSettings;
            _settingService = settingService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _languageService = languageService;
            _pluginFinder = pluginFinder;
            _webHelper = webHelper;
            _storeService = storeService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual async Task PrepareAddressWarehouseModel(WarehouseModel model)
        {
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(model.Address.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true) : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });

            model.Address.CountryEnabled = true;
            model.Address.StateProvinceEnabled = true;
            model.Address.CityEnabled = true;
            model.Address.StreetAddressEnabled = true;
            model.Address.ZipPostalCodeEnabled = true;
            model.Address.ZipPostalCodeRequired = true;
            model.Address.PhoneEnabled = true;
            model.Address.FaxEnabled = true;
            model.Address.CompanyEnabled = true;
        }

        protected virtual async Task PreparePickupPointModel(PickupPointModel model)
        {
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(model.Address.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true) : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });

            model.Address.CountryEnabled = true;
            model.Address.StateProvinceEnabled = true;
            model.Address.CityEnabled = true;
            model.Address.StreetAddressEnabled = true;
            model.Address.ZipPostalCodeEnabled = true;
            model.Address.ZipPostalCodeRequired = true;
            model.Address.PhoneEnabled = true;
            model.Address.FaxEnabled = true;
            model.Address.CompanyEnabled = true;

            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Shipping.PickupPoint.SelectStore"), Value = "" });
            foreach (var c in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = c.Shortcut, Value = c.Id.ToString() });

            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Shipping.PickupPoint.SelectWarehouse"), Value = "" });
            foreach (var c in await _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

        }

        #endregion

        #region Shipping rate computation methods

        public IActionResult Providers() => View();

        [HttpPost]
        public IActionResult Providers(DataSourceRequest command)
        {
            var shippingProvidersModel = new List<ShippingRateComputationMethodModel>();
            var shippingProviders = _shippingService.LoadAllShippingRateComputationMethods();
            foreach (var shippingProvider in shippingProviders)
            {
                var tmp1 = shippingProvider.ToModel();
                tmp1.IsActive = shippingProvider.IsShippingRateComputationMethodActive(_shippingSettings);
                tmp1.LogoUrl = shippingProvider.PluginDescriptor.GetLogoUrl(_webHelper);
                tmp1.ConfigurationUrl = shippingProvider.GetConfigurationPageUrl();
                shippingProvidersModel.Add(tmp1);
            }
            shippingProvidersModel = shippingProvidersModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = shippingProvidersModel,
                Total = shippingProvidersModel.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProviderUpdate(ShippingRateComputationMethodModel model)
        {
            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(model.SystemName);
            if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_shippingSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_shippingSettings);
                }
            }
            var pluginDescriptor = srcm.PluginDescriptor;
            //display order
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginConfigFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        public IActionResult ConfigureProvider(string systemName)
        {
            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(systemName);
            if (srcm == null)
                //No shipping rate computation method found with the specified id
                return RedirectToAction("Providers");

            var model = srcm.ToModel();
            model.IsActive = srcm.IsShippingRateComputationMethodActive(_shippingSettings);
            model.LogoUrl = srcm.PluginDescriptor.GetLogoUrl(_webHelper);
            model.ConfigurationUrl = srcm.GetConfigurationPageUrl();

            return View(model);
        }

        #endregion

        #region Shipping methods

        public IActionResult Methods() => View();

        [HttpPost]
        public async Task<IActionResult> Methods(DataSourceRequest command)
        {
            var shippingMethodsModel = (await _shippingService.GetAllShippingMethods())
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = shippingMethodsModel,
                Total = shippingMethodsModel.Count
            };

            return Json(gridModel);
        }


        public async Task<IActionResult> CreateMethod()
        {
            var model = new ShippingMethodModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
                await _shippingService.InsertShippingMethod(sm);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Added"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> EditMethod(string id)
        {
            var sm = await _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            var model = sm.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sm.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = sm.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditMethod(ShippingMethodModel model, bool continueEditing)
        {
            var sm = await _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);
                await _shippingService.UpdateShippingMethod(sm);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Updated"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMethod(string id)
        {
            var sm = await _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            await _shippingService.DeleteShippingMethod(sm);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Deleted"));
            return RedirectToAction("Methods");
        }

        #endregion

        #region Delivery dates

        public IActionResult DeliveryDates() => View();

        [HttpPost]
        public async Task<IActionResult> DeliveryDates(DataSourceRequest command)
        {
            var deliveryDatesModel = (await _shippingService.GetAllDeliveryDates())
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = deliveryDatesModel,
                Total = deliveryDatesModel.Count
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> CreateDeliveryDate()
        {
            var model = new DeliveryDateModel
            {
                ColorSquaresRgb = "#000000"
            };
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateDeliveryDate(DeliveryDateModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var deliveryDate = model.ToEntity();
                await _shippingService.InsertDeliveryDate(deliveryDate);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Added"));
                return continueEditing ? RedirectToAction("EditDeliveryDate", new { id = deliveryDate.Id }) : RedirectToAction("DeliveryDates");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> EditDeliveryDate(string id)
        {
            var deliveryDate = await _shippingService.GetDeliveryDateById(id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");

            var model = deliveryDate.ToModel();

            if (String.IsNullOrEmpty(model.ColorSquaresRgb))
            {
                model.ColorSquaresRgb = "#000000";
            }

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = deliveryDate.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditDeliveryDate(DeliveryDateModel model, bool continueEditing)
        {
            var deliveryDate = await _shippingService.GetDeliveryDateById(model.Id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");

            if (ModelState.IsValid)
            {
                deliveryDate = model.ToEntity(deliveryDate);
                await _shippingService.UpdateDeliveryDate(deliveryDate);
                //locales
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Updated"));
                return continueEditing ? RedirectToAction("EditDeliveryDate", new { id = deliveryDate.Id }) : RedirectToAction("DeliveryDates");
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDeliveryDate(string id)
        {
            var deliveryDate = await _shippingService.GetDeliveryDateById(id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");
            if (ModelState.IsValid)
            {
                await _shippingService.DeleteDeliveryDate(deliveryDate);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Deleted"));
                return RedirectToAction("DeliveryDates");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("EditDeliveryDate", new { id = id });
        }

        #endregion

        #region Warehouses

        public IActionResult Warehouses() => View();

        [HttpPost]
        public async Task<IActionResult> Warehouses(DataSourceRequest command)
        {
            var warehousesModel = (await _shippingService.GetAllWarehouses())
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = warehousesModel,
                Total = warehousesModel.Count
            };

            return Json(gridModel);
        }
        public async Task<IActionResult> CreateWarehouse()
        {
            var model = new WarehouseModel();
            await PrepareAddressWarehouseModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateWarehouse(WarehouseModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                await _addressService.InsertAddressSettings(address);
                var warehouse = model.ToEntity();
                warehouse.AddressId = address.Id;
                await _shippingService.InsertWarehouse(warehouse);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Added"));
                return continueEditing ? RedirectToAction("EditWarehouse", new { id = warehouse.Id }) : RedirectToAction("Warehouses");
            }

            //If we got this far, something failed, redisplay form
            await PrepareAddressWarehouseModel(model);
            return View(model);
        }

        public async Task<IActionResult> EditWarehouse(string id)
        {
            var warehouse = await _shippingService.GetWarehouseById(id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            var address = await _addressService.GetAddressByIdSettings(warehouse.AddressId);
            var model = warehouse.ToModel();
            if (address != null)
            {
                model.Address = await address.ToModel(_countryService, _stateProvinceService);
            }
            await PrepareAddressWarehouseModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditWarehouse(WarehouseModel model, bool continueEditing)
        {
            var warehouse = await _shippingService.GetWarehouseById(model.Id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            if (ModelState.IsValid)
            {
                var address = await _addressService.GetAddressByIdSettings(warehouse.AddressId) ??
                    new Domain.Common.Address
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                address = model.Address.ToEntity(address);
                if (!String.IsNullOrEmpty(address.Id))
                    await _addressService.UpdateAddressSettings(address);
                else
                    await _addressService.InsertAddressSettings(address);

                warehouse = model.ToEntity(warehouse);
                await _shippingService.UpdateWarehouse(warehouse);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Updated"));
                return continueEditing ? RedirectToAction("EditWarehouse", new { id = warehouse.Id }) : RedirectToAction("Warehouses");
            }

            //If we got this far, something failed, redisplay form
            await PrepareAddressWarehouseModel(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWarehouse(string id)
        {
            var warehouse = await _shippingService.GetWarehouseById(id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            await _shippingService.DeleteWarehouse(warehouse);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.warehouses.Deleted"));
            return RedirectToAction("Warehouses");
        }

        #endregion

        #region PickupPoints

        public IActionResult PickupPoints() => View();

        [HttpPost]
        public async Task<IActionResult> PickupPoints(DataSourceRequest command)
        {
            var pickupPointsModel = (await _shippingService.GetAllPickupPoints())
                .Select(x => x.ToModel())
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = pickupPointsModel,
                Total = pickupPointsModel.Count
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> CreatePickupPoint()
        {
            var model = new PickupPointModel();
            await PreparePickupPointModel(model);
            return View(model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreatePickupPoint(PickupPointModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                await _addressService.InsertAddressSettings(address);
                var pickuppoint = model.ToEntity();
                pickuppoint.Address = address;
                await _shippingService.InsertPickupPoint(pickuppoint);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.PickupPoints.Added"));
                return continueEditing ? RedirectToAction("EditPickupPoint", new { id = pickuppoint.Id }) : RedirectToAction("PickupPoints");
            }

            //If we got this far, something failed, redisplay form
            await PreparePickupPointModel(model);
            return View(model);
        }

        public async Task<IActionResult> EditPickupPoint(string id)
        {
            var pickuppoint = await _shippingService.GetPickupPointById(id);
            if (pickuppoint == null)
                //No pickup pint found with the specified id
                return RedirectToAction("PickupPoints");

            var model = pickuppoint.ToModel();
            model.Address = await pickuppoint.Address.ToModel(_countryService, _stateProvinceService);
            await PreparePickupPointModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditPickupPoint(PickupPointModel model, bool continueEditing)
        {
            var pickupPoint = await _shippingService.GetPickupPointById(model.Id);
            if (pickupPoint == null)
                //No pickup point found with the specified id
                return RedirectToAction("PickupPoints");

            if (ModelState.IsValid)
            {
                var address = new Domain.Common.Address { CreatedOnUtc = DateTime.UtcNow };
                address = model.Address.ToEntity(address);
                pickupPoint = model.ToEntity(pickupPoint);
                pickupPoint.Address = address;
                await _shippingService.UpdatePickupPoint(pickupPoint);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.PickupPoints.Updated"));
                return continueEditing ? RedirectToAction("EditPickupPoint", new { id = pickupPoint.Id }) : RedirectToAction("PickupPoints");
            }
            //If we got this far, something failed, redisplay form
            await PreparePickupPointModel(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePickupPoint(string id)
        {
            var pickupPoint = await _shippingService.GetPickupPointById(id);
            if (pickupPoint == null)
                //No pickup point found with the specified id
                return RedirectToAction("PickupPoints");

            await _shippingService.DeletePickupPoint(pickupPoint);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.PickupPoints.Deleted"));
            return RedirectToAction("PickupPoints");
        }

        #endregion

        #region Restrictions

        public async Task<IActionResult> Restrictions()
        {
            var model = new ShippingMethodRestrictionModel();

            var countries = await _countryService.GetAllCountries(showHidden: true);
            var shippingMethods = await _shippingService.GetAllShippingMethods();
            var customerRoles = await _customerService.GetAllCustomerRoles();

            foreach (var country in countries)
            {
                model.AvailableCountries.Add(new CountryModel
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new ShippingMethodModel
                {
                    Id = sm.Id,
                    Name = sm.Name
                });
            }
            foreach (var r in customerRoles)
            {
                model.AvailableCustomerRoles.Add(new CustomerRoleModel() { Id = r.Id, Name = r.Name });
            }

            foreach (var country in countries)
            {
                foreach (var shippingMethod in shippingMethods)
                {
                    bool restricted = shippingMethod.CountryRestrictionExists(country.Id);
                    if (!model.Restricted.ContainsKey(country.Id))
                        model.Restricted[country.Id] = new Dictionary<string, bool>();
                    model.Restricted[country.Id][shippingMethod.Id] = restricted;
                }
            }

            foreach (var role in customerRoles)
            {
                foreach (var shippingMethod in shippingMethods)
                {
                    bool restricted = shippingMethod.CustomerRoleRestrictionExists(role.Id);
                    if (!model.RestictedRole.ContainsKey(role.Id))
                        model.RestictedRole[role.Id] = new Dictionary<string, bool>();
                    model.RestictedRole[role.Id][shippingMethod.Id] = restricted;
                }
            }


            return View(model);
        }

        [HttpPost, ActionName("Restrictions")]
        [RequestFormLimits(ValueCountLimit = 2048)]
        public async Task<IActionResult> RestrictionSave(IFormCollection form)
        {
            var countries = await _countryService.GetAllCountries(showHidden: true);
            var shippingMethods = await _shippingService.GetAllShippingMethods();
            var customerRoles = await _customerService.GetAllCustomerRoles();

            foreach (var shippingMethod in shippingMethods)
            {
                string formKey = "restrict_" + shippingMethod.Id;
                var countryIdsToRestrict = form[formKey].ToString() != null
                    ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToList()
                    : new List<string>();

                foreach (var country in countries)
                {

                    bool restrict = countryIdsToRestrict.Contains(country.Id);
                    if (restrict)
                    {
                        if (shippingMethod.RestrictedCountries.FirstOrDefault(c => c.Id == country.Id) == null)
                        {
                            shippingMethod.RestrictedCountries.Add(country);
                            await _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                    else
                    {
                        if (shippingMethod.RestrictedCountries.FirstOrDefault(c => c.Id == country.Id) != null)
                        {
                            shippingMethod.RestrictedCountries.Remove(shippingMethod.RestrictedCountries.FirstOrDefault(x=>x.Id ==  country.Id));
                            await _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                }

                formKey = "restrictrole_" + shippingMethod.Id;
                var roleIdsToRestrict = form[formKey].ToString() != null
                    ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToList()
                    : new List<string>();


                foreach (var role in customerRoles)
                {

                    bool restrict = roleIdsToRestrict.Contains(role.Id);
                    if (restrict)
                    {
                        if (shippingMethod.RestrictedRoles.FirstOrDefault(c => c == role.Id) == null)
                        {
                            shippingMethod.RestrictedRoles.Add(role.Id);
                            await _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                    else
                    {
                        if (shippingMethod.RestrictedRoles.FirstOrDefault(c => c == role.Id) != null)
                        {
                            shippingMethod.RestrictedRoles.Remove(role.Id);
                            await _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                }
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Restrictions.Updated"));
            //selected tab
            await SaveSelectedTabIndex();

            return RedirectToAction("Restrictions");
        }

        #endregion
    }
}
