using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Web.Commands.Models.ShoppingCart;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.ShoppingCart
{
    public class SaveCheckoutAttributesCommandHandler : IRequestHandler<SaveCheckoutAttributesCommand, IList<CustomAttribute>>
    {
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;

        public SaveCheckoutAttributesCommandHandler(
            ICheckoutAttributeService checkoutAttributeService, 
            ICheckoutAttributeParser checkoutAttributeParser, 
            IDownloadService downloadService, 
            IGenericAttributeService genericAttributeService)
        {
            _checkoutAttributeService = checkoutAttributeService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<IList<CustomAttribute>> Handle(SaveCheckoutAttributesCommand request, CancellationToken cancellationToken)
        {
            if (request.Cart == null)
                throw new ArgumentNullException("cart");

            if (request.Form == null)
                throw new ArgumentNullException("form");

            var customAttributes = new List<CustomAttribute>();
            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(request.Store.Id, !request.Cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = request.Form[controlId];
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes,
                                        attribute, ctrlAttributes).ToList();

                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = request.Form[controlId].ToString();
                            if (!string.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes, attribute, item).ToList();
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.CheckoutAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes,
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
                                customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes,
                                    attribute, enteredText).ToList();
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = request.Form[controlId + "_day"];
                            var month = request.Form[controlId + "_month"];
                            var year = request.Form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes,
                                    attribute, selectedDate.Value.ToString("D")).ToList();
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(request.Form[controlId], out downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                customAttributes = _checkoutAttributeParser.AddCheckoutAttribute(customAttributes,
                                           attribute, download.DownloadGuid.ToString()).ToList();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMet(attribute, customAttributes);
                if (conditionMet.HasValue && !conditionMet.Value)
                    customAttributes = _checkoutAttributeParser.RemoveCheckoutAttribute(customAttributes, attribute).ToList();
            }
            await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.CheckoutAttributes, customAttributes, request.Store.Id);

            return customAttributes;
        }
    }
}
