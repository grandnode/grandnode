using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetRegister : IRequest<RegisterModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public RegisterModel Model { get; set; }
        public bool ExcludeProperties { get; set; }
        public string OverrideCustomCustomerAttributesXml { get; set; } = "";
    }
}
