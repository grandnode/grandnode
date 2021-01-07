using Grand.Domain.Catalog;
using Grand.Domain.Common;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product attribute parser interface
    /// </summary>
    public partial interface IProductAttributeParser
    {
        #region Product attributes

        /// <summary>
        /// Gets selected product attribute mappings
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Selected product attribute mappings</returns>
        IList<ProductAttributeMapping> ParseProductAttributeMappings(Product product, IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Get product attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Product attribute values</returns>
        IList<ProductAttributeValue> ParseProductAttributeValues(Product product, IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Gets selected product attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="productAttributeMappingId">Product attribute mapping identifier</param>
        /// <returns>Product attribute values</returns>
        IList<string> ParseValues(IList<CustomAttribute> customAttributes, string productAttributeMappingId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="customattributes">Attributes</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        IList<CustomAttribute> AddProductAttribute(IList<CustomAttribute> customAttributes, ProductAttributeMapping productAttributeMapping, string value);

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="customAttributesXml">Attributes</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Updated result</returns>
        IList<CustomAttribute> RemoveProductAttribute(IList<CustomAttribute> customAttributes, ProductAttributeMapping productAttributeMapping);


        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="customAttributes1">The attributes of the first product</param>
        /// <param name="customAttributes2">The attributes of the second product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Result</returns>
        bool AreProductAttributesEqual(Product product, IList<CustomAttribute> customAttributes1, IList<CustomAttribute> customAttributes2, bool ignoreNonCombinableAttributes);

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="pam">Product attribute</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Result</returns>
        bool? IsConditionMet(Product product, ProductAttributeMapping pam, IList<CustomAttribute> selectedAttributes);

        /// <summary>
        /// Finds a product attribute combination by attributes stored in XML 
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Found product attribute combination</returns>
        ProductAttributeCombination FindProductAttributeCombination(Product product,
            IList<CustomAttribute> customAttributes, bool ignoreNonCombinableAttributes = true);

        /// <summary>
        /// Generate all combinations
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Attribute combinations</returns>
        IList<IEnumerable<CustomAttribute>> GenerateAllCombinations(Product product, bool ignoreNonCombinableAttributes = false);

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        IList<CustomAttribute> AddGiftCardAttribute(IList<CustomAttribute> customAttributes, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage);

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        void GetGiftCardAttribute(IList<CustomAttribute> customAttributes, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage);

        #endregion
    }
}
