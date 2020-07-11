using Grand.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductSpecificationAttributeDto : BaseApiEntityModel
    {
        public string SpecificationAttributeId { get; set; }
        public string SpecificationAttributeOptionId { get; set; }
        public string CustomValue { get; set; }
        public bool AllowFiltering { get; set; }
        public bool ShowOnProductPage { get; set; }
        public int DisplayOrder { get; set; }
        [BsonElement("AttributeTypeId")]
        public SpecificationAttributeType AttributeType { get; set; }
    }
}
