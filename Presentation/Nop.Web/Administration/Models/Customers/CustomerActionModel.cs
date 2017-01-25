using FluentValidation.Attributes;
using Nop.Admin.Validators.Customers;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Customers
{
    [Validator(typeof(CustomerActionValidator))]
    public partial class CustomerActionModel : BaseNopEntityModel
    {
        public CustomerActionModel()
        {
            this.ActionType = new List<SelectListItem>();
            this.Banners = new List<SelectListItem>();
            this.MessageTemplates = new List<SelectListItem>();
            this.CustomerRoles = new List<SelectListItem>();
            this.CustomerTags = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ActionTypeId")]
        public int ActionTypeId { get; set; }
        public IList<SelectListItem> ActionType { get; set; }
        

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ConditionId")]
        public int ConditionId { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ReactionType")]
        public int ReactionTypeId { get; set; }
        public CustomerReactionTypeEnum ReactionType
        {
            get { return (CustomerReactionTypeEnum)ReactionTypeId; }
            set { this.ReactionTypeId = (int)value; }
        }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Banner")]
        public int BannerId { get; set; }
        public IList<SelectListItem> Banners { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.MessageTemplate")]
        public int MessageTemplateId { get; set; }
        public IList<SelectListItem> MessageTemplates { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.CustomerRole")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> CustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.CustomerTag")]
        public int CustomerTagId { get; set; }
        public IList<SelectListItem> CustomerTags { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.StartDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime StartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.EndDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime EndDateTimeUtc { get; set; }

    }



}