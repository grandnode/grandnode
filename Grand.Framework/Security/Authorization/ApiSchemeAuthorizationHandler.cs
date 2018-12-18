using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Grand.Framework.Security.Authorization
{
    public class ApiSchemeAuthorizationHandler: AuthorizationHandler<ApiAuthorizationSchemeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAuthorizationSchemeRequirement requirement)
        {
            var mvcContext = context.Resource as AuthorizationFilterContext;
            if (requirement.IsValid(mvcContext?.HttpContext.Request.Headers))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;

        }
    }
}
