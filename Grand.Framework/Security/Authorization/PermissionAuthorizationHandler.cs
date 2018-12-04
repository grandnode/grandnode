using Grand.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!_permissionService.Authorize(requirement.Permission))
            {
                var redirectContext = context.Resource as AuthorizationFilterContext;
                redirectContext.Result = new RedirectToActionResult("AccessDenied", "Security", null);
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
