using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.News;
using Grand.Core;
using Grand.Services.Localization;

namespace Grand.Web.ViewComponents
{
    public class NewsRssHeaderLinkViewComponent : ViewComponent
    {
        private readonly NewsSettings _newsSettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        public NewsRssHeaderLinkViewComponent(NewsSettings newsSettings, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            this._newsSettings = newsSettings;
            this._workContext = workContext;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowHeaderRssUrl)
                return Content("");

            return View();

        }
    }
}