using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ExternalMethodsViewComponent : BaseViewComponent
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