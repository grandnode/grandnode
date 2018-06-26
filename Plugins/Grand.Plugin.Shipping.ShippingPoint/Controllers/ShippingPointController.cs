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

namespace Grand.Plugin.Shipping.ShippingPoint.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class ShippingPointController : BaseShippingController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IShippingPointService _shippingPointService;
        private readonly ICountryService _countryService;
        private readonly IStoreService _storeService;
        private readonly IPriceFormatter _priceFormatter;

        public ShippingPointController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IShippingPointService ShippingPointService,
            ICountryService countryService,
            IStoreService storeService,
            IPriceFormatter priceFormatter
            )
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._shippingPointService = ShippingPointService;
            this._countryService = countryService;
            this._storeService = storeService;
            this._priceFormatter = priceFormatter;
        }

        public IActionResult Configure()
        {
            return View("~/Plugins/Shipping.ShippingPoint/Views/Configure.cshtml");
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var shippingPoints = _shippingPointService.GetAllStoreShippingPoint(storeId: "", pageIndex: command.Page - 1, pageSize: command.PageSize);
            var viewModel = new List<ShippingPointModel>();

            foreach (var shippingPoint in shippingPoints)
            {
                var storeName = _storeService.GetStoreById(shippingPoint.StoreId);
                viewModel.Add(new ShippingPointModel
                {
                    ShippingPointName = shippingPoint.ShippingPointName,
                    Description = shippingPoint.Description,
                    Id = shippingPoint.Id,
                    OpeningHours = shippingPoint.OpeningHours,
                    PickupFee = shippingPoint.PickupFee,
                    StoreName = storeName != null ? storeName.Name : _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"),

                });
            }

            return Json(new DataSourceResult
            {
                Data = viewModel,
                Total = shippingPoints.TotalCount
            });
        }

        private ShippingPointModel PrepareShippingPointModel(ShippingPointModel model)
        {
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = string.Empty });
            foreach (var country in _countryService.GetAllCountries(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString() });
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"), Value = string.Empty });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            return model;
        }

        public IActionResult Create()
        {
            var model = new ShippingPointModel();
            return View("~/Plugins/Shipping.ShippingPoint/Views/Create.cshtml", PrepareShippingPointModel(model));
        }

        [HttpPost]
        public IActionResult Create(ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = model.ToEntity();
                _shippingPointService.InsertStoreShippingPoint(shippingPoint);

                ViewBag.RefreshPage = true;
            }

            PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/Create.cshtml", model);
        }

        public IActionResult Edit(string id)
        {
            var shippingPoints = _shippingPointService.GetStoreShippingPointById(id);
            var model = shippingPoints.ToModel();
            PrepareShippingPointModel(model);
            return View("~/Plugins/Shipping.ShippingPoint/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = _shippingPointService.GetStoreShippingPointById(model.Id);
                shippingPoint = model.ToEntity();
                _shippingPointService.UpdateStoreShippingPoint(shippingPoint);
            }
            ViewBag.RefreshPage = true;

            PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var model = _shippingPointService.GetStoreShippingPointById(id);
            _shippingPointService.DeleteStoreShippingPoint(model);

            return new NullJsonResult();
        }
 
    }
}
