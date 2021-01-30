using Grand.Core.Data;
using Grand.Domain.Stores;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute that confirms access to public store
    /// </summary>
    public class CheckAccessPublicStoreAttribute : TypeFilterAttribute
    {
        private readonly bool _ignoreFilter;

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public CheckAccessPublicStoreAttribute(bool ignore = false) : base(typeof(CheckAccessPublicStoreFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter => _ignoreFilter;

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to public store
        /// </summary>
        private class CheckAccessPublicStoreFilter : IAsyncAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;
            private readonly StoreInformationSettings _storeInformationSettings;
            #endregion

            #region Ctor

            public CheckAccessPublicStoreFilter(bool ignoreFilter, IPermissionService permissionService, StoreInformationSettings storeInformationSettings)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
                _storeInformationSettings = storeInformationSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
            {
                //ignore filter (the action available even when navigation is not allowed)
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                //check whether this filter has been overridden for the Action
                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(f => f.Scope == FilterScope.Action)
                    .Select(f => f.Filter).OfType<CheckAccessPublicStoreAttribute>().FirstOrDefault();

               
                //ignore filter (the action is available even if navigation is not allowed)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //check whether current customer has access to a public store
                
                if (await _permissionService.Authorize(StandardPermissionProvider.PublicStoreAllowNavigation))
                    return;

                if (_storeInformationSettings.StoreClosed)
                {
                    filterContext.Result = new RedirectToRouteResult("StoreClosed", new RouteValueDictionary());                    
                }
                else
                    //customer hasn't access to a public store
                    filterContext.Result = new ChallengeResult();
            }

            #endregion
        }

        #endregion
    }
}