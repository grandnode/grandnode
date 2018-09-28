using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ExternalMethodsViewComponent : BaseViewComponent
    {
        private readonly IExternalAuthenticationViewModelService _externalAuthenticationViewModelService;

        public ExternalMethodsViewComponent(IExternalAuthenticationViewModelService externalAuthenticationViewModelService)
        {
            this._externalAuthenticationViewModelService = externalAuthenticationViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _externalAuthenticationViewModelService.PrepereExternalAuthenticationMethodModel();
            return View(model);

        }
    }
}