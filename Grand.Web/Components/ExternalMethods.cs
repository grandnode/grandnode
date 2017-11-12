using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class ExternalMethodsViewComponent : ViewComponent
    {
        private readonly IExternalAuthenticationWebService _externalAuthenticationWebService;

        public ExternalMethodsViewComponent(IExternalAuthenticationWebService externalAuthenticationWebService)
        {
            this._externalAuthenticationWebService = externalAuthenticationWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _externalAuthenticationWebService.PrepereExternalAuthenticationMethodModel();
            return View(model);

        }
    }
}