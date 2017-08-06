using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Shipping;
using System;
using Grand.Core.Domain.Orders;
using Grand.Services.Shipping.Tracking;
using System.Collections.Generic;
using Grand.Services.Configuration;
using Grand.Core.Domain.Shipping;
using Grand.Services.Localization;
using Grand.Plugin.Shipping.ByWeight.Services;
using Grand.Services.Catalog;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ShippingByWeightSettings shippingByWeightSettings,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
            this._priceCalculationService = priceCalculationService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }
        #endregion

        #region Utilities

        private decimal? GetRate(decimal subTotal, decimal weight, string shippingMethodId,
            string storeId, string warehouseId, string countryId, string stateProvinceId, string zip)
        {

            var shippingByWeightService = EngineContext.Current.Resolve<IShippingByWeightService>();
            var shippingByWeightSettings = EngineContext.Current.Resolve<ShippingByWeightSettings>();

            var shippingByWeightRecord = shippingByWeightService.FindRecord(shippingMethodId,
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
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
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
                subTotal += _priceCalculationService.GetSubTotal(packageItem.ShoppingCartItem);
            }
            decimal weight = _shippingService.GetTotalWeight(getShippingOptionRequest);

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, weight, shippingMethod.Id,
                    storeId, warehouseId, countryId, stateProvinceId, zip);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
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
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }

        /// <summary>
        /// Returns a value indicating whether shipping methods should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this shipping methods if all products in the cart are downloadable
            //or hide this shipping methods if current customer is from certain country
            return false;
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
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
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

        public IList<string> ValidateShippingForm(IFormCollection form)
        {
            //you can implement here any validation logic
            return new List<string>();
        }


        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "";
        }

        IShipmentTracker IShippingRateComputationMethod.ShipmentTracker => throw new NotImplementedException();

        #endregion
    }

}
