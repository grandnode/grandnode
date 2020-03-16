using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class CurrencySelectorViewComponent : BaseViewComponent
    {
        private readonly ICurrencyService _currencyService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public CurrencySelectorViewComponent(ICurrencyService currencyService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _currencyService = currencyService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareCurrencySelector();
            if (model.AvailableCurrencies.Count == 1)
                Content("");

            return View(model);
        }
        private async Task<CurrencySelectorModel> PrepareCurrencySelector()
        {
            var availableCurrencies = (await _currencyService.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id))
                .Select(x =>
                {
                    //currency char
                    var currencySymbol = "";
                    if (!string.IsNullOrEmpty(x.DisplayLocale))
                        currencySymbol = new RegionInfo(x.DisplayLocale).CurrencySymbol;
                    else
                        currencySymbol = x.CurrencyCode;
                    //model
                    var currencyModel = new CurrencyModel {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                        CurrencyCode = x.CurrencyCode,
                        CurrencySymbol = currencySymbol
                    };
                    return currencyModel;
                }).ToList();

            var model = new CurrencySelectorModel {
                CurrentCurrencyId = _workContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };

            return model;
        }
    }
}