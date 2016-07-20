using System.Web.Mvc;
using Grand.Web.Framework.UI.Paging;

namespace Grand.Plugin.Misc.FacebookShop.Extensions
{
    public static class PagerHtmlExtension
    {
        //jsut a copy of \Presentation\Grand.Web\Extensions\PagerHtmlExtension.cs
        public static Pager Pager(this HtmlHelper helper, IPageableModel pagination)
        {
            return new Pager(pagination, helper.ViewContext);
        }
    }
}
