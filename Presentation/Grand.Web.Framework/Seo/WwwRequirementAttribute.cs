using System;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Seo;
using Grand.Core.Infrastructure;

namespace Grand.Web.Framework.Seo
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class WwwRequirementAttribute : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            // only redirect for GET requests, 
            // otherwise the browser might not propagate the verb and request body correctly.
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            //ignore this rule for localhost
            if (filterContext.HttpContext.Request.IsLocal)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;
            var seoSettings = EngineContext.Current.Resolve<SeoSettings>();

            switch (seoSettings.WwwRequirement)
            {
                case WwwRequirement.WithWww:
                {
                    var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                    string url = webHelper.GetThisPageUrl(true);
                    var currentConnectionSecured = webHelper.IsCurrentConnectionSecured();
                    if (currentConnectionSecured)
                    {
                        bool startsWith3W = url.StartsWith("https://www.", StringComparison.OrdinalIgnoreCase);
                        if (!startsWith3W)
                        {
                            url = url.Replace("https://", "https://www.");

                            //301 (permanent) redirection
                            filterContext.Result = new RedirectResult(url, true);
                        }
                    }
                    else
                    {
                        bool startsWith3W = url.StartsWith("http://www.", StringComparison.OrdinalIgnoreCase);
                        if (!startsWith3W)
                        {
                            url = url.Replace("http://", "http://www.");

                            //301 (permanent) redirection
                            filterContext.Result = new RedirectResult(url, true);
                        }
                    }
                }
                    break;
                case WwwRequirement.WithoutWww:
                {
                    var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                    string url = webHelper.GetThisPageUrl(true);
                    var currentConnectionSecured = webHelper.IsCurrentConnectionSecured();
                    if (currentConnectionSecured)
                    {
                        bool startsWith3W = url.StartsWith("https://www.", StringComparison.OrdinalIgnoreCase);
                        if (startsWith3W)
                        {
                            url = url.Replace("https://www.", "https://");

                            //301 (permanent) redirection
                            filterContext.Result = new RedirectResult(url, true);
                        }
                    }
                    else
                    {
                        bool startsWith3W = url.StartsWith("http://www.", StringComparison.OrdinalIgnoreCase);
                        if (startsWith3W)
                        {
                            url = url.Replace("http://www.", "http://");

                            //301 (permanent) redirection
                            filterContext.Result = new RedirectResult(url, true);
                        }
                    }
                }
                    break;
                case WwwRequirement.NoMatter:
                {
                    //do nothing
                }
                break;
                default:
                    throw new GrandException("Not supported WwwRequirement parameter");
            }
        }
    }
}
