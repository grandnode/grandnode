using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetTwoFactorAuthentication : IRequest<CustomerInfoModel.TwoFactorAuthenticationModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
    }
}
