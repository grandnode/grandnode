using Grand.Core;
using Grand.Core.Data;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents filter attribute that saves last visited page by customer
    /// </summary>
    public class SaveLastVisitedPageAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SaveLastVisitedPageAttribute() : base(typeof(SaveLastVisitedPageFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that saves last visited page by customer
        /// </summary>
        private class SaveLastVisitedPageFilter : IAsyncActionFilter
        {
            #region Fields

            private readonly CustomerSettings _customerSettings;
            private readonly IGenericAttributeService _genericAttributeService;
            private readonly IWebHelper _webHelper;
            private readonly IWorkContext _workContext;
            private readonly ICustomerActivityService _customerActivityService;
            #endregion

            #region Ctor

            public SaveLastVisitedPageFilter(CustomerSettings customerSettings,
                IGenericAttributeService genericAttributeService,
                IWebHelper webHelper,
                IWorkContext workContext,
                ICustomerActivityService customerActivityService)
            {
                _customerSettings = customerSettings;
                _genericAttributeService = genericAttributeService;
                _webHelper = webHelper;
                _workContext = workContext;
                _customerActivityService = customerActivityService;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await next();

                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //only in GET requests
                if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
                    return;

                //ajax request should not save
                bool isAjaxCall = context.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest";
                if (isAjaxCall)
                    return;

                //whether is need to store last visited page URL
                if (!_customerSettings.StoreLastVisitedPage)
                    return;

                //get current page
                var pageUrl = _webHelper.GetThisPageUrl(true);
                if (string.IsNullOrEmpty(pageUrl))
                    return;

                //get previous last page
                var previousPageUrl = _workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastVisitedPage);

                //save new one if don't match
                if (!pageUrl.Equals(previousPageUrl, StringComparison.OrdinalIgnoreCase))
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastVisitedPage, pageUrl);

                if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers[HeaderNames.Referer]))
                    if (!context.HttpContext.Request.Headers[HeaderNames.Referer].ToString().Contains(context.HttpContext.Request.Host.ToString()))
                    {
                        var previousUrlReferrer = _workContext.CurrentCustomer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastUrlReferrer);
                        var actualUrlReferrer = context.HttpContext.Request.Headers[HeaderNames.Referer];
                        if (previousUrlReferrer != actualUrlReferrer)
                        {
                            await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastUrlReferrer, actualUrlReferrer);
                        }
                    }

                if (_customerSettings.SaveVisitedPage)
                {
                    if (!_workContext.CurrentCustomer.IsSearchEngineAccount())
                    {
                        await _customerActivityService.InsertActivityAsync("PublicStore.Url", pageUrl, pageUrl, _workContext.CurrentCustomer.Id, _webHelper.GetCurrentIpAddress());
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}