using Grand.Core;
using Grand.Plugin.Shipping.ShippingPoint.Models;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Shipping.ShippingPoint.Controllers
{
    public class SelectedShippingPointController : Controller
    {
        private readonly ILocalizationService _localizationService;
        private readonly IShippingPointService _shippingPointService;
        private readonly IStoreContext _storeContext;
        private readonly ICountryService _countryService;
        private readonly IPriceFormatter _priceFormatter;

        public SelectedShippingPointController(ILocalizationService localizationService,
            IShippingPointService shippingPointService, IStoreContext storeContext,
            ICountryService countryService, IPriceFormatter priceFormatter)
        {
            this._localizationService = localizationService;
            this._shippingPointService = shippingPointService;
            this._storeContext = storeContext;
            this._countryService = countryService;
            this._priceFormatter = priceFormatter;
        }
        public IActionResult Get(string shippingOptionId)
        {
            var shippingPoint = _shippingPointService.GetStoreShippingPointById(shippingOptionId);
            if (shippingPoint != null)
            {
                var countryName = _countryService.GetCountryById(shippingPoint.CountryId);

                var viewModel = new PointModel()
                {
                    ShippingPointName = shippingPoint.ShippingPointName,
                    Description = shippingPoint.Description,
                    PickupFee = _priceFormatter.FormatShippingPrice(shippingPoint.PickupFee, true),
                    OpeningHours = shippingPoint.OpeningHours,
                    Address1 = shippingPoint.Address1,
                    City = shippingPoint.City,
                    CountryName = _countryService.GetCountryById(shippingPoint.CountryId)?.Name,
                    ZipPostalCode = shippingPoint.ZipPostalCode,
                };
                return View("~/Plugins/Shipping.ShippingPoint/Views/FormShippingOption.cshtml", viewModel);
            }
            return Content("ShippingPointController: given Shipping Option doesn't exist");
        }
    }
}
