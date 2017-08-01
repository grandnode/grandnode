using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class LanguageSelectorViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public LanguageSelectorViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareLanguageSelector();
            if (model.AvailableLanguages.Count == 1)
                Content("");

            return View(model);
        }
    }
}