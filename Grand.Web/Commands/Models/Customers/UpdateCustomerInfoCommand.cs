using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Commands.Models.Customers
{
    public class UpdateCustomerInfoCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public CustomerInfoModel Model { get; set; }
        public IFormCollection Form { get; set; }
        public string CustomerAttributesXml { get; set; }
        public Customer OriginalCustomerIfImpersonated { get; set; }
    }
}
