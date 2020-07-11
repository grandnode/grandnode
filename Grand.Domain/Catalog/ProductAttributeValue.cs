using Grand.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute value
    /// </summary>
    public partial class ProductAttributeValue : SubBaseEntity, ILocalizedEntity
    {
        public ProductAttributeValue()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the product attribute mapping identifier
        /// </summary>
        public string ProductAttributeMappingId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the attribute value type identifier
        /// </summary>
        public int AttributeValueTypeId { get; set; }

        /// <summary>
        /// Gets or sets the associated product identifier (used only with AttributeValueType.AssociatedToProduct)
        /// </summary>
        public string AssociatedProductId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color RGB value (used with "Color squares" attribute type)
        /// </summary>
        public string ColorSquaresRgb { get; set; }

        /// <summary>
        /// Gets or sets the picture ID for image square (used with "Image squares" attribute type)
        /// </summary>
        public string ImageSquaresPictureId { get; set; }

        /// <summary>
        /// Gets or sets the price adjustment (used only with AttributeValueType.Simple)
        /// </summary>
        public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the weight adjustment (used only with AttributeValueType.Simple)
        /// </summary>
        public decimal WeightAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the attibute value cost (used only with AttributeValueType.Simple)
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the quantity of associated product (used only with AttributeValueType.AssociatedToProduct)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is pre-selected
        /// </summary>
        public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the picture (identifier) associated with this value. This picture should replace a product main picture once clicked (selected).
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
        /// <summary>
        /// Gets or sets the attribute value type
        /// </summary>
        public AttributeValueType AttributeValueType
        {
            get
            {
                return (AttributeValueType)this.AttributeValueTypeId;
            }
            set
            {
                this.AttributeValueTypeId = (int)value;
            }
        }
    }
}
