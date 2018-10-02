using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Services.Common;
using Grand.Core.Domain.Customers;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Grand.Services.Directory;

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
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor
        public ShippingPointComputationMethod(IShippingService shippingService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ISettingService settingService,
            IShippingPointService shippingPointService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IWebHelper webHelper
            )
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
            this._priceCalculationService = priceCalculationService;
            this._settingService = settingService;
            this._shippingPointService = shippingPointService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._webHelper = webHelper;
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

        public bool HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public IList<string> ValidateShippingForm(IFormCollection form)
        {
            var shippingMethodName = form["shippingoption"].ToString().Replace("___", "_").Split(new[] { '_' })[0];
            var shippingOptionId = form["selectedShippingOption"].ToString();

            if (string.IsNullOrEmpty(shippingOptionId))
                return new List<string>() { _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed") };

            if (shippingMethodName != _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"))
                throw new ArgumentException("shippingMethodName");

            var chosenShippingOption = _shippingPointService.GetStoreShippingPointById(shippingOptionId);
            if (chosenShippingOption == null)
                return new List<string>() { _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed") };

            //override price 
            var offeredShippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
            offeredShippingOptions.Find(x => x.Name == shippingMethodName).Rate = chosenShippingOption.PickupFee;

            _genericAttributeService.SaveAttribute(
                _workContext.CurrentCustomer,
                SystemCustomerAttributeNames.OfferedShippingOptions,
                offeredShippingOptions,
                _storeContext.CurrentStore.Id);

            var forCustomer =
            string.Format("<strong>{0}:</strong> {1}<br><strong>{2}:</strong> {3}<br>",
                _localizationService.GetResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName"), chosenShippingOption.ShippingPointName,
                _localizationService.GetResource("Plugins.Shipping.ShippingPoint.Fields.Description"), chosenShippingOption.Description
            );

            _genericAttributeService.SaveAttribute(
                _workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ShippingOptionAttributeDescription,
                forCustomer,
                    _storeContext.CurrentStore.Id);

            var serializedObject = new Domain.ShippingPointSerializable()
            {
                Id = chosenShippingOption.Id,
                ShippingPointName = chosenShippingOption.ShippingPointName,
                Description = chosenShippingOption.Description,
                OpeningHours = chosenShippingOption.OpeningHours,
                PickupFee = chosenShippingOption.PickupFee,
                Country = _countryService.GetCountryById(chosenShippingOption.CountryId)?.Name,
                City = chosenShippingOption.City,
                Address1 = chosenShippingOption.Address1,
                ZipPostalCode = chosenShippingOption.ZipPostalCode,
                StoreId = chosenShippingOption.StoreId,
            };

            var stringBuilder = new StringBuilder();
            string serializedAttribute;
            using (var tw = new StringWriter(stringBuilder))
            {
                var xmlS = new XmlSerializer(typeof(Domain.ShippingPointSerializable));
                xmlS.Serialize(tw, serializedObject);
                serializedAttribute = stringBuilder.ToString();
            }

            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ShippingOptionAttributeXml,
                    serializedAttribute,
                    _storeContext.CurrentStore.Id);

            return new List<string>();
        }
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "ShippingPoint";
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ShippingPoint/Configure";
        }


        //public JsonResult GetFormPartialView(string shippingOption)
        //{
        //    throw new NotImplementedException();
        //}

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
                return null;
            }
        }

        #endregion
    }
}
