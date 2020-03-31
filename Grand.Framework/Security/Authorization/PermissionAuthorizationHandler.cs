using Grand.Services.Customers;
using Grand.Services.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        public PermissionAuthorizationHandler(IPermissionService permissionService, ICustomerService customerService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var email = context.User.FindFirst(c => c.Type == ClaimTypes.Email);
            var customer = await _customerService.GetCustomerByEmail(email?.Value);
            if (customer != null)
            {
                if (await _permissionService.Authorize(requirement.Permission, customer))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    //TODO - it should be redirect to Security Controller => AccessDenied action
                    context.Fail();
                }
            }
        }

    }
}
