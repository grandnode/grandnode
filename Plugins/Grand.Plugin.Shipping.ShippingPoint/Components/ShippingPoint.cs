using Grand.Core;
using Grand.Plugin.Shipping.ShippingPoint.Models;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Grand.Plugin.Shipping.ShippingPoint.Components
{
    [ViewComponent(Name = "ShippingPoint")]
    public class SelectedShippingPointViewComponent : ViewComponent
    {
        private readonly ILocalizationService _localizationService;
        private readonly IShippingPointService _shippingPointService;
        private readonly IStoreContext _storeContext;
        private readonly ICountryService _countryService;
        private readonly IPriceFormatter _priceFormatter;

        public SelectedShippingPointViewComponent(ILocalizationService localizationService,
            IShippingPointService shippingPointService, IStoreContext storeContext,
            ICountryService countryService, IPriceFormatter priceFormatter)
        {
            this._localizationService = localizationService;
            this._shippingPointService = shippingPointService;
            this._storeContext = storeContext;
            this._countryService = countryService;
            this._priceFormatter = priceFormatter;
        }
        public IViewComponentResult Invoke(string shippingOption)
        {
            var parameter = shippingOption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (parameter == _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"))
            {
                var shippingPoints = _shippingPointService.GetAllStoreShippingPoint(_storeContext.CurrentStore.Id);

                var shippingPointsModel = new List<SelectListItem>();
                shippingPointsModel.Add(new SelectListItem() { Value = "", Text = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectShippingOption") });

                foreach (var shippingPoint in shippingPoints)
                {
                    shippingPointsModel.Add(new SelectListItem() { Value = shippingPoint.Id, Text = shippingPoint.ShippingPointName });
                }

                return View("~/Plugins/Shipping.ShippingPoint/Views/FormComboBox.cshtml", shippingPointsModel);
            }
            return Content("ShippingPointController: given Shipping Option doesn't exist");

        }
    }
}
