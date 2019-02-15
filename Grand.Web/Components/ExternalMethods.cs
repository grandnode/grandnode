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
            this._externalAuthenticationViewModelService = externalAuthenticationViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await Task.Run(() => _externalAuthenticationViewModelService.PrepereExternalAuthenticationMethodModel());
            return View(model);
        }
    }
}