using Grand.Core.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Common
{
    public partial class AddressAttributeModel : BaseGrandEntityModel
    {
        public AddressAttributeModel()
        {
            Values = new List<AddressAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<AddressAttributeValueModel> Values { get; set; }
    }

    public partial class AddressAttributeValueModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}