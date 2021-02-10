using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Localization;
using Grand.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address attribute parser
    /// </summary>
    public partial class AddressAttributeParser : IAddressAttributeParser
    {
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ILocalizationService _localizationService;

        public AddressAttributeParser(
            IAddressAttributeService addressAttributeService,
            ILocalizationService localizationService)
        {
            _addressAttributeService = addressAttributeService;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets selected address attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected address attributes</returns>
        public virtual async Task<IList<AddressAttribute>> ParseAddressAttributes(IList<CustomAttribute> customAttributes)
        {
            var result = new List<AddressAttribute>();
            if (!customAttributes.Any())
                return result;

            foreach (var customAttribute in customAttributes.GroupBy(x => x.Key))
            {
                var attribute = await _addressAttributeService.GetAddressAttributeById(customAttribute.Key);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }
            return result;
        }

        /// <summary>
        /// Get address attribute values
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Address attribute values</returns>
        public virtual async Task<IList<AddressAttributeValue>> ParseAddressAttributeValues(IList<CustomAttribute> customAttributes)
        {
            var values = new List<AddressAttributeValue>();
            if (!customAttributes.Any())
                return values;

            var attributes = await ParseAddressAttributes(customAttributes);
            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = customAttributes.Where(x => x.Key == attribute.Id).Select(x => x.Value);
                foreach (var valueStr in valuesStr)
                {
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        var value = attribute.AddressAttributeValues.FirstOrDefault(x => x.Id == valueStr);
                        if (value != null)
                            values.Add(value);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="attribute">Address attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        public virtual IList<CustomAttribute> AddAddressAttribute(IList<CustomAttribute> customAttributes, AddressAttribute attribute, string value)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            customAttributes.Add(new CustomAttribute() { Key = attribute.Id, Value = value });

            return customAttributes;
        }

        /// <summary>
        /// Validates address attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetAttributeWarnings(IList<CustomAttribute> customAttributes)
        {
            var warnings = new List<string>();

            //ensure it's our attributes
            var attributes1 = await ParseAddressAttributes(customAttributes);

            //validate required address attributes (whether they're chosen/selected/entered)
            var attributes2 = await _addressAttributeService.GetAllAddressAttributes();
            foreach (var a2 in attributes2)
            {
                if (a2.IsRequired)
                {
                    bool found = false;
                    //selected address attributes
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id == a2.Id)
                        {
                            var valuesStr = customAttributes.Where(x => x.Key == a1.Id).Select(x => x.Value);
                            foreach (var str1 in valuesStr)
                            {
                                if (!string.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        var notFoundWarning = string.Format(_localizationService.GetResource("ShoppingCart.SelectAttribute"), a2.GetLocalized(a => a.Name, ""));
                        warnings.Add(notFoundWarning);
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="language">Languages</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>Attributes</returns>
        public virtual async Task<string> FormatAttributes(
            Language language,
            IList<CustomAttribute> customAttributes,
            string serapator = "<br />",
            bool htmlEncode = true)
        {
            var result = new StringBuilder();
            if (customAttributes == null)
                return result.ToString();

            var attributes = await ParseAddressAttributes(customAttributes);
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = customAttributes.Where(x => x.Key == attribute.Id).Select(x => x.Value).ToList();
                for (int j = 0; j < valuesStr.Count; j++)
                {
                    string valueStr = valuesStr[j];
                    string formattedAttribute = "";
                    if (!attribute.ShouldHaveValues())
                    {
                        //no values
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            var attributeName = attribute.GetLocalized(a => a.Name, language.Id);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            formattedAttribute = string.Format("{0}: {1}", attributeName, FormatText.ConvertText(valueStr));
                            //we never encode multiline textbox input
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            //not supported for address attributes
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, language.Id), valueStr);
                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }
                    else
                    {
                        string attributeValueId = valueStr;
                        var attributeValue = attribute.AddressAttributeValues.FirstOrDefault(x => x.Id == attributeValueId);
                        if (attributeValue != null)
                        {
                            formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, language.Id), attributeValue.GetLocalized(a => a.Name, language.Id));
                        }
                        //encode (if required)
                        if (htmlEncode)
                            formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                    }

                    if (!string.IsNullOrEmpty(formattedAttribute))
                    {
                        if (i != 0 || j != 0)
                            result.Append(serapator);
                        result.Append(formattedAttribute);
                    }
                }
            }

            return result.ToString();
        }

    }
}
