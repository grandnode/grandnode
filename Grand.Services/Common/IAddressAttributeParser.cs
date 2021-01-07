using Grand.Domain.Common;
using Grand.Domain.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address attribute parser interface
    /// </summary>
    public partial interface IAddressAttributeParser
    {
        /// <summary>
        /// Gets selected address attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected address attributes</returns>
        Task<IList<AddressAttribute>> ParseAddressAttributes(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Get address attribute values
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Address attribute values</returns>
        Task<IList<AddressAttributeValue>> ParseAddressAttributeValues(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="attribute">Address attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        IList<CustomAttribute> AddAddressAttribute(IList<CustomAttribute> customAttributes, AddressAttribute attribute, string value);

        /// <summary>
        /// Validates address attributes
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
        Task<string> FormatAttributes(
            Language language,
            IList<CustomAttribute> customAttributes,
            string serapator = "<br />",
            bool htmlEncode = true);
    }
}
