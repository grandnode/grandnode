using FluentValidation.Attributes;
using Nop.Admin.Validators.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Nop.Admin.Models.Customers
{
    [Validator(typeof(CustomerActionConditionValidator))]
    public partial class CustomerActionConditionModel : BaseNopEntityModel
    {
        public CustomerActionConditionModel()
        {
            this.CustomerActionConditionType = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.CustomerConditionTypeId")]
        public int CustomerActionConditionTypeId { get; set; }
        public IList<SelectListItem> CustomerActionConditionType { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.ConditionId")]
        public int ConditionId { get; set; }

        public int CustomerActionId { get; set; }


        public partial class AddProductToConditionModel 
        {
            public AddProductToConditionModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public int CustomerActionId { get; set; }
            public int CustomerActionConditionId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

        public partial class AddCategoryConditionModel 
        {
            [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
            [AllowHtml]
            public string SearchCategoryName { get; set; }

            public int CustomerActionId { get; set; }
            public int CustomerActionConditionId { get; set; }

            public int[] SelectedCategoryIds { get; set; }
        }

        public partial class AddManufacturerConditionModel 
        {
            [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
            [AllowHtml]
            public string SearchManufacturerName { get; set; }

            public int CustomerActionId { get; set; }
            public int CustomerActionConditionId { get; set; }

            public int[] SelectedManufacturerIds { get; set; }
        }

        public partial class AddVendorConditionModel
        {
            public int CustomerActionId { get; set; }
            public int ConditionId { get; set; }

            public int VendorId { get; set; }
            public int Id { get; set; }
        }

        public partial class AddProductAttributeConditionModel
        {
            public int CustomerActionId { get; set; }
            public int ConditionId { get; set; }
            public int ProductAttributeId { get; set; }
            public string Name { get; set; }
            public int Id { get; set; }
        }

        public partial class AddProductSpecificationConditionModel
        {
            public int CustomerActionId { get; set; }
            public int ConditionId { get; set; }
            public int SpecificationId { get; set; }
            public int SpecificationValueId { get; set; }
            public string Name { get; set; }
            public int Id { get; set; }
        }

    }
}