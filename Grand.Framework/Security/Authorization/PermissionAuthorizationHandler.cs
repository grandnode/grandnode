using Grand.Services.Customers;
using Grand.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _contextAccessor;
        public PermissionAuthorizationHandler(IPermissionService permissionService, ICustomerService customerService, IHttpContextAccessor contextAccessor)
        {
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._contextAccessor = contextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var email = context.User.FindFirst(c => c.Type == ClaimTypes.Email);
            var customer = _customerService.GetCustomerByEmail(email.Value);
            if (customer != null)
            {
                if (!_permissionService.Authorize(requirement.Permission, customer))
                {
                    var redirectContext = context.Resource as AuthorizationFilterContext;
                    var httpContext = _contextAccessor.HttpContext;
                    redirectContext.Result = new RedirectToActionResult("AccessDenied", "Security", new { pageUrl = httpContext.Request.Path });
                }
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
