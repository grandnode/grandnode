using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(CategoryValidator))]
    public partial class CategoryModel : BaseGrandEntityModel, ILocalizedModel<CategoryLocalizedModel>
    {
        public CategoryModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<CategoryLocalizedModel>();
            AvailableCategoryTemplates = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryTemplate")]
        public string CategoryTemplateId { get; set; }
        public IList<SelectListItem> AvailableCategoryTemplates { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Parent")]
        public string ParentCategoryId { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
        public int PageSize { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.PriceRanges")]
        
        public string PriceRanges { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.FeaturedProductsOnHomaPage")]
        public bool FeaturedProductsOnHomaPage { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Deleted")]
        public bool Deleted { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Flag")]
        public string Flag { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.FlagStyle")]
        public string FlagStyle { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Icon")]
        public string Icon { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.HideOnCatalog")]
        public bool HideOnCatalog { get; set; }

        public IList<CategoryLocalizedModel> Locales { get; set; }

        public string Breadcrumb { get; set; }

        //seach box
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.ShowOnSearchBox")]
        public bool ShowOnSearchBox { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.SearchBoxDisplayOrder")]
        public int SearchBoxDisplayOrder { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


        public IList<SelectListItem> AvailableCategories { get; set; }


        //discounts
        public List<DiscountModel> AvailableDiscounts { get; set; }
        public string[] SelectedDiscountIds { get; set; }


        #region Nested classes

        public partial class CategoryProductModel : BaseGrandEntityModel
        {
            public string CategoryId { get; set; }

            public string ProductId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
            public string ProductName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Categories.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Categories.Products.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class AddCategoryProductModel : BaseGrandModel
        {
            public AddCategoryProductModel()
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

            public string CategoryId { get; set; }

            public string[] SelectedProductIds { get; set; }
        }

        public partial class ActivityLogModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Categories.ActivityLogType")]
            public string ActivityLogTypeName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Categories.ActivityLog.Comment")]
            public string Comment { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Categories.ActivityLog.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Categories.ActivityLog.Customer")]
            public string CustomerId { get; set; }
            public string CustomerEmail { get; set; }
        }


        #endregion
    }

    public partial class CategoryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        
        public string Description {get;set;}

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        
        public string SeName { get; set; }
    }

}