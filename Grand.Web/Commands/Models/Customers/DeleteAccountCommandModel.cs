using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Stores;
using MediatR;

namespace Grand.Web.Commands.Models.Customers
{
    public class DeleteAccountCommandModel : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
