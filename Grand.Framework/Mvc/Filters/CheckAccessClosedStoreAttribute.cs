using Grand.Core;
using Grand.Core.Data;
using Grand.Domain.Stores;
using Grand.Services.Security;
using Grand.Services.Topics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter => _ignoreFilter;

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to closed store
        /// </summary>
        private class CheckAccessClosedStoreFilter : IAsyncActionFilter
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
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
                _storeContext = storeContext;
                _topicService = topicService;
                _storeInformationSettings = storeInformationSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {

                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                {
                    await next();
                    return;
                }
                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(f => f.Scope == FilterScope.Action)
                    .Select(f => f.Filter).OfType<CheckAccessClosedStoreAttribute>().FirstOrDefault();

                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    await next();
                    return;
                }

                if (!DataSettingsHelper.DatabaseIsInstalled())
                {
                    await next();
                    return;
                }

                //store isn't closed
                if (!_storeInformationSettings.StoreClosed)
                {
                    await next();
                    return;
                }

                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                {
                    await next();
                    return;
                }

                //topics accessible when a store is closed
                if (controllerName.Equals("Topic", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("TopicDetails", StringComparison.OrdinalIgnoreCase))
                {
                    //get identifiers of topics are accessible when a store is closed
                    var allowedTopicIds = (await _topicService.GetAllTopics(_storeContext.CurrentStore.Id))
                        .Where(topic => topic.AccessibleWhenStoreClosed).Select(topic => topic.Id);

                    //check whether requested topic is allowed
                    var requestedTopicId = context.RouteData.Values["topicId"] as string;
                    if (!string.IsNullOrEmpty(requestedTopicId) && allowedTopicIds.Contains(requestedTopicId))
                    {
                        await next();
                        return;
                    }
                }

                //check whether current customer has access to a closed store
                if (await _permissionService.Authorize(StandardPermissionProvider.AccessClosedStore))
                {
                    await next();
                    return;
                }

                //store is closed and no access, so redirect to 'StoreClosed' page
                context.Result = new RedirectToRouteResult("StoreClosed", new RouteValueDictionary());
            }

            #endregion
        }

        #endregion
    }
}