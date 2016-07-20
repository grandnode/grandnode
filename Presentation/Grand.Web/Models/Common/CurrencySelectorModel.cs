using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Common
{
    public partial class CurrencySelectorModel : BaseNopModel
    {
        public CurrencySelectorModel()
        {
            AvailableCurrencies = new List<CurrencyModel>();
        }

        public IList<CurrencyModel> AvailableCurrencies { get; set; }

        public string CurrentCurrencyId { get; set; }
    }
}