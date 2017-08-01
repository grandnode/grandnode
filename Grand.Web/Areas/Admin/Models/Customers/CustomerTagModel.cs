using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Customers;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerTagValidator))]
    public partial class CustomerTagModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.CustomerTags.Fields.Name")]
        
        public string Name { get; set; }
    }
}