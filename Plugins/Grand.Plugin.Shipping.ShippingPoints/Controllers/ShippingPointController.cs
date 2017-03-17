using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Core;
using Grand.Services.Localization;
using Grand.Web.Framework.Controllers;
using Grand.Services.Common;
using Grand.Plugin.Shipping.ShippingPoint.Models;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using System;
using Grand.Web.Framework.Kendoui;
using Grand.Services.Security;
using Grand.Web.Framework.Mvc;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Services.Directory;
using Grand.Services.Stores;
using System.IO;
using System.Xml.Serialization;
using Grand.Core.Domain.Shipping;
using System.Text;
using System.Linq;
using Grand.Services.Catalog;

namespace Grand.Plugin.Shipping.ShippingPoint.Controllers
{
    [AdminAuthorize]
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

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //little hack here
            //always set culture to 'en-US' (Telerik has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            CommonHelper.SetTelerikCulture();

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            return View("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/Configure.cshtml");
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
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
            }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Create()
        {
            var model = new ShippingPointModel();
            return View("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/Create.cshtml", PrepareShippingPointModel(model));
        }

        [HttpPost]
        public ActionResult Create(string btnId, string formId, ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = model.ToEntity();
                _shippingPointService.InsertStoreShippingPoint(shippingPoint);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
            }

            PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/Create.cshtml", model);
        }

        public ActionResult Edit(string id)
        {
            var shippingPoints = _shippingPointService.GetStoreShippingPointById(id);
            var model = shippingPoints.ToModel();
            PrepareShippingPointModel(model);
            return View("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/Edit.cshtml", model);
        }

        [HttpPost]
        public ActionResult Edit(string btnId, string formId, ShippingPointModel model)
        {
            if (ModelState.IsValid)
            {
                var shippingPoint = _shippingPointService.GetStoreShippingPointById(model.Id);
                shippingPoint = model.ToEntity();
                _shippingPointService.UpdateStoreShippingPoint(shippingPoint);
            }
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            PrepareShippingPointModel(model);

            return View("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/Edit.cshtml", model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var model = _shippingPointService.GetStoreShippingPointById(id);
            _shippingPointService.DeleteStoreShippingPoint(model);

            return new NullJsonResult();
        }

        public override JsonResult GetFormPartialView(string shippingMethod)
        {
            var parameter = shippingMethod.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (parameter == _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"))
            {
                var shippingPoints = _shippingPointService.GetAllStoreShippingPoint(_storeContext.CurrentStore.Id);          

                var shippingPointsModel = new List<SelectListItem>();
                shippingPointsModel.Add(new SelectListItem() { Value = "", Text = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectShippingOption") });

                foreach(var shippingPoint in shippingPoints)
                {
                    shippingPointsModel.Add(new SelectListItem() { Value = shippingPoint.Id, Text = shippingPoint.ShippingPointName });
                }

                return Json(this.RenderPartialViewToString("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/FormComboBox.cshtml", shippingPointsModel), JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "ShippingPointController: given Shipping Option doesn't exist" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShippingOptionPartialView(string shippingOptionId)
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
                return Json(this.RenderPartialViewToString("~/Plugins/Shipping.ShippingPoint/Views/ShippingPoint/FormShippingOption.cshtml", viewModel), JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "ShippingPointController: given Shipping Option doesn't exist" }, JsonRequestBehavior.AllowGet);
        }

        public override IList<string> ValidateShippingForm(FormCollection form)
        {
            var shippingMethodName = form.GetValue("shippingoption").AttemptedValue.Replace("___", "_").Split(new[] { '_' })[0];
            var shippingOptionId = form.GetValue("selectedShippingOption")?.AttemptedValue;

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
    }
}
