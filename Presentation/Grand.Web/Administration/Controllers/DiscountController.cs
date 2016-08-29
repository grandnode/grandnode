using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Discounts;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Controllers
{
    public partial class DiscountController : BaseAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Constructors

        public DiscountController(IDiscountService discountService, 
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ICategoryService categoryService,
            IProductService productService,
            IWebHelper webHelper, 
            IDateTimeHelper dateTimeHelper,
            ICustomerActivityService customerActivityService, 
            CurrencySettings currencySettings,
            IPermissionService permissionService,
            IWorkContext workContext,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IOrderService orderService,
            IPriceFormatter priceFormatter)
        {
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._categoryService = categoryService;
            this._productService = productService;
            this._webHelper = webHelper;
            this._dateTimeHelper = dateTimeHelper;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._orderService = orderService;
            this._priceFormatter = priceFormatter;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, string discountRequirementId)
        {   
            if (discountRequirementRule == null)
                throw new ArgumentNullException("discountRequirementRule");

            if (discount == null)
                throw new ArgumentNullException("discount");

            string url = string.Format("{0}{1}", _webHelper.GetStoreLocation(), discountRequirementRule.GetConfigurationUrl(discount.Id, discountRequirementId));
            return url;
        }
        
        [NonAction]
        protected virtual void PrepareDiscountModel(DiscountModel model, Discount discount)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.Select"), Value = "" });
            var discountRules = _discountService.LoadAllDiscountRequirementRules();
            foreach (var discountRule in discountRules)
                model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = discountRule.PluginDescriptor.FriendlyName, Value = discountRule.PluginDescriptor.SystemName });

            if (discount != null)
            {
                //requirements
                foreach (var dr in discount.DiscountRequirements.OrderBy(dr=>dr.Id))
                {
                    var drr = _discountService.LoadDiscountRequirementRuleBySystemName(dr.DiscountRequirementRuleSystemName);
                    if (drr != null)
                    {
                        model.DiscountRequirementMetaInfos.Add(new DiscountModel.DiscountRequirementMetaInfo
                        {
                            DiscountRequirementId = dr.Id,
                            RuleName = drr.PluginDescriptor.FriendlyName,
                            ConfigurationUrl = GetRequirementUrlInternal(drr, discount, dr.Id)
                        });
                    }
                }
            }
        }

        #endregion

        #region Discounts

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountListModel();
            model.AvailableDiscountTypes = DiscountType.AssignedToOrderTotal.ToSelectList(false).ToList();
            model.AvailableDiscountTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DiscountListModel model, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            DiscountType? discountType = null;
            if (model.SearchDiscountTypeId > 0)
                discountType = (DiscountType) model.SearchDiscountTypeId;
            var discounts = _discountService.GetAllDiscounts(discountType,
                model.SearchDiscountCouponCode,
                model.SearchDiscountName,
                true);

            var gridModel = new DataSourceResult
            {
                Data = discounts.PagedForCommand(command).Select(x =>
                {
                    var discountModel = x.ToModel();
                    discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
                    discountModel.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                    discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1).TotalCount;
                    return discountModel;
                }),
                Total = discounts.Count
            };

            return Json(gridModel);
        }
        
        //create
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel();
            PrepareDiscountModel(model, null);
            //default values
            model.LimitationTimes = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var discount = model.ToEntity();
                _discountService.InsertDiscount(discount);

                //activity log
                _customerActivityService.InsertActivity("AddNewDiscount", discount.Id, _localizationService.GetResource("ActivityLog.AddNewDiscount"), discount.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = discount.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, null);
            return View(model);
        }

        //edit
        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var model = discount.ToModel();
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.Id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevDiscountType = discount.DiscountType;
                discount = model.ToEntity(discount);
                _discountService.UpdateDiscount(discount);

                //clean up old references (if changed) and update "HasDiscountsApplied" properties
                if (prevDiscountType == DiscountType.AssignedToCategories 
                    && discount.DiscountType != DiscountType.AssignedToCategories)
                {
                    //applied to categories
                    //_categoryService.
                    var categories = _categoryService.GetAllCategoriesByDiscount(discount.Id);

                    //update "HasDiscountsApplied" property
                    foreach (var category in categories)
                    {
                        var item = category.AppliedDiscounts.Where(x => x.Id == discount.Id).FirstOrDefault();
                        category.AppliedDiscounts.Remove(item);
                        //_discountService.UpdateDiscount(discount);
                        //_categoryService.Update(category);
                    }
                }
                if (prevDiscountType == DiscountType.AssignedToManufacturers
                    && discount.DiscountType != DiscountType.AssignedToManufacturers)
                {
                    //applied to manufacturers
                    var manufacturers = _manufacturerService.GetAllManufacturersByDiscount(discount.Id);
                    foreach (var manufacturer in manufacturers)
                    {
                        var item = manufacturer.AppliedDiscounts.Where(x => x.Id == discount.Id).FirstOrDefault();
                        manufacturer.AppliedDiscounts.Remove(item);
                        //_manufacturerService.UpdateHasDiscountsApplied(manufacturer);
                    }
                }
                if (prevDiscountType == DiscountType.AssignedToSkus
                    && discount.DiscountType != DiscountType.AssignedToSkus)
                {
                    //applied to products
                    //var products = discount.AppliedToProducts.ToList();
                    var products = _productService.GetProductsByDiscount(discount.Id);

                    foreach (var p in products)
                    {
                        var item = p.AppliedDiscounts.Where(x => x.Id == discount.Id).FirstOrDefault();
                        p.AppliedDiscounts.Remove(item);
                        _productService.DeleteDiscount(item, p.Id);
                        _productService.UpdateHasDiscountsApplied(p.Id);
                    }
                }

                //activity log
                _customerActivityService.InsertActivity("EditDiscount", discount.Id, _localizationService.GetResource("ActivityLog.EditDiscount"), discount.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit",  new {id = discount.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        //delete
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            //applied to categories
            var categories = _categoryService.GetAllCategoriesByDiscount(id); //discount.AppliedToCategories.ToList();
            //applied to manufacturers
            var manufacturers = _manufacturerService.GetAllManufacturersByDiscount(id);
            //applied to products
            var products = _productService.GetProductsByDiscount(id);

            _discountService.DeleteDiscount(discount);

            //update "HasDiscountsApplied" properties
            /*foreach (var category in categories)
            {
                _categoryService.UpdateHasDiscountsApplied(category);
            }
            foreach (var manufacturer in manufacturers)
                _manufacturerService.UpdateHasDiscountsApplied(manufacturer);
            */
            foreach (var p in products)
                _productService.UpdateHasDiscountsApplied(p.Id);

            //activity log
            _customerActivityService.InsertActivity("DeleteDiscount", discount.Id, _localizationService.GetResource("ActivityLog.DeleteDiscount"), discount.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Discount requirements

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDiscountRequirementConfigurationUrl(string systemName, string discountId, string discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");
            
            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(systemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            string url = GetRequirementUrlInternal(discountRequirementRule, discount, discountRequirementId);
            return Json(new { url = url }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDiscountRequirementMetaInfo(string discountRequirementId, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(discountRequirement.DiscountRequirementRuleSystemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            string url = GetRequirementUrlInternal(discountRequirementRule, discount, discountRequirementId);
            string ruleName = discountRequirementRule.PluginDescriptor.FriendlyName;
            return Json(new { url = url, ruleName = ruleName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDiscountRequirement(string discountRequirementId, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            _discountService.DeleteDiscountRequirement(discountRequirement);
            discount.DiscountRequirements.Remove(discountRequirement);
            _discountService.UpdateDiscount(discount);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Applied to products

        [HttpPost]
        public ActionResult ProductList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var products = _productService.GetProductsByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new DiscountModel.AppliedToProductModel
                {
                    ProductId = x.Id,
                    ProductName = x.Name
                }),
                Total = products.Count
            };

            return Json(gridModel);
        }

        public ActionResult ProductDelete(string discountId, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new Exception("No product found with the specified id");

            //remove discount
            if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
            {
                product.AppliedDiscounts.Remove(discount);
                _productService.DeleteDiscount(discount, product.Id);
            }
            //_productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product.Id);

            return new NullJsonResult();
        }

        public ActionResult ProductAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddProductToDiscountModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAddPopupList(DataSourceRequest command, DiscountModel.AddProductToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, DiscountModel.AddProductToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedProductIds != null)
            {
                foreach (string id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                        {
                            product.AppliedDiscounts.Add(discount);
                            _productService.InsertDiscount(discount, product.Id);
                        }
                        //_productService.UpdateProduct(product);
                        _productService.UpdateHasDiscountsApplied(product.Id);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Applied to categories

        [HttpPost]
        public ActionResult CategoryList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var categories = _categoryService.GetAllCategoriesByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x => new DiscountModel.AppliedToCategoryModel
                {
                    CategoryId = x.Id,
                    CategoryName = x.GetFormattedBreadCrumb(_categoryService)
                }),
                Total = categories.Count
            };

            return Json(gridModel);
        }

        public ActionResult CategoryDelete(string discountId, string categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new Exception("No category found with the specified id");

            //remove discount
            if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                category.AppliedDiscounts.Remove(discount);

            _categoryService.UpdateCategory(category);
            //_categoryService.UpdateHasDiscountsApplied(category);

            return new NullJsonResult();
        }

        public ActionResult CategoryAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddCategoryToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CategoryAddPopupList(DataSourceRequest command, DiscountModel.AddCategoryToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult CategoryAddPopup(string btnId, string formId, DiscountModel.AddCategoryToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedCategoryIds != null)
            {
                foreach (string id in model.SelectedCategoryIds)
                {
                    var category = _categoryService.GetCategoryById(id);
                    if (category != null)
                    {
                        if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            category.AppliedDiscounts.Add(discount);

                        _categoryService.UpdateCategory(category);
                        //_categoryService.UpdateHasDiscountsApplied(category);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Applied to manufacturers

        [HttpPost]
        public ActionResult ManufacturerList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturers = _manufacturerService.GetAllManufacturersByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => new DiscountModel.AppliedToManufacturerModel
                {
                    ManufacturerId = x.Id,
                    ManufacturerName = x.Name
                }),
                Total = manufacturers.Count
            };

            return Json(gridModel);
        }

        public ActionResult ManufacturerDelete(string discountId, string manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                throw new Exception("No manufacturer found with the specified id");

            //remove discount
            if (manufacturer.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                manufacturer.AppliedDiscounts.Remove(discount);

            _manufacturerService.UpdateManufacturer(manufacturer);
            return new NullJsonResult();
        }

        public ActionResult ManufacturerAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddManufacturerToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ManufacturerAddPopupList(DataSourceRequest command, DiscountModel.AddManufacturerToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName,"",
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ManufacturerAddPopup(string btnId, string formId, DiscountModel.AddManufacturerToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedManufacturerIds != null)
            {
                foreach (string id in model.SelectedManufacturerIds)
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(id);
                    if (manufacturer != null)
                    {
                        if (manufacturer.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            manufacturer.AppliedDiscounts.Add(discount);

                        _manufacturerService.UpdateManufacturer(manufacturer);
                        //_manufacturerService.UpdateHasDiscountsApplied(manufacturer);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Discount usage history
        
        [HttpPost]
        public ActionResult UsageHistoryList(string discountId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = _discountService.GetAllDiscountUsageHistory(discount.Id, null, null, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = duh.Select(x => {
                    var order = _orderService.GetOrderById(x.OrderId);
                    var duhModel = new DiscountModel.DiscountUsageHistoryModel
                    {
                        Id = x.Id,
                        DiscountId = x.DiscountId,
                        OrderId = x.OrderId,
                        OrderTotal = order != null ? _priceFormatter.FormatPrice(order.OrderTotal, true, false) : "",
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                    return duhModel;
                }),
                Total = duh.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult UsageHistoryDelete(string discountId, string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");
            
            var duh = _discountService.GetDiscountUsageHistoryById(id);
            if (duh != null)
                _discountService.DeleteDiscountUsageHistory(duh);

            return new NullJsonResult();
        }

        #endregion
    }
}
