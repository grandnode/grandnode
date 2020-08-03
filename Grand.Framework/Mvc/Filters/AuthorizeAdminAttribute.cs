using Grand.Core.Data;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute that confirms access to the admin panel
    /// </summary>
    public class AuthorizeAdminAttribute : TypeFilterAttribute
    {
        private readonly bool _ignoreFilter;

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public AuthorizeAdminAttribute(bool ignore = false) : base(typeof(AuthorizeAdminFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter => _ignoreFilter;

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to the admin panel
        /// </summary>
        private class AuthorizeAdminFilter : IAsyncAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;

            #endregion

            #region Ctor

            public AuthorizeAdminFilter(bool ignoreFilter, IPermissionService permissionService)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
            {
                
                if (filterContext == null)
                    throw new ArgumentNullException("filterContext");

                //check whether this filter has been overridden for the action
                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(f => f.Scope == FilterScope.Action)
                    .Select(f => f.Filter).OfType<AuthorizeAdminAttribute>().FirstOrDefault();

                //ignore filter (the action is available even if a customer hasn't access to the admin area)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //there is AdminAuthorizeFilter, so check access
                if (filterContext.Filters.Any(filter => filter is AuthorizeAdminFilter))
                {
                    //authorize permission of access to the admin area
                    if (!await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                        filterContext.Result = new ChallengeResult();
                }
            }

            #endregion
        }

        #endregion
    }
}