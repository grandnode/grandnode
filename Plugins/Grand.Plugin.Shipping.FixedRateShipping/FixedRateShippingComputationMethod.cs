using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Shipping.Tracking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Grand.Plugin.Shipping.FixedRateShipping
{
    /// <summary>
    /// Fixed rate shipping computation method
    /// </summary>
    public class FixedRateShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor
        public FixedRateShippingComputationMethod(ISettingService settingService,
            IShippingService shippingService,
            IWebHelper webHelper)
        {
            this._settingService = settingService;
            this._shippingService = shippingService;
            this._webHelper = webHelper;
        }
        #endregion

        #region Utilities

        private decimal GetRate(string shippingMethodId)
        {
            string key = string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId);
            var rate = this._settingService.GetSettingByKey<decimal>(key);
            return rate;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ShippingFixedRate/Configure";
        }

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

            string restrictByCountryId = (getShippingOptionRequest.ShippingAddress != null && !String.IsNullOrEmpty(getShippingOptionRequest.ShippingAddress.CountryId)) ? getShippingOptionRequest.ShippingAddress.CountryId : "";
            var shippingMethods = this._shippingService.GetAllShippingMethods(restrictByCountryId, getShippingOptionRequest.Customer);
            foreach (var shippingMethod in shippingMethods)
            {
                var shippingOption = new ShippingOption();
                shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                shippingOption.Rate = GetRate(shippingMethod.Id);
                response.ShippingOptions.Add(shippingOption);
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
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            string restrictByCountryId = (getShippingOptionRequest.ShippingAddress != null && !String.IsNullOrEmpty(getShippingOptionRequest.ShippingAddress.CountryId)) ? getShippingOptionRequest.ShippingAddress.CountryId : "";
            var shippingMethods = this._shippingService.GetAllShippingMethods(restrictByCountryId);

            var rates = new List<decimal>();
            foreach (var shippingMethod in shippingMethods)
            {
                decimal rate = GetRate(shippingMethod.Id);
                if (!rates.Contains(rate))
                    rates.Add(rate);
            }

            //return default rate if all of them equal
            if (rates.Count == 1)
                return rates[0];

            return null;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.FixedRateShipping.Fields.ShippingMethodName", "Shipping method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.FixedRateShipping.Fields.Rate", "Rate");

            base.Install();
        }


        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.FixedRateShipping.Fields.ShippingMethodName");
            this.DeletePluginLocaleResource("Plugins.Shipping.FixedRateShipping.Fields.Rate");

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

        public Type GetControllerType()
        {
            return typeof(Controllers.ShippingFixedRateController);
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

        public IList<string> ValidateShippingForm(IFormCollection form)
        {
            //you can implement here any validation logic
            return new List<string>();
        }

        public JsonResult GetFormPartialView(string shippingOption)
        {
            //you can use here any view 
            return new JsonResult("");
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "";
        }

        #endregion
    }
}