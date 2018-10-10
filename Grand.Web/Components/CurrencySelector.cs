using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class CurrencySelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public CurrencySelectorViewComponent(ICommonViewModelService commonViewModelService)
        {
            this._commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareCurrencySelector();
            if (model.AvailableCurrencies.Count == 1)
                Content("");

            return View(model);
        }
    }
}