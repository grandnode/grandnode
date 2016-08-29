using System.Web.Mvc;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Catalog
{
    public partial class ProductSpecificationAttributeModel : BaseNopEntityModel
    {
        [AllowHtml]
        public string AttributeTypeName { get; set; }

        [AllowHtml]
        public string AttributeName { get; set; }

        [AllowHtml]
        public string ValueRaw { get; set; }

        public bool AllowFiltering { get; set; }

        public bool ShowOnProductPage { get; set; }

        public int DisplayOrder { get; set; }
        public string ProductSpecificationId { get; set; }
        public string ProductId { get; set; }
    }
}