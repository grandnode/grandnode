using Grand.Core;
using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Discounts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Discounts)]
    public partial class DiscountController : BaseAdminController
    {
        #region Fields
        
        private readonly IDiscountViewModelService _discountViewModelService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Constructors

        public DiscountController(
            IDiscountViewModelService discountViewModelService,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreService storeService,
            IDateTimeHelper dateTimeHelper)
        {
            _discountViewModelService = discountViewModelService;
            _discountService = discountService;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeService = storeService;
            _dateTimeHelper = dateTimeHelper;
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
        public async Task<IActionResult> List(DiscountListModel model, DataSourceRequest command)
        {
            var (discountModel, totalCount) = await _discountViewModelService.PrepareDiscountModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = discountModel.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        //create
        public async Task<IActionResult> Create()
        {
            var model = new DiscountModel();
            await _discountViewModelService.PrepareDiscountModel(model, null);

            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            //default values
            model.LimitationTimes = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(DiscountModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                var discount = await _discountViewModelService.InsertDiscountModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = discount.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            await _discountViewModelService.PrepareDiscountModel(model, null);

            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        //edit
        public async Task<IActionResult> Edit(string id)
        {
            var discount = await _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!discount.LimitedToStores || (discount.LimitedToStores && discount.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && discount.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Permisions"));
                else
                {
                    if (!discount.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }

            var model = discount.ToModel(_dateTimeHelper);
            await _discountViewModelService.PrepareDiscountModel(model, discount);

            //Stores
            await model.PrepareStoresMappingModel(discount, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(DiscountModel model, bool continueEditing)
        {
            var discount = await _discountService.GetDiscountById(model.Id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!discount.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = discount.Id });
            }

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }
                discount = await _discountViewModelService.UpdateDiscountModel(discount, model);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();
                    return RedirectToAction("Edit", new { id = discount.Id });
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            await _discountViewModelService.PrepareDiscountModel(model, discount);

            //Stores
            await model.PrepareStoresMappingModel(discount, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        //delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var discount = await _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!discount.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = discount.Id });
            }

            var usagehistory = await _discountService.GetAllDiscountUsageHistory(discount.Id);
            if (usagehistory.Count > 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted.UsageHistory"));
                return RedirectToAction("Edit", new { id = discount.Id });
            }
            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteDiscount(discount);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = discount.Id });
        }

        #endregion

        #region Discount coupon codes
        [HttpPost]
        public async Task<IActionResult> CouponCodeList(DataSourceRequest command, string discountId)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var couponcodes = await _discountService.GetAllCouponCodesByDiscountId(discount.Id, pageIndex: command.Page - 1, pageSize: command.PageSize);
            var gridModel = new DataSourceResult {
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

        public async Task<IActionResult> CouponCodeDelete(string discountId, string Id)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var coupon = await _discountService.GetDiscountCodeById(Id);
            if (coupon == null)
                throw new Exception("No coupon code found with the specified id");
            if (ModelState.IsValid)
            {
                if (!coupon.Used)
                    await _discountService.DeleteDiscountCoupon(coupon);
                else
                    return new JsonResult(new DataSourceResult() { Errors = "You can't delete coupon code, it was used" });

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);

        }
        public async Task<IActionResult> CouponCodeInsert(string discountId, string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
                throw new Exception("Coupon code can't be empty");

            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            couponCode = couponCode.ToUpper();

            if ((await _discountService.GetDiscountByCouponCode(couponCode)) != null)
                return new JsonResult(new DataSourceResult() { Errors = "Coupon code exists" });
            if (ModelState.IsValid)
            {
                await _discountViewModelService.InsertCouponCode(discountId, couponCode);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion

        #region Discount requirements

        [AcceptVerbs("GET")]
        public async Task<IActionResult> GetDiscountRequirementConfigurationUrl(string systemName, string discountId, string discountRequirementId)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var discountPlugin = _discountService.LoadDiscountPluginBySystemName(systemName);

            if (discountPlugin == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var singleRequirement = discountPlugin.GetRequirementRules().Single(x => x.SystemName == systemName);
            string url = _discountViewModelService.GetRequirementUrlInternal(singleRequirement, discount, discountRequirementId);
            return Json(new { url = url });
        }

        public async Task<IActionResult> GetDiscountRequirementMetaInfo(string discountRequirementId, string discountId)
        {
            var discount = await _discountService.GetDiscountById(discountId);
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
        public async Task<IActionResult> DeleteDiscountRequirement(string discountRequirementId, string discountId)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
            if (discountRequirement == null)
                throw new ArgumentException("Discount requirement could not be loaded");

            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteDiscountRequirement(discountRequirement, discount);
                return Json(new { Result = true });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Applied to products

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> ProductList(DataSourceRequest command, string discountId, [FromServices] IProductService productService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var products = await productService.GetProductsByDiscount(discount.Id, pageIndex: command.Page - 1, pageSize: command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.Select(x => new DiscountModel.AppliedToProductModel {
                    ProductId = x.Id,
                    ProductName = x.Name
                }),
                Total = products.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> ProductDelete(string discountId, string productId, [FromServices] IProductService productService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var product = await productService.GetProductById(productId);
            if (product == null)
                throw new Exception("No product found with the specified id");

            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteProduct(discount, product);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAddPopup(string discountId)
        {
            var model = await _discountViewModelService.PrepareProductToDiscountModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, DiscountModel.AddProductToDiscountModel model)
        {
            var products = await _discountViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult {
                Data = products.products.ToList(),
                Total = products.totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(DiscountModel.AddProductToDiscountModel model)
        {
            var discount = await _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedProductIds != null)
            {
                await _discountViewModelService.InsertProductToDiscountModel(model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to categories

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> CategoryList(DataSourceRequest command, string discountId, [FromServices] ICategoryService categoryService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var categories = await categoryService.GetAllCategoriesByDiscount(discount.Id);
            var items = new List<DiscountModel.AppliedToCategoryModel>();
            foreach (var item in categories)
            {
                items.Add(new DiscountModel.AppliedToCategoryModel {
                    CategoryId = item.Id,
                    CategoryName = await categoryService.GetFormattedBreadCrumb(item)
                });
            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = categories.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> CategoryDelete(string discountId, string categoryId, [FromServices] ICategoryService categoryService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var category = await categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new Exception("No category found with the specified id");

            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteCategory(discount, category);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public IActionResult CategoryAddPopup(string discountId)
        {
            var model = new DiscountModel.AddCategoryToDiscountModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> CategoryAddPopupList(DataSourceRequest command, DiscountModel.AddCategoryToDiscountModel model, [FromServices] ICategoryService categoryService)
        {
            var categories = await categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var items = new List<CategoryModel>();
            foreach (var item in categories)
            {
                var categoryModel = item.ToModel();
                categoryModel.Breadcrumb = await categoryService.GetFormattedBreadCrumb(item);
                items.Add(categoryModel);
            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = categories.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> CategoryAddPopup(DiscountModel.AddCategoryToDiscountModel model)
        {
            var discount = await _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedCategoryIds != null)
            {
                await _discountViewModelService.InsertCategoryToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to manufacturers

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> ManufacturerList(DataSourceRequest command, string discountId, [FromServices] IManufacturerService manufacturerService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturers = await manufacturerService.GetAllManufacturersByDiscount(discount.Id);
            var gridModel = new DataSourceResult {
                Data = manufacturers.Select(x => new DiscountModel.AppliedToManufacturerModel {
                    ManufacturerId = x.Id,
                    ManufacturerName = x.Name
                }),
                Total = manufacturers.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> ManufacturerDelete(string discountId, string manufacturerId, [FromServices] IManufacturerService manufacturerService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturer = await manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                throw new Exception("No manufacturer found with the specified id");

            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteManufacturer(discount, manufacturer);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public IActionResult ManufacturerAddPopup(string discountId)
        {
            var model = new DiscountModel.AddManufacturerToDiscountModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ManufacturerAddPopupList(DataSourceRequest command, DiscountModel.AddManufacturerToDiscountModel model, [FromServices] IManufacturerService manufacturerService)
        {
            var manufacturers = await manufacturerService.GetAllManufacturers(model.SearchManufacturerName, "",
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ManufacturerAddPopup(DiscountModel.AddManufacturerToDiscountModel model)
        {
            var discount = await _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedManufacturerIds != null)
            {
                await _discountViewModelService.InsertManufacturerToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to vendors

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> VendorList(DataSourceRequest command, string discountId, [FromServices] IVendorService vendorService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendors = await vendorService.GetAllVendorsByDiscount(discount.Id);
            var gridModel = new DataSourceResult {
                Data = vendors.Select(x => new DiscountModel.AppliedToVendorModel {
                    VendorId = x.Id,
                    VendorName = x.Name
                }),
                Total = vendors.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> VendorDelete(string discountId, string vendorId, [FromServices] IVendorService vendorService)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var vendor = await vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new Exception("No vendor found with the specified id");
            if (ModelState.IsValid)
            {
                await _discountViewModelService.DeleteVendor(discount, vendor);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public IActionResult VendorAddPopup(string discountId)
        {
            var model = new DiscountModel.AddVendorToDiscountModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> VendorAddPopupList(DataSourceRequest command, DiscountModel.AddVendorToDiscountModel model, [FromServices] IVendorService vendorService)
        {
            var vendors = await vendorService.GetAllVendors(model.SearchVendorName, command.Page - 1, command.PageSize, true);

            //search for emails
            if (!(string.IsNullOrEmpty(model.SearchVendorEmail)))
            {
                var tempVendors = vendors.Where(x => x.Email.ToLowerInvariant().Contains(model.SearchVendorEmail.Trim()));
                vendors = new PagedList<Domain.Vendors.Vendor>(tempVendors, command.Page - 1, command.PageSize);
            }

            var gridModel = new DataSourceResult {
                Data = vendors.Select(x => x.ToModel()),
                Total = vendors.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> VendorAddPopup(DiscountModel.AddVendorToDiscountModel model)
        {
            var discount = await _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedVendorIds != null)
            {
                await _discountViewModelService.InsertVendorToDiscountModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Discount usage history

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> UsageHistoryList(string discountId, DataSourceRequest command)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var (usageHistoryModels, totalCount) = await _discountViewModelService.PrepareDiscountUsageHistoryModel(discount, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = usageHistoryModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> UsageHistoryDelete(string discountId, string id)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = await _discountService.GetDiscountUsageHistoryById(id);
            if (duh != null)
            {
                if (ModelState.IsValid)
                {
                    await _discountService.DeleteDiscountUsageHistory(duh);
                }
                else
                    return ErrorForKendoGridJson(ModelState);
            }
            return new NullJsonResult();
        }

        #endregion
    }
}
