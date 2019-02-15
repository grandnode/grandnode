using Grand.Core.Domain.Directory;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Directory;
using System;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CurrencyViewModelService : ICurrencyViewModelService
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;

        #endregion

        #region Constructors

        public CurrencyViewModelService(ICurrencyService currencyService,
            ISettingService settingService)
        {
            _currencyService = currencyService;
            _settingService = settingService;
        }

        #endregion

        public virtual CurrencyModel PrepareCurrencyModel()
        {
            var model = new CurrencyModel();
            //default values
            model.Published = true;
            model.Rate = 1;
            return model;
        }

        public virtual Currency InsertCurrencyModel(CurrencyModel model)
        {
            var currency = model.ToEntity();
            currency.CreatedOnUtc = DateTime.UtcNow;
            currency.UpdatedOnUtc = DateTime.UtcNow;
            _currencyService.InsertCurrency(currency);

            return currency;
        }

        public virtual Currency UpdateCurrencyModel(Currency currency, CurrencyModel model)
        {
            currency = model.ToEntity(currency);
            currency.UpdatedOnUtc = DateTime.UtcNow;
            _currencyService.UpdateCurrency(currency);
            return currency;
        }
    }
}
