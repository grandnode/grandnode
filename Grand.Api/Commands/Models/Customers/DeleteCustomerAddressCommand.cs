using Grand.Api.DTOs.Customers;
using MediatR;

namespace Grand.Api.Commands.Models.Customers
{
    public class DeleteCustomerAddressCommand : IRequest<bool>
    {
        public CustomerDto Customer { get; set; }
        public AddressDto Address { get; set; }
    }
}
