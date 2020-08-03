using Grand.Api.DTOs.Customers;
using MediatR;

namespace Grand.Api.Commands.Models.Customers
{
    public class UpdateCustomerCommand : IRequest<CustomerDto>
    {
        public CustomerDto Model { get; set; }
    }
}
