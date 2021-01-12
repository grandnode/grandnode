using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product attribute parser
    /// </summary>
    public partial class ProductAttributeParser : IProductAttributeParser
    {

        #region Ctor

        public ProductAttributeParser()
        {
        }

        #endregion

        #region Product attributes

        /// <summary>
        /// Gets selected product attribute mappings
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Selected product attribute mappings</returns>
        public virtual IList<ProductAttributeMapping> ParseProductAttributeMappings(Product product, IList<CustomAttribute> customAttributes)
        {
            var result = new List<ProductAttributeMapping>();
            if (customAttributes == null || !customAttributes.Any())
                return result;

            foreach (var customAttribute in customAttributes.GroupBy(x => x.Key))
            {
                var attribute = product.ProductAttributeMappings.Where(x => x.Id == customAttribute.Key).FirstOrDefault();
                if (attribute != null)
                {
                    attribute.ProductId = product.Id;
                    result.Add(attribute);
                }
            }
            return result;
        }

        /// <summary>
        /// Get product attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Product attribute values</returns>
        public virtual IList<ProductAttributeValue> ParseProductAttributeValues(Product product, IList<CustomAttribute> customAttributes)
        {
            var values = new List<ProductAttributeValue>();
            if (customAttributes == null || !customAttributes.Any())
                return values;

            var attributes = ParseProductAttributeMappings(product, customAttributes);
            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = ParseValues(customAttributes, attribute.Id);
                foreach (var valueStr in valuesStr)
                {
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        if (attribute.ProductAttributeValues.Where(x => x.Id == valueStr).Count() > 0)
                        {
                            var value = attribute.ProductAttributeValues.Where(x => x.Id == valueStr).FirstOrDefault();
                            if (value != null)
                            {
                                value.ProductId = product.Id;
                                value.ProductAttributeMappingId = attribute.Id;
                                values.Add(value);
                            }
                        }
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Gets selected product attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes </param>
        /// <param name="productAttributeMappingId">Product attribute mapping identifier</param>
        /// <returns>Product attribute values</returns>
        public virtual IList<string> ParseValues(IList<CustomAttribute> customAttributes, string productAttributeMappingId)
        {
            var selectedValues = new List<string>();
            if (customAttributes == null || !customAttributes.Any())
                return selectedValues;

            return customAttributes.Where(x => x.Key == productAttributeMappingId).Select(x => x.Value).ToList();

        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        public virtual IList<CustomAttribute> AddProductAttribute(IList<CustomAttribute> customAttributes, ProductAttributeMapping productAttributeMapping, string value)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            customAttributes.Add(new CustomAttribute() { Key = productAttributeMapping.Id, Value = value });

            return customAttributes;
        }
        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Updated result (XML format)</returns>
        public virtual IList<CustomAttribute> RemoveProductAttribute(IList<CustomAttribute> customAttributes, ProductAttributeMapping productAttributeMapping)
        {
            return customAttributes.Where(x => x.Key != productAttributeMapping.Id).ToList();
        }
        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="customAttributes1">The attributes of the first product</param>
        /// <param name="customAttributes2">The attributes of the second product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Result</returns>
        public virtual bool AreProductAttributesEqual(Product product, IList<CustomAttribute> customAttributes1, IList<CustomAttribute> customAttributes2, bool ignoreNonCombinableAttributes)
        {
            var attributes1 = ParseProductAttributeMappings(product, customAttributes1);
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
            }
            var attributes2 = ParseProductAttributeMappings(product, customAttributes2);
            //TO DO - Where(x=>x.IsRequired).ToList()

            if (ignoreNonCombinableAttributes)
            {
                attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
            }
            if (attributes1.Count != attributes2.Count)
                return false;

            bool attributesEqual = true;
            foreach (var a1 in attributes1)
            {
                bool hasAttribute = false;
                foreach (var a2 in attributes2)
                {
                    if (a1.Id == a2.Id)
                    {
                        hasAttribute = true;
                        var values1Str = ParseValues(customAttributes1, a1.Id);
                        var values2Str = ParseValues(customAttributes2, a2.Id);
                        if (values1Str.Count == values2Str.Count)
                        {
                            foreach (string str1 in values1Str)
                            {
                                bool hasValue = false;
                                foreach (string str2 in values2Str)
                                {
                                    //case insensitive? 
                                    if (str1.Trim() == str2.Trim())
                                    {
                                        hasValue = true;
                                        break;
                                    }
                                }

                                if (!hasValue)
                                {
                                    attributesEqual = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            attributesEqual = false;
                            break;
                        }
                    }
                }

                if (hasAttribute == false)
                {
                    attributesEqual = false;
                    break;
                }
            }

            return attributesEqual;
        }

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="pam">Product attribute</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Result</returns>
        public virtual bool? IsConditionMet(Product product, ProductAttributeMapping pam, IList<CustomAttribute> selectedAttributes)
        {
            if (pam == null)
                throw new ArgumentNullException("pam");

            if (selectedAttributes == null)
                selectedAttributes = new List<CustomAttribute>();

            var conditionAttribute = pam.ConditionAttribute;
            if (!conditionAttribute.Any())
                //no condition
                return null;

            //load an attribute this one depends on
            var dependOnAttribute = ParseProductAttributeMappings(product, conditionAttribute).FirstOrDefault();
            if (dependOnAttribute == null)
                return true;

            var valuesThatShouldBeSelected = ParseValues(conditionAttribute, dependOnAttribute.Id)
                //a workaround here:
                //ConditionAttribute can contain "empty" values (nothing is selected)
                //but in other cases (like below) we do not store empty values
                //that's why we remove empty values here
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var selectedValues = ParseValues(selectedAttributes, dependOnAttribute.Id);
            if (valuesThatShouldBeSelected.Count != selectedValues.Count)
                return false;

            //compare values
            var allFound = true;
            foreach (var t1 in valuesThatShouldBeSelected)
            {
                bool found = false;
                foreach (var t2 in selectedValues)
                    if (t1 == t2)
                        found = true;
                if (!found)
                    allFound = false;
            }

            return allFound;
        }

        /// <summary>
        /// Finds a product attribute combination by attributes stored in XML 
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Found product attribute combination</returns>
        public virtual ProductAttributeCombination FindProductAttributeCombination(Product product,
            IList<CustomAttribute> customAttributes, bool ignoreNonCombinableAttributes = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var combinations = product.ProductAttributeCombinations;
            return combinations.FirstOrDefault(x =>
                AreProductAttributesEqual(product, x.Attributes, customAttributes, ignoreNonCombinableAttributes));
        }

        /// <summary>
        /// Generate all combinations
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Attribute combinations</returns>
        public virtual IList<IEnumerable<CustomAttribute>> GenerateAllCombinations(Product product, bool ignoreNonCombinableAttributes = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var allProductAttributMappings = product.ProductAttributeMappings;
            //TO DO .Where(x => x.IsRequired).ToList()

            if (ignoreNonCombinableAttributes)
            {
                allProductAttributMappings = allProductAttributMappings.Where(x => !x.IsNonCombinable()).ToList();
            }

            var query = allProductAttributMappings.Select(o1 => o1.ProductAttributeValues.Select(o2 => 
                new CustomAttribute 
                {
                        Key = o1.Id,
                        Value = o2.Id
                }
                )).SelectMany(x => x).ToList();

            var result = query.GroupBy(t => t.Key).CartesianProduct().ToList();

            return result;
        }

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        public IList<CustomAttribute> AddGiftCardAttribute(IList<CustomAttribute> customAttributes, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            customAttributes.Add(new CustomAttribute() { Key = "RecipientName", Value = recipientName });
            customAttributes.Add(new CustomAttribute() { Key = "RecipientEmail", Value = recipientEmail });
            customAttributes.Add(new CustomAttribute() { Key = "SenderName", Value = senderName });
            customAttributes.Add(new CustomAttribute() { Key = "SenderEmail", Value = senderEmail });
            customAttributes.Add(new CustomAttribute() { Key = "Message", Value = giftCardMessage });

            return customAttributes;
        }

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        public void GetGiftCardAttribute(IList<CustomAttribute> customAttributes, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            recipientName = customAttributes.FirstOrDefault(x => x.Key == "RecipientName")?.Value;
            recipientEmail = customAttributes.FirstOrDefault(x => x.Key == "RecipientEmail")?.Value; ;
            senderName = customAttributes.FirstOrDefault(x => x.Key == "SenderName")?.Value; ;
            senderEmail = customAttributes.FirstOrDefault(x => x.Key == "SenderEmail")?.Value; ;
            giftCardMessage = customAttributes.FirstOrDefault(x => x.Key == "Message")?.Value; ;

        }

        #endregion
    }
}
