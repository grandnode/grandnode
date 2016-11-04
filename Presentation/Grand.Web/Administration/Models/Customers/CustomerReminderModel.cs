using FluentValidation.Attributes;
using Grand.Admin.Validators.Customers;
using Grand.Core.Domain.Customers;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Grand.Admin.Models.Customers
{
    [Validator(typeof(CustomerReminderValidator))]
    public partial class CustomerReminderModel : BaseNopEntityModel
    {
        public CustomerReminderModel()
        {
        }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.StartDate")]
        public DateTime StartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.EndDate")]
        public DateTime EndDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.LastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.AllowRenew")]
        public bool AllowRenew { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.RenewedDay")]
        public int RenewedDay { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.ReminderRule")]
        public int ReminderRuleId { get; set; }
        
        [NopResourceDisplayName("Admin.Customers.CustomerReminder.Fields.ConditionId")]
        public int ConditionId { get; set; }
        public int ConditionCount { get; set; }


        public partial class ConditionModel : BaseNopEntityModel
        {
            public ConditionModel()
            {
                this.ConditionType = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Condition.Fields.Name")]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Condition.Fields.ConditionTypeId")]
            public int ConditionTypeId { get; set; }
            public IList<SelectListItem> ConditionType { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Condition.Fields.ConditionId")]
            public int ConditionId { get; set; }

            public string CustomerReminderId { get; set; }

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
                public string SearchCategoryId { get; set; }
                [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
                public string SearchManufacturerId { get; set; }
                [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
                public string SearchStoreId { get; set; }
                [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
                public string SearchVendorId { get; set; }
                [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
                public int SearchProductTypeId { get; set; }

                public IList<SelectListItem> AvailableCategories { get; set; }
                public IList<SelectListItem> AvailableManufacturers { get; set; }
                public IList<SelectListItem> AvailableStores { get; set; }
                public IList<SelectListItem> AvailableVendors { get; set; }
                public IList<SelectListItem> AvailableProductTypes { get; set; }

                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }

                public string[] SelectedProductIds { get; set; }
            }
            public partial class AddCategoryConditionModel
            {
                [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
                [AllowHtml]
                public string SearchCategoryName { get; set; }

                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }

                public string[] SelectedCategoryIds { get; set; }
            }
            public partial class AddManufacturerConditionModel
            {
                [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
                [AllowHtml]
                public string SearchManufacturerName { get; set; }

                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }

                public string[] SelectedManufacturerIds { get; set; }
            }
            public partial class AddCustomerRoleConditionModel
            {
                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }

                public string CustomerRoleId { get; set; }
                public string Id { get; set; }
            }
            public partial class AddCustomerTagConditionModel
            {
                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }

                public string CustomerTagId { get; set; }
                public string Id { get; set; }
            }
            public partial class AddCustomerRegisterConditionModel
            {
                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }
                public string CustomerRegisterName { get; set; }
                public string CustomerRegisterValue { get; set; }
                public string Id { get; set; }
            }
            public partial class AddCustomCustomerAttributeConditionModel
            {
                public string Id { get; set; }
                public string CustomerReminderId { get; set; }
                public string ConditionId { get; set; }
                public string CustomerAttributeName { get; set; }
                public string CustomerAttributeValue { get; set; }
            }

        }

        [Validator(typeof(CustomerReminderLevelValidator))]
        public partial class ReminderLevelModel : BaseNopEntityModel
        {
            public ReminderLevelModel()
            {
                EmailAccounts = new List<SelectListItem>();
            }

            public string CustomerReminderId { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.SendDay")]
            public int Day { get; set; }
            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.SendHour")]
            public int Hour { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.SendMinutes")]
            public int Minutes { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.Name")]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.AllowedTokens")]
            public string AllowedTokens { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.Level")]
            public int Level { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.EmailAccountId")]
            public string EmailAccountId { get; set; }
            public IList<SelectListItem> EmailAccounts { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.BccEmailAddresses")]
            public string BccEmailAddresses { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.Subject")]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Customers.CustomerReminder.Level.Fields.Body")]
            [AllowHtml]
            public string Body { get; set; }
        }

    }



}