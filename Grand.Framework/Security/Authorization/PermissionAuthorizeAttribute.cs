using Microsoft.AspNetCore.Authorization;
using System;

namespace Grand.Framework.Security.Authorization
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "Permission";

        public PermissionAuthorizeAttribute(string permission) => Permission = permission;

        // Get or set the permision property by manipulating the underlying Policy property
        public string Permission
        {
            get
            {
                if (Policy.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    return Policy.Replace(POLICY_PREFIX, "");
                }
                return string.Empty;
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }
    }
}
