using System.Web.Mvc;
using WebMarkupMin.AspNet4.Mvc;

namespace Nop.Web.Extensions
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            if (bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EnableWebMarkupMin.AspNet4.Mvc.CompressContentAttribute"]))
            {
                filters.Add(new CompressContentAttribute());
            }
            if (bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EnableWebMarkupMin.AspNet4.Mvc.MinifyHtmlAttribute"]))
            {
                filters.Add(new MinifyHtmlAttribute());
            }
            if (bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EnableWebMarkupMin.AspNet4.Mvc.MinifyXmlAttribute"]))
            {
                filters.Add(new MinifyXmlAttribute());
            }
        }
    }
}