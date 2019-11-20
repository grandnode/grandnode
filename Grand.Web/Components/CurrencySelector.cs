using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class CurrencySelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public CurrencySelectorViewComponent(ICommonViewModelService commonViewModelService)
        {
            _commonViewModelService = commonViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonViewModelService.PrepareCurrencySelector();
            if (model.AvailableCurrencies.Count == 1)
                Content("");

            return View(model);
        }
    }
}