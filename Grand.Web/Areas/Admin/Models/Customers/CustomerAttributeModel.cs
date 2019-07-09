using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerAttributeValidator))]
    public partial class CustomerAttributeModel : BaseGrandEntityModel, ILocalizedModel<CustomerAttributeLocalizedModel>
    {
        public CustomerAttributeModel()
        {
            Locales = new List<CustomerAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.AttributeControlType")]
        
        public string AttributeControlTypeName { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        public IList<CustomerAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class CustomerAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerAttributes.Fields.Name")]
        
        public string Name { get; set; }

    }
}