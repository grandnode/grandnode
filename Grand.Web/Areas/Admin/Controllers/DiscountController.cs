using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
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
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Discounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Grand.Web.Areas.Admin.Controllers
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
            var discountPlugins = _discountService.LoadAllDiscountPlugins();
            foreach (var discountPlugin in discountPlugins)
                foreach (var discountRule in discountPlugin.GetRequirementRules())
                    model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = discountRule.FriendlyName, Value = discountRule.SystemName });

            //discount amount providers
            foreach (var item in _discountService.LoadDiscountAmountProviders())
            {
                model.AvailableDiscountAmountProviders.Add(new SelectListItem() { Value = item.PluginDescriptor.SystemName, Text = item.PluginDescriptor.FriendlyName });
            }

            if (discount != null)
            {
                //requirements
                foreach (var dr in discount.DiscountRequirements.OrderBy(dr => dr.Id))
                {
                    var discountPlugin = _discountService.LoadDiscountPluginBySystemName(dr.DiscountRequirementRuleSystemName);
                    var discountRequirement = discountPlugin.GetRequirementRules().Single(x => x.SystemName == dr.DiscountRequirementRuleSystemName);
                    {
                        if (discountPlugin != null)
                        {
                            model.DiscountRequirementMetaInfos.Add(new DiscountModel.DiscountRequirementMetaInfo
                            {
                                DiscountRequirementId = dr.Id,
                                RuleName = discountRequirement.FriendlyName,
                                ConfigurationUrl = GetRequirementUrlInternal(discountRequirement, discount, dr.Id)
                            });
                        }
                    }
                }
            }
            else
                model.IsEnabled = true;
        }

        #endregion

        #region Discounts

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountListModel
            {
                AvailableDiscountTypes = DiscountType.AssignedToOrderTotal.ToSelectList(false).ToList()
            };
            model.AvailableDiscountTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public IActionResult List(DiscountListModel model, DataSourceRequest command)
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
                    discountModel.PrimaryStoreCurrencyCode = x.CalculateByPlugin ? "" : _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                    discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1).TotalCount;
                    return discountModel;
                }),
                Total = discounts.Count
            };

            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
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
        public IActionResult Create(DiscountModel model, bool continueEditing)
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
        public IActionResult Edit(string id)
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
        public IActionResult Edit(DiscountModel model, bool continueEditing)
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
                        var item = category.AppliedDiscounts.Where(x => x == discount.Id).FirstOrDefault();
                        category.AppliedDiscounts.Remove(item);
                    }
                }
                if (prevDiscountType == DiscountType.AssignedToManufacturers
                    && discount.DiscountType != DiscountType.AssignedToManufacturers)
                {
                    //applied to manufacturers
                    var manufacturers = _manufacturerService.GetAllManufacturersByDiscount(discount.Id);
                    foreach (var manufacturer in manufacturers)
                    {
                        var item = manufacturer.AppliedDiscounts.Where(x => x == discount.Id).FirstOrDefault();
                        manufacturer.AppliedDiscounts.Remove(item);
                    }
                }
                if (prevDiscountType == DiscountType.AssignedToSkus
                    && discount.DiscountType != DiscountType.AssignedToSkus)
                {
                    //applied to products
                    var products = _productService.GetProductsByDiscount(discount.Id);

                    foreach (var p in products)
                    {
                        var item = p.AppliedDiscounts.Where(x => x == discount.Id).FirstOrDefault();
                        p.AppliedDiscounts.Remove(item);
                        _productService.DeleteDiscount(item, p.Id);
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
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var usagehistory = _discountService.GetAllDiscountUsageHistory(discount.Id);
            if(usagehistory.Count > 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted.UsageHistory"));
                return RedirectToAction("Edit", new { id = discount.Id });
            }

            _discountService.DeleteDiscount(discount);

            //activity log
            _customerActivityService.InsertActivity("DeleteDiscount", discount.Id, _localizationService.GetResource("ActivityLog.DeleteDiscount"), discount.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Discount coupon codes
        [HttpPost]
        public IActionResult CouponCodeList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var couponcodes = _discountService.GetAllCouponCodesByDiscountId(discount.Id, pageIndex: command.Page - 1, pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = couponcodes.Select(x => new 
                {
                    Id = x.Id,
                    CouponCode = x.CouponCode,
                    Used = x.Used
                }),
                Total = couponcodes.TotalCount
            };
            return Json(gridModel);
        }

        public IActionResult CouponCodeDelete(string discountId, string Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var coupon = _discountService.GetDiscountCodeById(Id);
            if (coupon == null)
                throw new Exception("No coupon code found with the specified id");

            if(!coupon.Used)
                _discountService.DeleteDiscountCoupon(coupon);
            else
                return new JsonResult(new DataSourceResult() { Errors = "You can't delete coupon code, it was used" });

            return new NullJsonResult();
        }
        public IActionResult CouponCodeInsert(string discountId, string couponCode)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if(string.IsNullOrEmpty(couponCode))
                throw new Exception("Coupon code can't be empty");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            couponCode = couponCode.ToUpper();

            if (_discountService.GetDiscountByCouponCode(couponCode)!=null)
                return new JsonResult(new DataSourceResult() { Errors = "Coupon code exists" });

            var coupon = new DiscountCoupon
            {
                CouponCode = couponCode,
                DiscountId = discountId
            };
            _discountService.InsertDiscountCoupon(coupon);

            return new NullJsonResult();
        }
        #endregion

        #region Discount requirements

        [AcceptVerbs("GET")]
        public IActionResult GetDiscountRequirementConfigurationUrl(string systemName, string discountId, string discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var discountPlugin = _discountService.LoadDiscountPluginBySystemName(systemName);

            if (discountPlugin == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var singleRequirement = discountPlugin.GetRequirementRules().Single(x => x.SystemName == systemName);
            string url = GetRequirementUrlInternal(singleRequirement, discount, discountRequirementId);
            return Json(new { url = url });
        }

        public IActionResult GetDiscountRequirementMetaInfo(string discountRequirementId, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            var discountPlugin = _discountService.LoadDiscountPluginBySystemName(discountRequirement.DiscountRequirementRuleSystemName);
            if (discountPlugin == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discountRequirementRule = discountPlugin.GetRequirementRules().First(x => x.SystemName == discountRequirement.DiscountRequirementRuleSystemName);
            string url = GetRequirementUrlInternal(discountRequirementRule, discount, discountRequirementId);
            string ruleName = discountRequirementRule.FriendlyName;

            return Json(new { url = url, ruleName = ruleName });
        }

        [HttpPost]
        public IActionResult DeleteDiscountRequirement(string discountRequirementId, string discountId)
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

            return Json(new { Result = true });
        }

        #endregion

        #region Applied to products

        [HttpPost]
        public IActionResult ProductList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var products = _productService.GetProductsByDiscount(discount.Id, pageIndex: command.Page - 1, pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new DiscountModel.AppliedToProductModel
                {
                    ProductId = x.Id,
                    ProductName = x.Name
                }),
                Total = products.TotalCount
            };

            return Json(gridModel);
        }

        public IActionResult ProductDelete(string discountId, string productId)
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
            if (product.AppliedDiscounts.Count(d => d == discount.Id) > 0)
            {
                product.AppliedDiscounts.Remove(discount.Id);
                _productService.DeleteDiscount(discount.Id, product.Id);
            }

            return new NullJsonResult();
        }

        public IActionResult ProductAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddProductToDiscountModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, DiscountModel.AddProductToDiscountModel model)
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
        public IActionResult ProductAddPopup(DiscountModel.AddProductToDiscountModel model)
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
                        if (product.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                        {
                            product.AppliedDiscounts.Add(discount.Id);
                            _productService.InsertDiscount(discount.Id, product.Id);
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to categories

        [HttpPost]
        public IActionResult CategoryList(DataSourceRequest command, string discountId)
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

        public IActionResult CategoryDelete(string discountId, string categoryId)
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
            if (category.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                category.AppliedDiscounts.Remove(discount.Id);

            _categoryService.UpdateCategory(category);

            return new NullJsonResult();
        }

        public IActionResult CategoryAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddCategoryToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, DiscountModel.AddCategoryToDiscountModel model)
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
        public IActionResult CategoryAddPopup(DiscountModel.AddCategoryToDiscountModel model)
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
                        if (category.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            category.AppliedDiscounts.Add(discount.Id);

                        _categoryService.UpdateCategory(category);
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to manufacturers

        [HttpPost]
        public IActionResult ManufacturerList(DataSourceRequest command, string discountId)
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

        public IActionResult ManufacturerDelete(string discountId, string manufacturerId)
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
            if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                manufacturer.AppliedDiscounts.Remove(discount.Id);

            _manufacturerService.UpdateManufacturer(manufacturer);
            return new NullJsonResult();
        }

        public IActionResult ManufacturerAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddManufacturerToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ManufacturerAddPopupList(DataSourceRequest command, DiscountModel.AddManufacturerToDiscountModel model)
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
        public IActionResult ManufacturerAddPopup(DiscountModel.AddManufacturerToDiscountModel model)
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
                        if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            manufacturer.AppliedDiscounts.Add(discount.Id);

                        _manufacturerService.UpdateManufacturer(manufacturer);
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to vendors

        [HttpPost]
        public IActionResult VendorList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendors = _vendorService.GetAllVendorsByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = vendors.Select(x => new DiscountModel.AppliedToVendorModel
                {
                    VendorId = x.Id,
                    VendorName = x.Name
                }),
                Total = vendors.Count
            };

            return Json(gridModel);
        }

        public IActionResult VendorDelete(string discountId, string vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new Exception("No vendor found with the specified id");

            //remove discount
            if (vendor.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                vendor.AppliedDiscounts.Remove(discount.Id);

            _vendorService.UpdateVendor(vendor);
            return new NullJsonResult();
        }

        public IActionResult VendorAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddVendorToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult VendorAddPopupList(DataSourceRequest command, DiscountModel.AddVendorToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var vendors = _vendorService.GetAllVendors(model.SearchVendorName, command.Page - 1, command.PageSize, true);

            //search for emails
            if (!(string.IsNullOrEmpty(model.SearchVendorEmail)))
            {
                var tempVendors = vendors.Where(x => x.Email.ToLowerInvariant().Contains(model.SearchVendorEmail.Trim()));
                vendors = new PagedList<Core.Domain.Vendors.Vendor>(tempVendors, command.Page - 1, command.PageSize);
            }

            var gridModel = new DataSourceResult
            {
                Data = vendors.Select(x => x.ToModel()),
                Total = vendors.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult VendorAddPopup(DiscountModel.AddVendorToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedVendorIds != null)
            {
                foreach (string id in model.SelectedVendorIds)
                {
                    var vendor = _vendorService.GetVendorById(id);
                    if (vendor != null)
                    {
                        if (vendor.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            vendor.AppliedDiscounts.Add(discount.Id);

                        _vendorService.UpdateVendor(vendor);
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to store
        [HttpPost]
        public IActionResult StoreList(DataSourceRequest command, string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var stores = _storeService.GetAllStoresByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = stores.Select(x => new DiscountModel.AppliedToStoreModel
                {
                    StoreId = x.Id,
                    StoreName = x.Name
                }),
                Total = stores.Count
            };

            return Json(gridModel);
        }

        public IActionResult StoreDelete(string discountId, string storeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var store = _storeService.GetStoreById(storeId);
            if (store == null)
                throw new Exception("No store found with the specified id");

            //remove discount
            if (store.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                store.AppliedDiscounts.Remove(discount.Id);

            _storeService.UpdateStore(store);
            return new NullJsonResult();
        }

        public IActionResult StoreAddPopup(string discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddStoreToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult StoreAddPopupList(DataSourceRequest command, DiscountModel.AddStoreToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var stores = _storeService.GetAllStores();
            var gridModel = new DataSourceResult
            {
                Data = stores.Select(x => x.ToModel()),
                Total = stores.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult StoreAddPopup(DiscountModel.AddStoreToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedStoreIds != null)
            {
                foreach (string id in model.SelectedStoreIds)
                {
                    var store = _storeService.GetStoreById(id);
                    if (store != null)
                    {
                        if (store.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            store.AppliedDiscounts.Add(discount.Id);

                        _storeService.UpdateStore(store);
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Discount usage history

        [HttpPost]
        public IActionResult UsageHistoryList(string discountId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = _discountService.GetAllDiscountUsageHistory(discount.Id, null, null, null, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = duh.Select(x => {
                    var order = _orderService.GetOrderById(x.OrderId);
                    var duhModel = new DiscountModel.DiscountUsageHistoryModel
                    {
                        Id = x.Id,
                        DiscountId = x.DiscountId,
                        OrderId = x.OrderId,
                        OrderNumber = order != null ? order.OrderNumber : 0,
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
        public IActionResult UsageHistoryDelete(string discountId, string id)
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
