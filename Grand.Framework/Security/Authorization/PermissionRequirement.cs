using Microsoft.AspNetCore.Authorization;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }
        public PermissionRequirement(string permission) { Permission = permission; }
    }
}
