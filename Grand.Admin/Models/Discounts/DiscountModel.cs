using Grand.Framework.Mapping;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grand.Framework.Mvc.Models;

namespace Grand.Admin.Models.Discounts
{
    public partial class DiscountModel : BaseEntityModel, IStoreMappingModel
    {
        public DiscountModel()
        {
            AvailableDiscountRequirementRules = new List<SelectListItem>();
            DiscountRequirementMetaInfos = new List<DiscountRequirementMetaInfo>();
            AvailableDiscountAmountProviders = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountType")]
        public int DiscountTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountType")]
        public string DiscountTypeName { get; set; }

        //used for the list page
        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.TimesUsed")]
        public int TimesUsed { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountAmount")]
        public decimal DiscountAmount { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.CalculateByPlugin")]
        public bool CalculateByPlugin { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountPluginName")]
        public string DiscountPluginName { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.MaximumDiscountAmount")]
        [UIHint("DecimalNullable")]
        public decimal? MaximumDiscountAmount { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.RequiresCouponCode")]
        public bool RequiresCouponCode { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.Reused")]
        public bool Reused { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.IsCumulative")]
        public bool IsCumulative { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountLimitation")]
        public int DiscountLimitationId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.LimitationTimes")]
        public int LimitationTimes { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.MaximumDiscountedQuantity")]
        [UIHint("Int32Nullable")]
        public int? MaximumDiscountedQuantity { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Requirements.DiscountRequirementType")]
        public string AddDiscountRequirement { get; set; }
        public IList<SelectListItem> AvailableDiscountRequirementRules { get; set; }
        public IList<DiscountRequirementMetaInfo> DiscountRequirementMetaInfos { get; set; }
        public IList<SelectListItem> AvailableDiscountAmountProviders { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }

        #region Nested classes

        public partial class DiscountRequirementMetaInfo : BaseModel
        {
            public string DiscountRequirementId { get; set; }
            public string RuleName { get; set; }
            public string ConfigurationUrl { get; set; }
        }

        public partial class DiscountUsageHistoryModel : BaseEntityModel
        {
            public string DiscountId { get; set; }

            [GrandResourceDisplayName("Admin.Promotions.Discounts.History.Order")]
            public string OrderId { get; set; }
            public int OrderNumber { get; set; }
            public string OrderCode { get; set; }

            [GrandResourceDisplayName("Admin.Promotions.Discounts.History.OrderTotal")]
            public string OrderTotal { get; set; }

            [GrandResourceDisplayName("Admin.Promotions.Discounts.History.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class AppliedToCategoryModel : BaseModel
        {
            public string CategoryId { get; set; }

            public string CategoryName { get; set; }
        }
        public partial class AddCategoryToDiscountModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]

            public string SearchCategoryName { get; set; }

            public string DiscountId { get; set; }

            public string[] SelectedCategoryIds { get; set; }
        }


        public partial class AppliedToManufacturerModel : BaseModel
        {
            public string ManufacturerId { get; set; }

            public string ManufacturerName { get; set; }
        }
        public partial class AddManufacturerToDiscountModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]

            public string SearchManufacturerName { get; set; }

            public string DiscountId { get; set; }

            public string[] SelectedManufacturerIds { get; set; }
        }


        public partial class AppliedToProductModel : BaseModel
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }
        }
        public partial class AddProductToDiscountModel : BaseModel
        {
            public AddProductToDiscountModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]

            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public string DiscountId { get; set; }

            public string[] SelectedProductIds { get; set; }
        }

        public partial class AppliedToVendorModel : BaseModel
        {
            public string VendorId { get; set; }

            public string VendorName { get; set; }
        }
        public partial class AddVendorToDiscountModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Vendors.List.SearchVendorName")]

            public string SearchVendorName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Vendors.List.SearchVendorEmail")]

            public string SearchVendorEmail { get; set; }

            public string DiscountId { get; set; }

            public string[] SelectedVendorIds { get; set; }
        }


        public partial class AppliedToStoreModel : BaseModel
        {
            public string StoreId { get; set; }

            public string StoreName { get; set; }
        }
        public partial class AddStoreToDiscountModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Stores.List.SearchStoreName")]

            public string SearchStoreName { get; set; }

            public string DiscountId { get; set; }

            public string[] SelectedStoreIds { get; set; }
        }

        #endregion
    }
}