using Grand.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _contextAccessor;
        public PermissionAuthorizationHandler(IPermissionService permissionService, IHttpContextAccessor contextAccessor)
        {
            this._permissionService = permissionService;
            this._contextAccessor = contextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!_permissionService.Authorize(requirement.Permission))
            {
                var redirectContext = context.Resource as AuthorizationFilterContext;
                var httpContext = _contextAccessor.HttpContext;
                redirectContext.Result = new RedirectToActionResult("AccessDenied", "Security", new { pageUrl = httpContext.Request.Path });
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
