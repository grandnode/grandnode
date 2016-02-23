using FluentValidation.Attributes;
using Nop.Admin.Validators.Customers;
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

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.StartDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime StartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerAction.Fields.EndDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime EndDateTimeUtc { get; set; }

    }



}