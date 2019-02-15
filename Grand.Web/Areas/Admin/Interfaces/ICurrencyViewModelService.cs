using Grand.Core.Domain.Directory;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICurrencyViewModelService
    {
        CurrencyModel PrepareCurrencyModel();
        Currency InsertCurrencyModel(CurrencyModel model);
        Currency UpdateCurrencyModel(Currency currency, CurrencyModel model);
    }
}
