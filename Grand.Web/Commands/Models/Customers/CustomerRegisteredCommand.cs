using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Commands.Models.Customers
{
    public class CustomerRegisteredCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public RegisterModel Model { get; set; }
        public IFormCollection Form { get; set; }
        public IList<CustomAttribute> CustomerAttributes { get; set; }
    }
}
