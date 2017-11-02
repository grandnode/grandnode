using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class LogoViewComponent : ViewComponent
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