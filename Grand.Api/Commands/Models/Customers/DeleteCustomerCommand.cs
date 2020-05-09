using MediatR;

namespace Grand.Api.Commands.Models.Customers
{
    public class DeleteCustomerCommand : IRequest<bool>
    {
        public string Email { get; set; }
    }
}
