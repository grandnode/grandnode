using Grand.Core;
using Grand.Services.Localization;
using Grand.Framework.Controllers;
using Grand.Services.Common;
using Grand.Plugin.Shipping.ShippingPoint.Models;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Framework.Kendoui;
using Grand.Services.Security;
using Grand.Framework.Mvc;
using Grand.Services.Directory;
using Grand.Services.Stores;
using Grand.Services.Catalog;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Grand.Framework.Security.Authorization;

namespace Grand.Plugin.Shipping.ShippingPoint.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.ShippingSettings)]
    public class ShippingPointController : BaseShippingController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IShippingPointService _shippingPointService;
        private readonly ICountryService _countryService;
        private readonly IStoreService _storeService;
        private readonly IPriceFormatter _priceFormatter;

        public ShippingPointController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IShippingPointService ShippingPointService,
            ICountryService countryService,
            IStoreService storeService,
            IPriceFormatter priceFormatter
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _shippingPointService = ShippingPointService;
            _countryService = countryService;
            _storeService = storeService;
            _priceFormatter = priceFormatter;
        }

        public IActionResult Configure()
        {
            return View("~/Plugins/Shipping.ShippingPoint/Views/Configure.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var shippingPoints = await _shippingPointService.GetAllStoreShippingPoint(storeId: "", pageIndex: command.Page - 1, pageSize: command.PageSize);
            var viewModel = new List<ShippingPointModel>();

            foreach (var shippingPoint in shippingPoints)
            {
                var storeName = await _storeService.GetStoreById(shippingPoint.StoreId);
                viewModel.Add(new ShippingPointModel
                {
                    ShippingPointName = shippingPoint.ShippingPointName,
                    Description = shippingPoint.Description,
                    Id = shippingPoint.Id,
                    OpeningHours = shippingPoint.OpeningHours,
                    PickupFee = shippingPoint.PickupFee,
                    StoreName = storeName != null ? storeName.Shortcut : _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"),

                });
            }

            return Json(new DataSourceResult
            {
                Data = viewModel,
                Total = shippingPoints.TotalCount
            });
        }

        private async Task<ShippingPointModel> PrepareShippingPointModel(ShippingPointModel model)
        {
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = string.Empty });
            foreach (var country in await _countryService.GetAllCountries(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString() });
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"), Value = string.Empty });
            foreach (var store in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Shortcut, Value = store.Id.ToString() });
            return model;
        }

        public async Task<IActionResult> Create()
        {
            var model = new ShippingPointModel();
            await PrepareShippingPointModel(model);
            return View("~/Plugins/Shipping.ShippingPoint/Views/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = model.ToEntity();
                await _shippingPointService.InsertStoreShippingPoint(shippingPoint);

                ViewBag.RefreshPage = true;
            }

            await PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var shippingPoints = await _shippingPointService.GetStoreShippingPointById(id);
            var model = shippingPoints.ToModel();
            await PrepareShippingPointModel(model);
            return View("~/Plugins/Shipping.ShippingPoint/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = await _shippingPointService.GetStoreShippingPointById(model.Id);
                shippingPoint = model.ToEntity();
                await _shippingPointService.UpdateStoreShippingPoint(shippingPoint);
            }
            ViewBag.RefreshPage = true;

            await PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var model = await _shippingPointService.GetStoreShippingPointById(id);
            await _shippingPointService.DeleteStoreShippingPoint(model);

            return new NullJsonResult();
        }
 
    }
}
