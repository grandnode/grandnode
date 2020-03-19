using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Checkout;
using MediatR;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetShippingAddress : IRequest<CheckoutShippingAddressModel>
    {
        public string SelectedCountryId { get; set; } = null;
        public bool PrePopulateNewAddressWithCustomerFields { get; set; } = false;
        public string OverrideAttributesXml { get; set; } = "";

        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }
        public Language Language { get; set; }
    }
}
