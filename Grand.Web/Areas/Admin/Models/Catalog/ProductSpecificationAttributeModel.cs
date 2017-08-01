using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductSpecificationAttributeModel : BaseGrandEntityModel
    {
        public int AttributeTypeId { get; set; }

        
        public string AttributeTypeName { get; set; }

        
        public string AttributeName { get; set; }

        public string AttributeId { get; set; }

        
        public string ValueRaw { get; set; }

        public bool AllowFiltering { get; set; }

        public bool ShowOnProductPage { get; set; }

        public int DisplayOrder { get; set; }

        public string ProductSpecificationId { get; set; }

        public string SpecificationAttributeOptionId { get; set; }

        public string ProductId { get; set; }
    }
}