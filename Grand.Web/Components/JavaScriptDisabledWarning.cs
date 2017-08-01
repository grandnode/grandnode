using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Common;

namespace Grand.Web.ViewComponents
{
    public class JavaScriptDisabledWarningViewComponent : ViewComponent
    {
        private readonly CommonSettings _commonSettings;
        public JavaScriptDisabledWarningViewComponent(CommonSettings commonSettings)
        {
            this._commonSettings = commonSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_commonSettings.DisplayJavaScriptDisabledWarning)
                return Content("");

            return View();
        }
    }
}