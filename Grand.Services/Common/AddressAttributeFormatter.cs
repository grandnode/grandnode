using System;
using System.Linq;
using System.Text;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Html;
using Grand.Services.Localization;
using System.Net;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address attribute helper
    /// </summary>
    public partial class AddressAttributeFormatter : IAddressAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;

        public AddressAttributeFormatter(IWorkContext workContext,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            this._workContext = workContext;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeParser = addressAttributeParser;
        }
        
        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>Attributes</returns>
        public virtual string FormatAttributes(string attributesXml,
            string serapator = "<br />", 
            bool htmlEncode = true)
        {
            var result = new StringBuilder();

            var attributes = _addressAttributeParser.ParseAddressAttributes(attributesXml);
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _addressAttributeParser.ParseValues(attributesXml, attribute.Id);
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
                            var attributeName = attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            formattedAttribute = string.Format("{0}: {1}", attributeName, HtmlHelper.FormatText(valueStr, false, true, false, false, false, false));
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
                            formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), valueStr);
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
                            formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                        }
                        //encode (if required)
                        if (htmlEncode)
                            formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                    }

                    if (!String.IsNullOrEmpty(formattedAttribute))
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
