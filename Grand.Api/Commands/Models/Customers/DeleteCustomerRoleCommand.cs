using Grand.Api.DTOs.Customers;
using MediatR;

namespace Grand.Api.Commands.Models.Customers
{
    public class DeleteCustomerRoleCommand : IRequest<bool>
    {
        public CustomerRoleDto Model { get; set; }
    }
}
