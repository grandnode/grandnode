using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerActionConditionValidator))]
    public partial class CustomerActionConditionModel : BaseGrandEntityModel
    {
        public CustomerActionConditionModel()
        {
            this.CustomerActionConditionType = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.CustomerConditionTypeId")]
        public int CustomerActionConditionTypeId { get; set; }
        public IList<SelectListItem> CustomerActionConditionType { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerActionCondition.Fields.ConditionId")]
        public int ConditionId { get; set; }

        public string CustomerActionId { get; set; }


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

            public string CustomerActionId { get; set; }
            public string CustomerActionConditionId { get; set; }

            public string[] SelectedProductIds { get; set; }
        }

        public partial class AddCategoryConditionModel 
        {
            [GrandResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
            
            public string SearchCategoryName { get; set; }

            public string CustomerActionId { get; set; }
            public string CustomerActionConditionId { get; set; }

            public string[] SelectedCategoryIds { get; set; }
        }

        public partial class AddManufacturerConditionModel 
        {
            [GrandResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
            
            public string SearchManufacturerName { get; set; }

            public string CustomerActionId { get; set; }
            public string CustomerActionConditionId { get; set; }

            public string[] SelectedManufacturerIds { get; set; }
        }

        public partial class AddVendorConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }

            public string VendorId { get; set; }
            public string Id { get; set; }
        }

        public partial class AddCustomerRoleConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }

            public string CustomerRoleId { get; set; }
            public string Id { get; set; }
        }

        public partial class AddStoreConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }

            public string StoreId { get; set; }
            public string Id { get; set; }
        }

        public partial class AddCustomerTagConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }

            public string CustomerTagId { get; set; }
            public string Id { get; set; }
        }

        public partial class AddProductAttributeConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }
            public string ProductAttributeId { get; set; }
            public string Name { get; set; }
            public string Id { get; set; }
        }
        public partial class AddUrlConditionModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }
        }

        public partial class AddCustomerRegisterConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }
            public string CustomerRegisterName { get; set; }
            public string CustomerRegisterValue { get; set; }
            public string Id { get; set; }
        }

        public partial class AddCustomCustomerAttributeConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }
            public string CustomerAttributeName { get; set; }
            public string CustomerAttributeValue { get; set; }
            public string Id { get; set; }
        }

        public partial class AddProductSpecificationConditionModel
        {
            public string CustomerActionId { get; set; }
            public string ConditionId { get; set; }
            public string SpecificationId { get; set; }
            public string SpecificationValueId { get; set; }
            public string Name { get; set; }
            public string Id { get; set; }
        }

    }
}