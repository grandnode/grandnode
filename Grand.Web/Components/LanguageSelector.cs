using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class LanguageSelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public LanguageSelectorViewComponent(ICommonViewModelService commonViewModelService)
        {
            _commonViewModelService = commonViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonViewModelService.PrepareLanguageSelector();
            if (model.AvailableLanguages.Count == 1)
                Content("");

            return View(model);
        }
    }
}