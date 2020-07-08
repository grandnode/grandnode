using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Shipping.Tracking;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class ShippingPointComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IShippingPointService _shippingPointService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IWebHelper _webHelper;
        private readonly ILanguageService _languageService;
        #endregion

        #region Ctor
        public ShippingPointComputationMethod(IStoreContext storeContext,
            IShippingPointService shippingPointService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IWebHelper webHelper,
            ILanguageService languageService
            )
        {
            _storeContext = storeContext;
            _shippingPointService = shippingPointService;
            _localizationService = localizationService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _countryService = countryService;
            _webHelper = webHelper;
            _languageService = languageService;
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


            response.ShippingOptions.Add(new ShippingOption()
            {
                Name = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"),
                Description = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginDescription"),
                Rate = 0,
                ShippingRateComputationMethodSystemName = "Shipping.ShippingPoint"
            });

            return await Task.FromResult(response);
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public Task<decimal?> GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return Task.FromResult(default(decimal?));
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task Install()
        {
            //locales       
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.PluginName", "Shipping Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.PluginDescription", "Choose a place where you can pick up your order");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ShippingPointName", "Point Name");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ShippingPointName.Hint", "Simple Name For Your Collection Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Description", "Description");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Description.Hint", "Information That Isn't Provided By Other Fields");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.PickupFee", "Pickup Fee");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.PickupFee.Hint", "Price For Using Given Collection Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.OpeningHours", "Open Between");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.OpeningHours.Hint", "Opening Hours");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Store", "Store Name");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Store.Hint", "Which Store/Stores Can Use This Collection Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.City", "City");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.City.hint", "City");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Address1", "Address 1");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Address1.hint", "Address 1");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode", "Zip postal code");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode.hint", "Zip postal code");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Country", "Country");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Country.hint", "Country Name");

            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.ShippingPointName", "Point Name");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Address", "Address");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.MethodAndFee", "{0} ({1})");

            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.AddNew", "Add New Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredShippingPointName", "Shipping Point Name Is Required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredDescription", "Description Is Required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredOpeningHours", "Opening Hours Are Required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.SelectShippingOption", "Select Shipping Option");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.ChooseShippingPoint", "Choose Shipping Point");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.SelectBeforeProceed", "Select Shipping Option Before Proceed");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.PluginName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.PluginDescription");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ShippingPointName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ShippingPointName.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Description");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Descriptio.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.PickupFee");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.PickupFee.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.OpeningHours");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.OpeningHours.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Store");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Store.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.AddNew");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredShippingPointName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredDescription");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.RequiredOpeningHours");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.SelectShippingOption");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.ChooseShippingPoint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.SelectBeforeProceed");

            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.City");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.City.hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Address1");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Address1.hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.ZipPostalCode.hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Country");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Fields.Country.hint");

            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.ShippingPointName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.Address");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Shipping.ShippingPoint.MethodAndFee");


            await base.Uninstall();
        }

        public async Task<bool> HideShipmentMethods(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(false);
        }

        public async Task<IList<string>> ValidateShippingForm(IFormCollection form)
        {
            var shippingMethodName = form["shippingoption"].ToString().Replace("___", "_").Split(new[] { '_' })[0];
            var shippingOptionId = form["selectedShippingOption"].ToString();

            if (string.IsNullOrEmpty(shippingOptionId))
                return new List<string>() { _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed") };

            if (shippingMethodName != _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"))
                throw new ArgumentException("shippingMethodName");

            var chosenShippingOption = await _shippingPointService.GetStoreShippingPointById(shippingOptionId);
            if (chosenShippingOption == null)
                return new List<string>() { _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectBeforeProceed") };

            //override price 
            var offeredShippingOptions = await _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(_genericAttributeService, SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
            offeredShippingOptions.Find(x => x.Name == shippingMethodName).Rate = chosenShippingOption.PickupFee;

            await _genericAttributeService.SaveAttribute(
                _workContext.CurrentCustomer,
                SystemCustomerAttributeNames.OfferedShippingOptions,
                offeredShippingOptions,
                _storeContext.CurrentStore.Id);

            var forCustomer =
            string.Format("<strong>{0}:</strong> {1}<br><strong>{2}:</strong> {3}<br>",
                _localizationService.GetResource("Plugins.Shipping.ShippingPoint.Fields.ShippingPointName"), chosenShippingOption.ShippingPointName,
                _localizationService.GetResource("Plugins.Shipping.ShippingPoint.Fields.Description"), chosenShippingOption.Description
            );

            await _genericAttributeService.SaveAttribute(
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
                Country = (await _countryService.GetCountryById(chosenShippingOption.CountryId))?.Name,
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

            await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
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
