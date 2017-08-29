using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using System.Linq;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute confirming that user with "Vendor" customer role has appropriate vendor account associated (and active)
    /// </summary>
    public class ValidateVendorAttribute : TypeFilterAttribute
    {
        private readonly bool _ignoreFilter;

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public ValidateVendorAttribute(bool ignore = false) : base(typeof(ValidateVendorFilter))
        {
            this._ignoreFilter = ignore;
            this.Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter => _ignoreFilter;

        #region Nested filter

        /// <summary>
        /// Represents a filter confirming that user with "Vendor" customer role has appropriate vendor account associated (and active)
        /// </summary>
        private class ValidateVendorFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public ValidateVendorFilter(bool ignoreFilter, IWorkContext workContext)
            {
                this._ignoreFilter = ignoreFilter;
                this._workContext = workContext;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                //ignore filter actions
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                //check whether this filter has been overridden for the Action
                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(f => f.Scope == FilterScope.Action)
                    .Select(f => f.Filter).OfType<ValidateVendorAttribute>().FirstOrDefault();

                //ignore filter (the action is available even if the current customer isn't a vendor)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //whether current customer is vendor
                if (!_workContext.CurrentCustomer.IsVendor())
                    return;

                //ensure that this user has active vendor record associated
                if (_workContext.CurrentVendor == null)
                    filterContext.Result = new ChallengeResult();
            }

            #endregion
        }

        #endregion
    }
}