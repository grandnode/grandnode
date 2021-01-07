using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Common;
using Grand.Web.Features.Models.Common;
using MediatR;

namespace Grand.Web.Features.Handlers.Common
{
    public class GetParseCustomAddressAttributesHandler : IRequestHandler<GetParseCustomAddressAttributes, IList<CustomAttribute>>
    {
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;

        public GetParseCustomAddressAttributesHandler(
            IAddressAttributeService addressAttributeService, 
            IAddressAttributeParser addressAttributeParser)
        {
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
        }

        public async Task<IList<CustomAttribute>> Handle(GetParseCustomAddressAttributes request, CancellationToken cancellationToken)
        {
            if (request.Form == null)
                throw new ArgumentNullException("form");

            var customAttributes = new List<CustomAttribute>();
            var attributes = await _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("address_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = request.Form[controlId];
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                customAttributes = _addressAttributeParser.AddAddressAttribute(customAttributes,
                                    attribute, ctrlAttributes).ToList();
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
                                    if (!string.IsNullOrEmpty(item))
                                        customAttributes = _addressAttributeParser.AddAddressAttribute(customAttributes,
                                            attribute, item).ToList();
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.AddressAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                customAttributes = _addressAttributeParser.AddAddressAttribute(customAttributes,
                                            attribute, selectedAttributeId.ToString()).ToList();
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = request.Form[controlId].ToString();
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                customAttributes = _addressAttributeParser.AddAddressAttribute(customAttributes,
                                    attribute, enteredText).ToList();
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported address attributes
                    default:
                        break;
                }
            }

            return customAttributes;
        }
    }
}
