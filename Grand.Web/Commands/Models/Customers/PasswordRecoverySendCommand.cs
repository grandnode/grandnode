using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Commands.Models.Customers
{
    public class PasswordRecoverySendCommand : IRequest<bool>
    {
        public PasswordRecoveryModel Model { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
