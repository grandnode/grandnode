using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class TaxTypeSelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public TaxTypeSelectorViewComponent(ICommonViewModelService commonViewModelService)
        {
            _commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareTaxTypeSelector();
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}