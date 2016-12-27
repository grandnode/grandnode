using System;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure;
using Grand.Services.Common;
using Grand.Services.Logging;

namespace Grand.Web.Framework
{
    public class StoreLastVisitedPageAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            //ajax request should not save
            if (filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            var customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            if (!customerSettings.StoreLastVisitedPage)
                return;

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            var pageUrl = webHelper.GetThisPageUrl(true);
            if (!String.IsNullOrEmpty(pageUrl))
            {
                var currentCustomer = EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer;
                var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                var previousPageUrl = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
                if (!pageUrl.Equals(previousPageUrl))
                {
                    genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.LastVisitedPage, pageUrl);
                }

                if (filterContext.HttpContext.Request.UrlReferrer != null)
                    if (filterContext.HttpContext.Request.Url.Host != filterContext.HttpContext.Request.UrlReferrer.Host)
                    {
                        var previousUrlReferrer = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastUrlReferrer);
                        var actualUrlReferrer = filterContext.HttpContext.Request.UrlReferrer.ToString();
                        if (previousUrlReferrer != actualUrlReferrer)
                        {
                            genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.LastUrlReferrer, actualUrlReferrer);
                        }
                    }

                if(customerSettings.SaveVisitedPage)
                {
                    if (!currentCustomer.IsSearchEngineAccount())
                    {
                        var customerActivity = EngineContext.Current.Resolve<ICustomerActivityService>();
                        customerActivity.InsertActivityAsync("PublicStore.Url", pageUrl, pageUrl, currentCustomer.Id, webHelper.GetCurrentIpAddress());
                    }
                }
            }
        }
    }
}
