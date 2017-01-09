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
    [Validator(typeof(CustomerActionValidator))]
    public partial class CustomerActionModel : BaseNopEntityModel
    {
        public CustomerActionModel()
        {
            this.ActionType = new List<SelectListItem>();
            this.Banners = new List<SelectListItem>();
            this.InteractiveForms = new List<SelectListItem>();
            this.MessageTemplates = new List<SelectListItem>();
            this.CustomerRoles = new List<SelectListItem>();
            this.CustomerTags = new List<SelectListItem>();

        }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ActionTypeId")]
        public string ActionTypeId { get; set; }
        public IList<SelectListItem> ActionType { get; set; }


        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ConditionId")]
        public int ConditionId { get; set; }
        public int ConditionCount { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.ReactionType")]
        public int ReactionTypeId { get; set; }
        public CustomerReactionTypeEnum ReactionType
        {
            get { return (CustomerReactionTypeEnum)ReactionTypeId; }
            set { this.ReactionTypeId = (int)value; }
        }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.Banner")]
        public string BannerId { get; set; }
        public IList<SelectListItem> Banners { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.InteractiveForm")]
        public string InteractiveFormId { get; set; }
        public IList<SelectListItem> InteractiveForms { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.MessageTemplate")]
        public string MessageTemplateId { get; set; }
        public IList<SelectListItem> MessageTemplates { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.CustomerRole")]
        public string CustomerRoleId { get; set; }
        public IList<SelectListItem> CustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.CustomerTag")]
        public string CustomerTagId { get; set; }
        public IList<SelectListItem> CustomerTags { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.StartDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime StartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.EndDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime EndDateTimeUtc { get; set; }

    }



}