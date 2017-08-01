using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(CampaignValidator))]
    public partial class CampaignModel : BaseGrandEntityModel
    {
        public CampaignModel()
        {
            this.AvailableStores = new List<SelectListItem>();
            this.AvailableCustomerTags = new List<SelectListItem>();
            this.CustomerTags = new List<string>();
            this.NewsletterCategories = new List<string>();
            this.AvailableCustomerRoles = new List<SelectListItem>();
            this.CustomerRoles = new List<string>();
            this.AvailableEmailAccounts = new List<EmailAccountModel>();
        }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.Subject")]
        
        public string Subject { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.Body")]
        
        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }


        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerCreatedDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerCreatedDateFrom { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerCreatedDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerCreatedDateTo { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastActivityDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastActivityDateFrom { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastActivityDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastActivityDateTo { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastPurchaseDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastPurchaseDateFrom { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastPurchaseDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastPurchaseDateTo { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerHasOrders")]
        public int CustomerHasOrders { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerHasShoppingCart")]
        public int CustomerHasShoppingCart { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerTags")]
        public IList<SelectListItem> AvailableCustomerTags { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerTags")]
        [UIHint("MultiSelect")]
        public IList<string> CustomerTags { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.NewsletterCategory")]
        [UIHint("MultiSelect")]
        public IList<string> NewsletterCategories { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.NewsletterCategory")]
        public IList<SelectListItem> AvailableNewsletterCategories { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerRoles")]
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerRoles")]
        [UIHint("MultiSelect")]
        public IList<string> CustomerRoles { get; set; }


        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.EmailAccount")]
        public string EmailAccountId { get; set; }
        public IList<EmailAccountModel> AvailableEmailAccounts { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        
        public string TestEmail { get; set; }
    }
}