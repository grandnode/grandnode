
using Grand.Core.Configuration;

namespace Grand.Core.Domain.Directory
{
    public class CurrencySettings : ISettings
    {
        public bool DisplayCurrencyLabel { get; set; }
        public string PrimaryStoreCurrencyId { get; set; }
        public string PrimaryExchangeRateCurrencyId { get; set; }
        public string ActiveExchangeRateProviderSystemName { get; set; }
        public bool AutoUpdateEnabled { get; set; }
    }
}