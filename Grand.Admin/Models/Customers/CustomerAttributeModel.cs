using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Customers
{
    public partial class CustomerAttributeModel : BaseEntityModel, ILocalizedModel<CustomerAttributeLocalizedModel>
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