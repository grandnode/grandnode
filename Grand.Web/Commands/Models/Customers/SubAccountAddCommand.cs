using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Services.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Commands.Models.Customers
{
    public class SubAccountAddCommand : IRequest<CustomerRegistrationResult>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public SubAccountModel Model { get; set; }
        public IFormCollection Form { get; set; }
    }
}
