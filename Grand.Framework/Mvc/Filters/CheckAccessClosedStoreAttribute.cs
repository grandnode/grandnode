using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain;
using Grand.Services.Security;
using Grand.Services.Topics;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute that confirms access to a closed store
    /// </summary>
    public class CheckAccessClosedStoreAttribute : TypeFilterAttribute
    {
        private readonly bool _ignoreFilter;
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public CheckAccessClosedStoreAttribute(bool ignore = false) : base(typeof(CheckAccessClosedStoreFilter))
        {
            this._ignoreFilter = ignore;
            this.Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter => _ignoreFilter;

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to closed store
        /// </summary>
        private class CheckAccessClosedStoreFilter : IActionFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;
            private readonly IStoreContext _storeContext;
            private readonly ITopicService _topicService;
            private readonly StoreInformationSettings _storeInformationSettings;

            #endregion

            #region Ctor

            public CheckAccessClosedStoreFilter(bool ignoreFilter,
                IPermissionService permissionService,
                IStoreContext storeContext,
                ITopicService topicService,
                StoreInformationSettings storeInformationSettings)
            {
                this._ignoreFilter = ignoreFilter;
                this._permissionService = permissionService;
                this._storeContext = storeContext;
                this._topicService = topicService;
                this._storeInformationSettings = storeInformationSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(f => f.Scope == FilterScope.Action)
                    .Select(f => f.Filter).OfType<CheckAccessClosedStoreAttribute>().FirstOrDefault();

                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //store isn't closed
                if (!_storeInformationSettings.StoreClosed)
                    return;

                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                    return;

                //topics accessible when a store is closed
                if (controllerName.Equals("Topic", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("TopicDetails", StringComparison.OrdinalIgnoreCase))
                {
                    //get identifiers of topics are accessible when a store is closed
                    var allowedTopicIds = _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                        .Where(topic => topic.AccessibleWhenStoreClosed).Select(topic => topic.Id);

                    //check whether requested topic is allowed
                    var requestedTopicId = context.RouteData.Values["topicId"] as string;
                    if (!string.IsNullOrEmpty(requestedTopicId) && allowedTopicIds.Contains(requestedTopicId))
                        return;
                }

                //check whether current customer has access to a closed store
                if (_permissionService.Authorize(StandardPermissionProvider.AccessClosedStore))
                    return;

                //store is closed and no access, so redirect to 'StoreClosed' page
                var storeClosedUrl = new UrlHelper(context).RouteUrl("StoreClosed");
                context.Result = new RedirectResult(storeClosedUrl);
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}