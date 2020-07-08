using Grand.Domain.Customers;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetPasswordRecoveryConfirm : IRequest<PasswordRecoveryConfirmModel>
    {
        public Customer Customer { get; set; }
        public string Token { get; set; }
    }
}
