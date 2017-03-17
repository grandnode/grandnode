using System;
using System.Web.Routing;
using Grand.Core;
using Grand.Core.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Shipping.Tracking;
using System.Collections.Generic;
using Grand.Plugin.Shipping.ShippingPoint.Controllers;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Core.Domain.Orders;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class ShippingPointComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ISettingService _settingService;
        private readonly IShippingPointService _shippingPointService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public ShippingPointComputationMethod(IShippingService shippingService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ISettingService settingService,
            IShippingPointService shippingPointService,
            ILocalizationService localizationService
            )
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
            this._priceCalculationService = priceCalculationService;
            this._settingService = settingService;
            this._shippingPointService = shippingPointService;
            this._localizationService = localizationService;
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

            
            response.ShippingOptions.Add(new ShippingOption()
            {
                Name = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"),
                Description = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginDescription"),
                Rate = 0,
                ShippingRateComputationMethodSystemName = "Shipping.ShippingPoint"
            });

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
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingPoint";
            routeValues = new RouteValueDictionary { { "Namespaces", "Grand.Plugin.Shipping.ShippingPoint.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //locales       
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.PluginName", "Shipping Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.PluginDescription", "Choose a place where you can pick up your order");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName", "Point Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName.Hint", "Simple Name For Your Collection Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Description", "Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Description.Hint", "Information That Isn't Provided By Other Fields");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.PickupFee", "Pickup Fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.PickupFee.Hint", "Price For Using Given Collection Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.OpeningHours", "Open Between");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.OpeningHours.Hint", "Opening Hours");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Store", "Store Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Store.Hint", "Which Store/Stores Can Use This Collection Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.City", "City");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.City.hint", "City");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Address1", "Address 1");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Address1.hint", "Address 1");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode", "Zip postal code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode.hint", "Zip postal code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Country.hint", "Country Name");

            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.ShippingPointName", "Point Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.Address", "Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.MethodAndFee", "{0} ({1})");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.AddNew", "Add New Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredShippingPointName", "Shipping Point Name Is Required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredDescription", "Description Is Required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredOpeningHours", "Opening Hours Are Required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.SelectShippingOption", "Select Shipping Option");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.ChooseShippingPoint", "Choose Shipping Point");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed", "Select Shipping Option Before Proceed");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.PluginName");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.PluginDescription");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Description");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Descriptio.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.PickupFee");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.PickupFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.OpeningHours");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.OpeningHours.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Store.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.AddNew");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredShippingPointName");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredDescription");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.RequiredOpeningHours");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.SelectShippingOption");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.ChooseShippingPoint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed");

            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.City");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.City.hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Address1");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Address1.hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode.hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Fields.Country.hint");

            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.ShippingPointName");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.Address");
            this.DeletePluginLocaleResource("Plugins.Shipping.ShippingPoint.MethodAndFee");


            base.Uninstall();
        }

        public Type GetControllerType()
        {
            return typeof(ShippingPointController);
        }

        public bool HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
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

        #endregion
    }
}
