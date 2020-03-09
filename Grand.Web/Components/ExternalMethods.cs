using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ExternalMethodsViewComponent : BaseViewComponent
    {
        private readonly IExternalAuthenticationViewModelService _externalAuthenticationViewModelService;

        public ExternalMethodsViewComponent(IExternalAuthenticationViewModelService externalAuthenticationViewModelService)
        {
            _externalAuthenticationViewModelService = externalAuthenticationViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model =  _externalAuthenticationViewModelService.PrepereExternalAuthenticationMethodModel();
            return View(model);
        }
    }
}