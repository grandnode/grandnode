﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Messages;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Grand.Admin.Models.Messages
{
    [Validator(typeof(CampaignValidator))]
    public partial class CampaignModel : BaseNopEntityModel
    {
        public CampaignModel()
        {
            this.AvailableStores = new List<SelectListItem>();
            this.AvailableCustomerTags = new List<SelectListItem>();
            this.CustomerTags = new List<string>();
            this.AvailableEmailAccounts = new List<EmailAccountModel>();
        }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }


        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerCreatedDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerCreatedDateFrom { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerCreatedDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerCreatedDateTo { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastActivityDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastActivityDateFrom { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastActivityDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastActivityDateTo { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastPurchaseDateFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastPurchaseDateFrom { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerLastPurchaseDateTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomerLastPurchaseDateTo { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerHasOrders")]
        public int CustomerHasOrders { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerHasShoppingCart")]
        public int CustomerHasShoppingCart { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerTags")]
        public IList<SelectListItem> AvailableCustomerTags { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerTags")]
        [UIHint("MultiSelect")]
        public IList<string> CustomerTags { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.EmailAccount")]
        public string EmailAccountId { get; set; }
        public IList<EmailAccountModel> AvailableEmailAccounts { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        [AllowHtml]
        public string TestEmail { get; set; }
    }
}