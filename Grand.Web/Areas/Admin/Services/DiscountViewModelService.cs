using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Vendors;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Discounts;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class DiscountViewModelService : IDiscountViewModelService
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Constructors

        public DiscountViewModelService(IDiscountService discountService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ICategoryService categoryService,
            IProductService productService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IWorkContext workContext,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IDateTimeHelper dateTimeHelper,
            CurrencySettings currencySettings)
        {
            _discountService = discountService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _categoryService = categoryService;
            _productService = productService;
            _webHelper = webHelper;
            _customerActivityService = customerActivityService;
            _workContext = workContext;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _dateTimeHelper = dateTimeHelper;
            _currencySettings = currencySettings;
        }

        #endregion

        public virtual DiscountListModel PrepareDiscountListModel()
        {
            var model = new DiscountListModel {
                AvailableDiscountTypes = DiscountType.AssignedToOrderTotal.ToSelectList(_localizationService,_workContext, false).ToList()
            };
            model.AvailableDiscountTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            return model;
        }
        public virtual async Task<(IEnumerable<DiscountModel> discountModel, int totalCount)> PrepareDiscountModel(DiscountListModel model, int pageIndex, int pageSize)
        {
            DiscountType? discountType = null;
            if (model.SearchDiscountTypeId > 0)
                discountType = (DiscountType)model.SearchDiscountTypeId;
            var discounts = await _discountService.GetAllDiscounts(discountType, _workContext.CurrentCustomer.StaffStoreId,
                model.SearchDiscountCouponCode,
                model.SearchDiscountName,
                true);
            var items = new List<DiscountModel>();
            foreach (var x in discounts.Skip((pageIndex - 1) * pageSize).Take(pageSize))
            {
                var discountModel = x.ToModel(_dateTimeHelper);
                discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
                discountModel.PrimaryStoreCurrencyCode = x.CalculateByPlugin ? "" : (await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
                discountModel.TimesUsed = (await _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1)).TotalCount;
                items.Add(discountModel);
            };
            return (items, discounts.Count);
        }

        public virtual async Task PrepareDiscountModel(DiscountModel model, Discount discount)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
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
                            model.DiscountRequirementMetaInfos.Add(new DiscountModel.DiscountRequirementMetaInfo {
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
        public virtual async Task<Discount> InsertDiscountModel(DiscountModel model)
        {
            var discount = model.ToEntity(_dateTimeHelper);
            await _discountService.InsertDiscount(discount);

            //activity log
            await _customerActivityService.InsertActivity("AddNewDiscount", discount.Id, _localizationService.GetResource("ActivityLog.AddNewDiscount"), discount.Name);
            return discount;
        }

        public virtual async Task<Discount> UpdateDiscountModel(Discount discount, DiscountModel model)
        {
            var prevDiscountType = discount.DiscountType;
            discount = model.ToEntity(discount, _dateTimeHelper);
            await _discountService.UpdateDiscount(discount);

            //clean up old references (if changed) and update "HasDiscountsApplied" properties
            if (prevDiscountType == DiscountType.AssignedToCategories
                && discount.DiscountType != DiscountType.AssignedToCategories)
            {
                //applied to categories
                //_categoryService.
                var categories = await _categoryService.GetAllCategoriesByDiscount(discount.Id);

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
                var manufacturers = await _manufacturerService.GetAllManufacturersByDiscount(discount.Id);
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
                var products = await _productService.GetProductsByDiscount(discount.Id);

                foreach (var p in products)
                {
                    var item = p.AppliedDiscounts.Where(x => x == discount.Id).FirstOrDefault();
                    p.AppliedDiscounts.Remove(item);
                    await _productService.DeleteDiscount(item, p.Id);
                }
            }

            //activity log
            await _customerActivityService.InsertActivity("EditDiscount", discount.Id, _localizationService.GetResource("ActivityLog.EditDiscount"), discount.Name);
            return discount;

        }
        public virtual async Task DeleteDiscount(Discount discount)
        {
            await _discountService.DeleteDiscount(discount);
            //activity log
            await _customerActivityService.InsertActivity("DeleteDiscount", discount.Id, _localizationService.GetResource("ActivityLog.DeleteDiscount"), discount.Name);
        }
        public virtual async Task InsertCouponCode(string discountId, string couponCode)
        {
            var coupon = new DiscountCoupon {
                CouponCode = couponCode.ToUpper(),
                DiscountId = discountId
            };
            await _discountService.InsertDiscountCoupon(coupon);
        }
        public virtual string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, string discountRequirementId)
        {
            if (discountRequirementRule == null)
                throw new ArgumentNullException("discountRequirementRule");

            if (discount == null)
                throw new ArgumentNullException("discount");

            string url = string.Format("{0}{1}", _webHelper.GetStoreLocation(), discountRequirementRule.GetConfigurationUrl(discount.Id, discountRequirementId));
            return url;
        }
        public virtual async Task DeleteDiscountRequirement(DiscountRequirement discountRequirement, Discount discount)
        {
            await _discountService.DeleteDiscountRequirement(discountRequirement);
            discount.DiscountRequirements.Remove(discountRequirement);
            await _discountService.UpdateDiscount(discount);
        }
        public virtual async Task<DiscountModel.AddProductToDiscountModel> PrepareProductToDiscountModel()
        {
            var model = new DiscountModel.AddProductToDiscountModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList().ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            return model;
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(DiscountModel.AddProductToDiscountModel model, int pageIndex, int pageSize)
        {
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task InsertProductToDiscountModel(DiscountModel.AddProductToDiscountModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    if (product.AppliedDiscounts.Count(d => d == model.DiscountId) == 0)
                    {
                        product.AppliedDiscounts.Add(model.DiscountId);
                        await _productService.InsertDiscount(model.DiscountId, product.Id);
                    }
                }
            }
        }
        public virtual async Task DeleteProduct(Discount discount, Product product)
        {
            //remove discount
            if (product.AppliedDiscounts.Count(d => d == discount.Id) > 0)
            {
                product.AppliedDiscounts.Remove(discount.Id);
                await _productService.DeleteDiscount(discount.Id, product.Id);
            }
        }
        public virtual async Task DeleteCategory(Discount discount, Category category)
        {
            //remove discount
            if (category.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                category.AppliedDiscounts.Remove(discount.Id);

            await _categoryService.UpdateCategory(category);
        }
        public virtual async Task InsertCategoryToDiscountModel(DiscountModel.AddCategoryToDiscountModel model)
        {
            foreach (string id in model.SelectedCategoryIds)
            {
                var category = await _categoryService.GetCategoryById(id);
                if (category != null)
                {
                    if (category.AppliedDiscounts.Count(d => d == model.DiscountId) == 0)
                        category.AppliedDiscounts.Add(model.DiscountId);

                    await _categoryService.UpdateCategory(category);
                }
            }
        }
        public virtual async Task DeleteManufacturer(Discount discount, Manufacturer manufacturer)
        {
            //remove discount
            if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                manufacturer.AppliedDiscounts.Remove(discount.Id);

            await _manufacturerService.UpdateManufacturer(manufacturer);
        }
        public virtual async Task InsertManufacturerToDiscountModel(DiscountModel.AddManufacturerToDiscountModel model)
        {
            foreach (string id in model.SelectedManufacturerIds)
            {
                var manufacturer = await _manufacturerService.GetManufacturerById(id);
                if (manufacturer != null)
                {
                    if (manufacturer.AppliedDiscounts.Count(d => d == model.DiscountId) == 0)
                        manufacturer.AppliedDiscounts.Add(model.DiscountId);

                    await _manufacturerService.UpdateManufacturer(manufacturer);
                }
            }
        }
        public virtual async Task DeleteVendor(Discount discount, Vendor vendor)
        {
            //remove discount
            if (vendor.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                vendor.AppliedDiscounts.Remove(discount.Id);

            await _vendorService.UpdateVendor(vendor);
        }
        public virtual async Task InsertVendorToDiscountModel(DiscountModel.AddVendorToDiscountModel model)
        {
            foreach (string id in model.SelectedVendorIds)
            {
                var vendor = await _vendorService.GetVendorById(id);
                if (vendor != null)
                {
                    if (vendor.AppliedDiscounts.Count(d => d == model.DiscountId) == 0)
                        vendor.AppliedDiscounts.Add(model.DiscountId);

                    await _vendorService.UpdateVendor(vendor);
                }
            }
        }

        public virtual async Task<(IEnumerable<DiscountModel.DiscountUsageHistoryModel> usageHistoryModels, int totalCount)> PrepareDiscountUsageHistoryModel(Discount discount, int pageIndex, int pageSize)
        {
            var duh = await _discountService.GetAllDiscountUsageHistory(discount.Id, null, null, null, pageIndex - 1, pageSize);
            var items = new List<DiscountModel.DiscountUsageHistoryModel>();

            foreach (var x in duh)
            {
                var order = await _orderService.GetOrderById(x.OrderId);
                var duhModel = new DiscountModel.DiscountUsageHistoryModel {
                    Id = x.Id,
                    DiscountId = x.DiscountId,
                    OrderId = x.OrderId,
                    OrderNumber = order != null ? order.OrderNumber : 0,
                    OrderCode = order != null ? order.Code : "",
                    OrderTotal = order != null ? _priceFormatter.FormatPrice(order.OrderTotal, true, false) : "",
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                };
                items.Add(duhModel);

            }
            return (items, duh.TotalCount);
        }
    }
}
