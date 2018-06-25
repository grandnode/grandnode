using Grand.Core.Domain.Customers;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Contact attribute helper
    /// </summary>
    public partial interface IContactAttributeFormatter
    {
        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Attributes</returns>
        string FormatAttributes(string attributesXml);

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        string FormatAttributes(string attributesXml,
            Customer customer, 
            string serapator = "<br />", 
            bool htmlEncode = true,
            bool allowHyperlinks = true);
    }
}
