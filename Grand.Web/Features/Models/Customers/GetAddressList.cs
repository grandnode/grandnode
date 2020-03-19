using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetAddressList : IRequest<CustomerAddressListModel>
    {
        public Store Store { get; set; }
        public Customer Customer { get; set; }
        public Language Language { get; set; }
    }
}
