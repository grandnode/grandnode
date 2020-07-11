using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Shipping;
using Grand.Services.Shipping.Tracking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Shipping
{
    public class FixedRateTestShippingRateComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        private decimal GetRate()
        {
            decimal rate = 10M;
            return rate;
        }

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
            response.ShippingOptions.Add(new ShippingOption
            {
                Name = "Shipping option 1",
                Description = "",
                Rate = GetRate()
            });
            response.ShippingOptions.Add(new ShippingOption
            {
                Name = "Shipping option 2",
                Description = "",
                Rate = GetRate()
            });

            return await Task.FromResult(response);
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public async Task<decimal?> GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            return await Task.FromResult(GetRate());
        }

        /// <summary>
        /// Returns a value indicating whether shipping methods should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public async Task<bool> HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(false);
        }

        public Type GetControllerType()
        {
            return typeof(Controller);
        }

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
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }

        public async Task<IList<string>> ValidateShippingForm(IFormCollection form)
        {
            return await Task.FromResult(new List<string>());
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "";
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get { return null; }
        }
        #endregion
    }
}
