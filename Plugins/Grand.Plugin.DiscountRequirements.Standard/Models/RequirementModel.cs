﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.DiscountRequirements.CustomerRoles.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole")]
        public string CustomerRoleId { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}

namespace Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount.Models
{
    public class RequirementModel
    {
        [GrandResourceDisplayName("Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }
    }
}

namespace Grand.Plugin.DiscountRequirements.HasAllProducts.Models
{
    public class RequirementModel
    {
        [GrandResourceDisplayName("Plugins.DiscountRequirements.HasAllProducts.Fields.Products")]
        public string Products { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }

        #region Nested classes

        public partial class AddProductModel : BaseModel
        {
            public AddProductModel()
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

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class ProductModel : BaseEntityModel
        {
            public string Name { get; set; }

            public bool Published { get; set; }
        }
        #endregion
    }
}

namespace Grand.Plugin.DiscountRequirements.HasOneProduct.Models
{
    public class RequirementModel
    {
        [GrandResourceDisplayName("Plugins.DiscountRequirements.HasOneProduct.Fields.Products")]
        public string Products { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }

        #region Nested classes

        public partial class AddProductModel : BaseModel
        {
            public AddProductModel()
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

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class ProductModel : BaseEntityModel
        {
            public string Name { get; set; }

            public bool Published { get; set; }
        }

        #endregion
    }
}

namespace Grand.Plugin.DiscountRules.ShoppingCart.Models
{
    public class RequirementModel
    {
        [GrandResourceDisplayName("Plugins.DiscountRules.ShoppingCart.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }
    }
}