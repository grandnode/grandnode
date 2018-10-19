using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class FooterViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public FooterViewComponent(ICommonViewModelService commonViewModelService)
        {
            this._commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareFooter();
            return View(model);

        }
    }
}