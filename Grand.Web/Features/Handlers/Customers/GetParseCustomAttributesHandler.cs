using Grand.Domain.Catalog;
using Grand.Services.Customers;
using Grand.Web.Features.Models.Customers;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetParseCustomAttributesHandler : IRequestHandler<GetParseCustomAttributes, string>
    {
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeParser _customerAttributeParser;

        public GetParseCustomAttributesHandler(ICustomerAttributeService customerAttributeService, ICustomerAttributeParser customerAttributeParser)
        {
            _customerAttributeService = customerAttributeService;
            _customerAttributeParser = customerAttributeParser;
        }

        public async Task<string> Handle(GetParseCustomAttributes request, CancellationToken cancellationToken)
        {
            string attributesXml = "";
            var attributes = await _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("customer_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = request.Form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = request.Form[controlId].ToString();
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!String.IsNullOrEmpty(item))
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.CustomerAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = request.Form[controlId].ToString();
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                            else
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, "");

                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }
            return attributesXml;
        }
    }
}
