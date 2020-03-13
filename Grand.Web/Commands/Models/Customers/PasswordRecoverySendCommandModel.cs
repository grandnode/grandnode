using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Commands.Models.Customers
{
    public class PasswordRecoverySendCommandModel : IRequest<bool>
    {
        public PasswordRecoveryModel Model { get; set; }
        public Customer Customer { get; set; }
        public Language Language { get; set; }
    }
}
