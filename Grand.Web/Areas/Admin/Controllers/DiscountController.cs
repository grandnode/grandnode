using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Discounts)]
    public partial class DiscountController : BaseAdminController
    {
        #region Fields
        private readonly IDiscountViewModelService _discountViewModelService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public DiscountController(
            IDiscountViewModelService discountViewModelService,
            IDiscountService discountService, 
            ILocalizationService localizationService)
        {
            this._discountViewModelService = discountViewModelService;
            this._discountService = discountService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Discounts

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _discountViewModelService.PrepareDiscountListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DiscountListModel model, DataSourceRequest command)
        {
            var discounts = _discountViewModelService.PrepareDiscountModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = discounts.discountModel.ToList(),
                Total = discounts.totalCount
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            var model = new DiscountModel();
            _discountViewModelService.PrepareDiscountModel(model, null);
            //default values
            model.LimitationTimes = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var discount = _discountViewModelService.InsertDiscountModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = discount.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            _discountViewModelService.PrepareDiscountModel(model, null);
            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var model = discount.ToModel();
            _discountViewModelService.PrepareDiscountModel(model, discount);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(DiscountModel model, bool continueEditing)
        {
            var discount = _discountService.GetDiscountById(model.Id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                discount = _discountViewModelService.UpdateDiscountModel(discount, model);
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
            _discountViewModelService.PrepareDiscountModel(model, discount);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var usagehistory = _discountService.GetAllDiscountUsageHistory(discount.Id);
            if (usagehistory.Count > 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted.UsageHistory"));
                return RedirectToAction("Edit", new { id = discount.Id });
            }
            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteDiscount(discount);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = discount.Id });
        }

        #endregion

        #region Discount coupon codes
        [HttpPost]
        public IActionResult CouponCodeList(DataSourceRequest command, string discountId)
        {
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
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var coupon = _discountService.GetDiscountCodeById(Id);
            if (coupon == null)
                throw new Exception("No coupon code found with the specified id");
            if (ModelState.IsValid)
            {
                if (!coupon.Used)
                    _discountService.DeleteDiscountCoupon(coupon);
                else
                    return new JsonResult(new DataSourceResult() { Errors = "You can't delete coupon code, it was used" });

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);

        }
        public IActionResult CouponCodeInsert(string discountId, string couponCode)
        {
            if(string.IsNullOrEmpty(couponCode))
                throw new Exception("Coupon code can't be empty");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            couponCode = couponCode.ToUpper();

            if (_discountService.GetDiscountByCouponCode(couponCode)!=null)
                return new JsonResult(new DataSourceResult() { Errors = "Coupon code exists" });
            if (ModelState.IsValid)
            {
                _discountViewModelService.InsertCouponCode(discountId, couponCode);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion

        #region Discount requirements

        [AcceptVerbs("GET")]
        public IActionResult GetDiscountRequirementConfigurationUrl(string systemName, string discountId, string discountRequirementId)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var discountPlugin = _discountService.LoadDiscountPluginBySystemName(systemName);

            if (discountPlugin == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var singleRequirement = discountPlugin.GetRequirementRules().Single(x => x.SystemName == systemName);
            string url = _discountViewModelService.GetRequirementUrlInternal(singleRequirement, discount, discountRequirementId);
            return Json(new { url = url });
        }

        public IActionResult GetDiscountRequirementMetaInfo(string discountRequirementId, string discountId)
        {
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
            string url = _discountViewModelService.GetRequirementUrlInternal(discountRequirementRule, discount, discountRequirementId);
            string ruleName = discountRequirementRule.FriendlyName;

            return Json(new { url = url, ruleName = ruleName });
        }

        [HttpPost]
        public IActionResult DeleteDiscountRequirement(string discountRequirementId, string discountId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteDiscountRequirement(discountRequirement, discount);
                return Json(new { Result = true });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Applied to products

        [HttpPost]
        public IActionResult ProductList(DataSourceRequest command, string discountId, [FromServices] IProductService productService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var products = productService.GetProductsByDiscount(discount.Id, pageIndex: command.Page - 1, pageSize: command.PageSize);
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

        public IActionResult ProductDelete(string discountId, string productId, [FromServices] IProductService productService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var product = productService.GetProductById(productId);
            if (product == null)
                throw new Exception("No product found with the specified id");

            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteProduct(discount, product);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ProductAddPopup(string discountId)
        {
            var model = _discountViewModelService.PrepareProductToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, DiscountModel.AddProductToDiscountModel model)
        {
            var products = _discountViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult();
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(DiscountModel.AddProductToDiscountModel model)
        {
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedProductIds != null)
            {
                _discountViewModelService.InsertProductToDiscountModel(model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to categories

        [HttpPost]
        public IActionResult CategoryList(DataSourceRequest command, string discountId, [FromServices] ICategoryService categoryService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var categories = categoryService.GetAllCategoriesByDiscount(discount.Id);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x => new DiscountModel.AppliedToCategoryModel
                {
                    CategoryId = x.Id,
                    CategoryName = x.GetFormattedBreadCrumb(categoryService)
                }),
                Total = categories.Count
            };

            return Json(gridModel);
        }

        public IActionResult CategoryDelete(string discountId, string categoryId, [FromServices] ICategoryService categoryService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var category = categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new Exception("No category found with the specified id");

            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteCategory(discount, category);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult CategoryAddPopup(string discountId)
        {
            var model = new DiscountModel.AddCategoryToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, DiscountModel.AddCategoryToDiscountModel model, [FromServices] ICategoryService categoryService)
        {
            var categories = categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(categoryService);
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
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedCategoryIds != null)
            {
                _discountViewModelService.InsertCategoryToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to manufacturers

        [HttpPost]
        public IActionResult ManufacturerList(DataSourceRequest command, string discountId, [FromServices] IManufacturerService manufacturerService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturers = manufacturerService.GetAllManufacturersByDiscount(discount.Id);
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

        public IActionResult ManufacturerDelete(string discountId, string manufacturerId, [FromServices] IManufacturerService manufacturerService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturer = manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                throw new Exception("No manufacturer found with the specified id");

            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteManufacturer(discount, manufacturer);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ManufacturerAddPopup(string discountId)
        {
            var model = new DiscountModel.AddManufacturerToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ManufacturerAddPopupList(DataSourceRequest command, DiscountModel.AddManufacturerToDiscountModel model, [FromServices] IManufacturerService manufacturerService)
        {
            var manufacturers = manufacturerService.GetAllManufacturers(model.SearchManufacturerName,"",
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
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedManufacturerIds != null)
            {
                _discountViewModelService.InsertManufacturerToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to vendors

        [HttpPost]
        public IActionResult VendorList(DataSourceRequest command, string discountId, [FromServices] IVendorService vendorService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendors = vendorService.GetAllVendorsByDiscount(discount.Id);
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

        public IActionResult VendorDelete(string discountId, string vendorId, [FromServices] IVendorService vendorService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendor = vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new Exception("No vendor found with the specified id");
            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteVendor(discount, vendor);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult VendorAddPopup(string discountId)
        {
            var model = new DiscountModel.AddVendorToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult VendorAddPopupList(DataSourceRequest command, DiscountModel.AddVendorToDiscountModel model, [FromServices] IVendorService vendorService)
        {
            var vendors = vendorService.GetAllVendors(model.SearchVendorName, command.Page - 1, command.PageSize, true);

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
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedVendorIds != null)
            {
                _discountViewModelService.InsertVendorToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to store
        [HttpPost]
        public IActionResult StoreList(DataSourceRequest command, string discountId, [FromServices] IStoreService storeService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var stores = storeService.GetAllStoresByDiscount(discount.Id);
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

        public IActionResult StoreDelete(string discountId, string storeId, [FromServices] IStoreService storeService)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");
            var store = storeService.GetStoreById(storeId);
            if (store == null)
                throw new Exception("No store found with the specified id");

            if (ModelState.IsValid)
            {
                _discountViewModelService.DeleteStore(discount, store);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult StoreAddPopup(string discountId)
        {
            var model = new DiscountModel.AddStoreToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult StoreAddPopupList(DataSourceRequest command, DiscountModel.AddStoreToDiscountModel model, [FromServices] IStoreService storeService)
        {
            var stores = storeService.GetAllStores();
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
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedStoreIds != null)
            {
                _discountViewModelService.InsertStoreToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Discount usage history

        [HttpPost]
        public IActionResult UsageHistoryList(string discountId, DataSourceRequest command)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = _discountViewModelService.PrepareDiscountUsageHistoryModel(discount, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = duh.usageHistoryModels.ToList(),
                Total = duh.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult UsageHistoryDelete(string discountId, string id)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");
            
            var duh = _discountService.GetDiscountUsageHistoryById(id);
            if (duh != null)
            {
                if (ModelState.IsValid)
                {
                    _discountService.DeleteDiscountUsageHistory(duh);
                }
                else
                    return ErrorForKendoGridJson(ModelState);
            }
            return new NullJsonResult();
        }

        #endregion
    }
}
