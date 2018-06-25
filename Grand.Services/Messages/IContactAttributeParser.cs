using Grand.Core.Domain.Messages;
using System.Collections.Generic;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Contact attribute parser interface
    /// </summary>
    public partial interface IContactAttributeParser
    {
        /// <summary>
        /// Gets selected contact attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected contact attributes</returns>
        IList<ContactAttribute> ParseContactAttributes(string attributesXml);

        /// <summary>
        /// Get contact attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Contact attribute values</returns>
        IList<ContactAttributeValue> ParseContactAttributeValues(string attributesXml);

        /// <summary>
        /// Gets selected contact attribute value
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="contactAttributeId">Contact attribute identifier</param>
        /// <returns>Contact attribute value</returns>
        IList<string> ParseValues(string attributesXml, string contactAttributeId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ca">Contact attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        string AddContactAttribute(string attributesXml, ContactAttribute ca, string value);

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="attribute">Contact attribute</param>
        /// <param name="selectedAttributesXml">Selected attributes (XML format)</param>
        /// <returns>Result</returns>
        bool? IsConditionMet(ContactAttribute attribute, string selectedAttributesXml);

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="attribute">contact attribute</param>
        /// <returns>Updated result (XML format)</returns>
        string RemoveContactAttribute(string attributesXml, ContactAttribute attribute);
    }
}
