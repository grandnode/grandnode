using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class LogoViewComponent : BaseViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public LogoViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareLogo();
            return View(model);
        }
    }
}