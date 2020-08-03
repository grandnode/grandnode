using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Discounts;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.DiscountRequirements.HasOneProduct.Models;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.DiscountRequirements.HasOneProduct.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class HasOneProductController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;

        public HasOneProductController(IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IProductService productService)
        {
            _discountService = discountService;
            _settingService = settingService;
            _permissionService = permissionService;
            _workContext = workContext;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _productService = productService;
        }

        public async Task<IActionResult> Configure(string discountId, string discountRequirementId)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            if (!String.IsNullOrEmpty(discountRequirementId))
            {
                var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var restrictedProductIds = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirements.Standard.RestrictedProductIds-{0}-{1}", discount.Id, !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : ""));

            var model = new RequirementModel();
            model.RequirementId = !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "";
            model.DiscountId = discountId;
            model.Products = restrictedProductIds;

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRequirementsHasOneProduct{0}-{1}", discount.Id, !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "");

            return View("~/Plugins/DiscountRequirements.Standard/Views/HasOneProduct/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(string discountId, string discountRequirementId, string productIds)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (!String.IsNullOrEmpty(discountRequirementId))
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);

            if (discountRequirement != null)
            {
                //update existing rule
                await _settingService.SetSetting(string.Format("DiscountRequirements.Standard.RestrictedProductIds-{0}-{1}", discount.Id, discountRequirement.Id), productIds);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirements.HasOneProduct"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                await _discountService.UpdateDiscount(discount);

                await _settingService.SetSetting(string.Format("DiscountRequirements.Standard.RestrictedProductIds-{0}-{1}", discount.Id, discountRequirement.Id), productIds);
            }
            return new JsonResult(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        public async Task<IActionResult> ProductAddPopup(string btnId, string productIdsInput)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            var model = new RequirementModel.AddProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(HttpContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });


            ViewBag.productIdsInput = productIdsInput;
            ViewBag.btnId = btnId;

            return View("~/Plugins/DiscountRequirements.Standard/Views/HasOneProduct/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, RequirementModel.AddProductModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = (await _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                )).products;
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => new RequirementModel.ProductModel
            {
                Id = x.Id,
                Name = x.Name,
                Published = x.Published
            });
            gridModel.Total = products.TotalCount;

            return new JsonResult(gridModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> LoadProductFriendlyNames(string productIds)
        {
            var result = "";

            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return new JsonResult(new { Text = result });

            if (!String.IsNullOrWhiteSpace(productIds))
            {
                var ids = new List<string>();
                var rangeArray = productIds
                    .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                //we support three ways of specifying products:
                //1. The comma-separated list of product identifiers (e.g. 77, 123, 156).
                //2. The comma-separated list of product identifiers with quantities.
                //      {Product ID}:{Quantity}. For example, 77:1, 123:2, 156:3
                //3. The comma-separated list of product identifiers with quantity range.
                //      {Product ID}:{Min quantity}-{Max quantity}. For example, 77:1-3, 123:2-5, 156:3-8
                foreach (string str1 in rangeArray)
                {
                    var str2 = str1;
                    //we do not display specified quantities and ranges
                    //so let's parse only product names (before : sign)
                    if (str2.Contains(":"))
                        str2 = str2.Substring(0, str2.IndexOf(":"));

                    ids.Add(str2);
                }

                var products = await _productService.GetProductsByIds(ids.ToArray(), true);
                for (int i = 0; i <= products.Count - 1; i++)
                {
                    result += products[i].Name;
                    if (i != products.Count - 1)
                        result += ", ";
                }
            }

            return new JsonResult(new { Text = result });
        }
    }
}