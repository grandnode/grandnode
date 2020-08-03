using Grand.Api.DTOs.Customers;
using MediatR;

namespace Grand.Api.Commands.Models.Customers
{
    public class UpdateCustomerRoleCommand : IRequest<CustomerRoleDto>
    {
        public CustomerRoleDto Model { get; set; }
    }
}
