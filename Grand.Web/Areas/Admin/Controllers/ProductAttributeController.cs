using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;
using System;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ProductAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;

        #endregion Fields

        #region Constructors

        public ProductAttributeController(IProductService productService,
            IProductAttributeService productAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService)
        {
            this._productService = productService;
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                if(!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });

                if (!(String.IsNullOrEmpty(local.Description)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Description",
                        LocaleValue = local.Description,
                    });

            }
            return localized;

        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(PredefinedProductAttributeValue ppav, PredefinedProductAttributeValueModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });
            }
            return localized;
        }

        #endregion

        #region Methods

        #region Attribute list / create / edit / delete

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttributes = _productAttributeService
                .GetAllProductAttributes(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productAttributes.Select(x => x.ToModel()),
                Total = productAttributes.TotalCount
            };

            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var model = new ProductAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productAttribute = model.ToEntity();
                productAttribute.Locales = UpdateLocales(productAttribute, model);
                _productAttributeService.InsertProductAttribute(productAttribute);

                //activity log
                _customerActivityService.InsertActivity("AddNewProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = productAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            var model = productAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = productAttribute.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                productAttribute = model.ToEntity(productAttribute);
                productAttribute.Locales = UpdateLocales(productAttribute, model);
                _productAttributeService.UpdateProductAttribute(productAttribute);

                //activity log
                _customerActivityService.InsertActivity("EditProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = productAttribute.Id });
                }
                return RedirectToAction("List");

            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            _productAttributeService.DeleteProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteProductAttribute"), productAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Used by products

        //used by products
        [HttpPost]
        public IActionResult UsedByProducts(DataSourceRequest command, string productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var orders = _productService.GetProductsByProductAtributeId(
                productAttributeId: productAttributeId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    return new ProductAttributeModel.UsedByProductModel
                    {
                        Id = x.Id,
                        ProductName = x.Name,
                        Published = x.Published
                    };
                }),
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }
        
        #endregion

        #region Predefined values

        [HttpPost]
        public IActionResult PredefinedProductAttributeValueList(string productAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var values = _productAttributeService.GetProductAttributeById(productAttributeId).PredefinedProductAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    return new PredefinedProductAttributeValueModel
                    {
                        Id = x.Id,
                        ProductAttributeId = x.ProductAttributeId,
                        Name = x.Name,
                        PriceAdjustment = x.PriceAdjustment,
                        PriceAdjustmentStr = x.PriceAdjustment.ToString("G29"),
                        WeightAdjustment = x.WeightAdjustment,
                        WeightAdjustmentStr = x.WeightAdjustment.ToString("G29"),
                        Cost = x.Cost,
                        IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder
                    };
                }),
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public IActionResult PredefinedProductAttributeValueCreatePopup(string productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(productAttributeId);
            if (productAttribute == null)
                throw new ArgumentException("No product attribute found with the specified id");

            var model = new PredefinedProductAttributeValueModel();
            model.ProductAttributeId = productAttributeId;

            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public IActionResult PredefinedProductAttributeValueCreatePopup(PredefinedProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.ProductAttributeId);
            if (productAttribute == null)
                throw new ArgumentException("No product attribute found with the specified id");

            if (ModelState.IsValid)
            {
                var ppav = new PredefinedProductAttributeValue
                {
                    ProductAttributeId = model.ProductAttributeId,
                    Name = model.Name,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    Cost = model.Cost,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                };
                ppav.Locales = UpdateLocales(ppav, model);
                productAttribute.PredefinedProductAttributeValues.Add(ppav);
                _productAttributeService.UpdateProductAttribute(productAttribute);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult PredefinedProductAttributeValueEditPopup(string id, string productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var ppav = _productAttributeService.GetProductAttributeById(productAttributeId).PredefinedProductAttributeValues.Where(x=>x.Id == id).FirstOrDefault();
            if (ppav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            var model = new PredefinedProductAttributeValueModel
            {
                ProductAttributeId = ppav.ProductAttributeId,
                Name = ppav.Name,
                PriceAdjustment = ppav.PriceAdjustment,
                WeightAdjustment = ppav.WeightAdjustment,
                Cost = ppav.Cost,
                IsPreSelected = ppav.IsPreSelected,
                DisplayOrder = ppav.DisplayOrder
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = ppav.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost]
        public IActionResult PredefinedProductAttributeValueEditPopup(PredefinedProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
            var productAttribute = _productAttributeService.GetProductAttributeById(model.ProductAttributeId);
            var ppav = productAttribute.PredefinedProductAttributeValues.Where(x=>x.Id == model.Id).FirstOrDefault();
            if (ppav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            if (ModelState.IsValid)
            {
                ppav.Name = model.Name;
                ppav.PriceAdjustment = model.PriceAdjustment;
                ppav.WeightAdjustment = model.WeightAdjustment;
                ppav.Cost = model.Cost;
                ppav.IsPreSelected = model.IsPreSelected;
                ppav.DisplayOrder = model.DisplayOrder;
                ppav.Locales = UpdateLocales(ppav, model);

                _productAttributeService.UpdateProductAttribute(productAttribute);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult PredefinedProductAttributeValueDelete(string id, string productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(productAttributeId);
            var ppav = productAttribute.PredefinedProductAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (ppav == null)
                throw new ArgumentException("No predefined product attribute value found with the specified id");
            productAttribute.PredefinedProductAttributeValues.Remove(ppav);
            _productAttributeService.UpdateProductAttribute(productAttribute);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
