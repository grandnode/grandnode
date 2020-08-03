using Grand.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductAttributeMappingDto : BaseApiEntityModel
    {
        public ProductAttributeMappingDto()
        {
            this.ProductAttributeValues = new List<ProductAttributeValueDto>();
        }
        public string ProductAttributeId { get; set; }
        public string TextPrompt { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public int? ValidationMinLength { get; set; }
        public int? ValidationMaxLength { get; set; }
        public string ValidationFileAllowedExtensions { get; set; }
        public int? ValidationFileMaximumSize { get; set; }
        public string DefaultValue { get; set; }
        public string ConditionAttributeXml { get; set; }
        [BsonElement("AttributeControlTypeId")]
        public AttributeControlType AttributeControlType { get; set; }

        public IList<ProductAttributeValueDto> ProductAttributeValues { get; set; }
        

    }
    public partial class ProductAttributeValueDto: BaseApiEntityModel
    {
        public string AssociatedProductId { get; set; }
        public string Name { get; set; }
        public string ColorSquaresRgb { get; set; }
        public string ImageSquaresPictureId { get; set; }
        public decimal PriceAdjustment { get; set; }
        public decimal WeightAdjustment { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public bool IsPreSelected { get; set; }
        public int DisplayOrder { get; set; }
        public string PictureId { get; set; }
        [BsonElement("AttributeValueTypeId")]
        public AttributeValueType AttributeValueType { get; set; }
    }
}
