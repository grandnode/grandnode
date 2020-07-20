using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductAttributeDto : BaseApiEntityModel
    {
        public ProductAttributeDto()
        {
            PredefinedProductAttributeValues = new List<PredefinedProductAttributeValueDto>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<PredefinedProductAttributeValueDto> PredefinedProductAttributeValues { get; set; }
    }

    public partial class PredefinedProductAttributeValueDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public decimal PriceAdjustment { get; set; }
        public decimal WeightAdjustment { get; set; }
        public decimal Cost { get; set; }
        public bool IsPreSelected { get; set; }
        public int DisplayOrder { get; set; }
    }
}
