using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Customers;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerAttributeValueValidator))]
    public partial class CustomerAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<CustomerAttributeValueLocalizedModel>
    {
        public CustomerAttributeValueModel()
        {
            Locales = new List<CustomerAttributeValueLocalizedModel>();
        }

        public string CustomerAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Values.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<CustomerAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class CustomerAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Values.Fields.Name")]
        
        public string Name { get; set; }
    }
}