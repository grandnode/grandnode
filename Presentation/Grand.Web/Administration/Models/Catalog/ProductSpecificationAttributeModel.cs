using System.Web.Mvc;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Catalog
{
    public partial class ProductSpecificationAttributeModel : BaseNopEntityModel
    {
        public int AttributeTypeId { get; set; }

        [AllowHtml]
        public string AttributeTypeName { get; set; }

        [AllowHtml]
        public string AttributeName { get; set; }

        public string AttributeId { get; set; }

        [AllowHtml]
        public string ValueRaw { get; set; }

        public bool AllowFiltering { get; set; }

        public bool ShowOnProductPage { get; set; }

        public int DisplayOrder { get; set; }

        public string ProductSpecificationId { get; set; }

        public string SpecificationAttributeOptionId { get; set; }

        public string ProductId { get; set; }
    }
}