using Grand.Core;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Plugin.Shipping.ByWeight.Services;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Shipping.Tracking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Shipping.ByWeight
{
    public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IWorkContext workContext,
            ILocalizationService localizationService, 
            ILanguageService languageService,
            IServiceProvider serviceProvider)
        {
            _shippingService = shippingService;
            _storeContext = storeContext;
            _priceCalculationService = priceCalculationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _workContext = workContext;
            _localizationService = localizationService;
            _languageService = languageService;
            _serviceProvider = serviceProvider;
        }
        #endregion

        #region Utilities

        private async Task<decimal?> GetRate(decimal subTotal, decimal weight, string shippingMethodId,
            string storeId, string warehouseId, string countryId, string stateProvinceId, string zip)
        {

            var shippingByWeightService = _serviceProvider.GetRequiredService<IShippingByWeightService>();
            var shippingByWeightSettings = _serviceProvider.GetRequiredService<ShippingByWeightSettings>();

            var shippingByWeightRecord = await shippingByWeightService.FindRecord(shippingMethodId,
                storeId, warehouseId, countryId, stateProvinceId, zip, weight);
            if (shippingByWeightRecord == null)
            {
                if (shippingByWeightSettings.LimitMethodsToCreated)
                    return null;

                return decimal.Zero;
            }

            //additional fixed cost
            decimal shippingTotal = shippingByWeightRecord.AdditionalFixedCost;
            //charge amount per weight unit
            if (shippingByWeightRecord.RatePerWeightUnit > decimal.Zero)
            {
                var weightRate = weight - shippingByWeightRecord.LowerWeightLimit;
                if (weightRate < decimal.Zero)
                    weightRate = decimal.Zero;
                shippingTotal += shippingByWeightRecord.RatePerWeightUnit * weightRate;
            }
            //percentage rate of subtotal
            if (shippingByWeightRecord.PercentageRateOfSubtotal > decimal.Zero)
            {
                shippingTotal += Math.Round((decimal)((((float)subTotal) * ((float)shippingByWeightRecord.PercentageRateOfSubtotal)) / 100f), 2);
            }

            if (shippingTotal < decimal.Zero)
                shippingTotal = decimal.Zero;
            return shippingTotal;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public async Task<GetShippingOptionResponse> GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            var storeId = getShippingOptionRequest.StoreId;
            if (String.IsNullOrEmpty(storeId))
                storeId = _storeContext.CurrentStore.Id;
            string countryId = getShippingOptionRequest.ShippingAddress.CountryId;
            string stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId;
            string warehouseId = getShippingOptionRequest.WarehouseFrom != null ? getShippingOptionRequest.WarehouseFrom.Id : "";
            string zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            decimal subTotal = decimal.Zero;
            foreach (var packageItem in getShippingOptionRequest.Items)
            {
                if (packageItem.ShoppingCartItem.IsFreeShipping)
                    continue;
                //TODO we should use getShippingOptionRequest.Items.GetQuantity() method to get subtotal
                subTotal += (await _priceCalculationService.GetSubTotal(packageItem.ShoppingCartItem)).subTotal;
            }
            decimal weight = await _shippingService.GetTotalWeight(getShippingOptionRequest);

            var shippingMethods = await _shippingService.GetAllShippingMethods(countryId, _workContext.CurrentCustomer);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = await GetRate(subTotal, weight, shippingMethod.Id,
                    storeId, warehouseId, countryId, stateProvinceId, zip);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id);
                    shippingOption.Rate = rate.Value;
                    response.ShippingOptions.Add(shippingOption);
                }
            }


            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public async Task<decimal?> GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return await Task.FromResult(default(decimal?));
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task Install()
        {
            //settings
            var settings = new ShippingByWeightSettings
            {
                LimitMethodsToCreated = false,
            };
            await _settingService.SaveSetting(settings);

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Store", "Store");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Warehouse", "Warehouse");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Warehouse.Hint", "If an asterisk is selected, then this shipping rate will apply to all warehouses.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Country", "Country");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.StateProvince", "State / province");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Zip", "Zip");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.ShippingMethod", "Shipping method");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.ShippingMethod.Hint", "The shipping method.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.From", "Order weight from");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.From.Hint", "Order weight from.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.To", "Order weight to");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.To.Hint", "Order weight to.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.AdditionalFixedCost", "Additional fixed cost");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.AdditionalFixedCost.Hint", "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.LowerWeightLimit", "Lower weight limit");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.LowerWeightLimit.Hint", "Lower weight limit. This field can be used for \"per extra weight unit\" scenarios.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.PercentageRateOfSubtotal", "Charge percentage (of subtotal)");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.PercentageRateOfSubtotal.Hint", "Charge percentage (of subtotal).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.RatePerWeightUnit", "Rate per weight unit");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.RatePerWeightUnit.Hint", "Rate per weight unit.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they've not configured here (zero shipping fee in this case).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Fields.DataHtml", "Data");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.AddRecord", "Add record");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Formula", "Formula to calculate rates");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ByWeight.Formula.Value", "[additional fixed cost] + ([order total weight] - [lower weight limit]) * [rate per weight unit] + [order subtotal] * [charge percentage]");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            await base.Uninstall();
        }

        /// <summary>
        /// Returns a value indicating whether shipping methods should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public async Task<bool> HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this shipping methods if all products in the cart are downloadable
            //or hide this shipping methods if current customer is from certain country
            return await Task.FromResult(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }


        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                return null;
            }
        }
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ShippingByWeight/Configure";
        }

        public async Task<IList<string>> ValidateShippingForm(IFormCollection form)
        {
            //you can implement here any validation logic
            return await Task.FromResult(new List<string>());
        }


        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "";
        }

        #endregion
    }

}
