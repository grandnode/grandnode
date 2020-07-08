using Grand.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerAttributeModel : BaseGrandEntityModel
    {
        public CustomerAttributeModel()
        {
            Values = new List<CustomerAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CustomerAttributeValueModel> Values { get; set; }

    }

    public partial class CustomerAttributeValueModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}