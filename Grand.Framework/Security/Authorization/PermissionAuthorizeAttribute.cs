using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public PermissionAuthorizeAttribute(string permission)
        {
            Permission = permission;
        }

        // Get or set the permision property by manipulating
        public string Permission { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //ignore filter (the action available even when navigation is not allowed)
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(Permission))
                return;

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            //check whether current customer has access to a public store
            if (await permissionService.Authorize(Permission))
                return;

            //authorize permission of access to the admin area
            if (!await permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                context.Result = new ChallengeResult();
            else
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { pageUrl = context.HttpContext.Request.Path });

            return;
        }
    }
}
