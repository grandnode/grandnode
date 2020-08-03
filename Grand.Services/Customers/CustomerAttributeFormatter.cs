using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Core.Html;
using Grand.Services.Localization;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial class CustomerAttributeFormatter : ICustomerAttributeFormatter
    {
        #region Fields

        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomerAttributeFormatter(ICustomerAttributeParser customerAttributeParser,
            IWorkContext workContext)
        {
            _customerAttributeParser = customerAttributeParser;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>Attributes</returns>
        public virtual async Task<string> FormatAttributes(string attributesXml, string serapator = "<br />", bool htmlEncode = true)
        {
            var result = new StringBuilder();

            var attributes = await _customerAttributeParser.ParseCustomerAttributes(attributesXml);
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _customerAttributeParser.ParseValues(attributesXml, attribute.Id);
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
                            //not supported for customer attributes
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
                        var attributeValue = attribute.CustomerAttributeValues.FirstOrDefault(x => x.Id == attributeValueId);
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

        #endregion
    }
}
