using Grand.Domain.Customers;
using Grand.Domain.Stores;
using MediatR;

namespace Grand.Web.Commands.Models.Customers
{
    public class DeleteAccountCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
