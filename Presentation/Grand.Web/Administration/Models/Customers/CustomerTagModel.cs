using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Customers;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Customers
{
    [Validator(typeof(CustomerTagValidator))]
    public partial class CustomerTagModel : BaseNopEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.CustomerTags.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}