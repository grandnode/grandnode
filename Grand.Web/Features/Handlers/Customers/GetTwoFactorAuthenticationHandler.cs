using Grand.Core;
using Grand.Services.Authentication;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetTwoFactorAuthenticationHandler : IRequestHandler<GetTwoFactorAuthentication, CustomerInfoModel.TwoFactorAuthenticationModel>
    {
        private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly IWorkContext _workContext;

        public GetTwoFactorAuthenticationHandler(ITwoFactorAuthenticationService twoFactorAuthenticationService, IWorkContext workContext)
        {
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _workContext = workContext;
        }

        public async Task<CustomerInfoModel.TwoFactorAuthenticationModel> Handle(GetTwoFactorAuthentication request, CancellationToken cancellationToken)
        {
            var secretkey = Guid.NewGuid().ToString();
            var setupInfo = _twoFactorAuthenticationService.GenerateCodeSetup(secretkey, _workContext.CurrentCustomer.Email);

            var model = new CustomerInfoModel.TwoFactorAuthenticationModel {
                CustomValues = setupInfo.CustomValues,
                SecretKey = secretkey
            };
            return await Task.FromResult(model);
        }
    }
}
