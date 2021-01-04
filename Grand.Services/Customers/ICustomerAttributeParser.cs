using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer attribute parser interface
    /// </summary>
    public partial interface ICustomerAttributeParser
    {
        /// <summary>
        /// Gets selected customer attributes
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Selected customer attributes</returns>
        Task<IList<CustomerAttribute>> ParseCustomerAttributes(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Get customer attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Customer attribute values</returns>
        Task<IList<CustomerAttributeValue>> ParseCustomerAttributeValues(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="ca">Customer attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        IList<CustomAttribute> AddCustomerAttribute(IList<CustomAttribute> customAttributes, CustomerAttribute ca, string value);

        /// <summary>
        /// Validates customer attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Warnings</returns>
        Task<IList<string>> GetAttributeWarnings(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>Attributes</returns>
        Task<string> FormatAttributes(Language language, IList<CustomAttribute> customAttributes, string serapator = "<br />", bool htmlEncode = true);

    }
}
