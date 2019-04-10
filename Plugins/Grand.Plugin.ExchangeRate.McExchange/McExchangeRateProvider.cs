using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Directory;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Plugin.ExchangeRate.EcbExchange
{
    public class EcbExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        private readonly ILocalizationService _localizationService;
        private readonly IServiceProvider _serviceProvider;

        public EcbExchangeRateProvider(ILocalizationService localizationService, IServiceProvider serviceProvider)
        {
            this._localizationService = localizationService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public async Task<IList<Core.Domain.Directory.ExchangeRate>> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            if (String.IsNullOrEmpty(exchangeRateCurrencyCode) ||
                exchangeRateCurrencyCode.ToLower() != "eur")
                throw new GrandException(_localizationService.GetResource("Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO"));

            var exchangeRates = new List<Grand.Core.Domain.Directory.ExchangeRate>();

            var request = WebRequest.Create("http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml") as HttpWebRequest;
            using (WebResponse response = request.GetResponse())
            {
                var document = new XmlDocument();
                document.Load(response.GetResponseStream());
                var nsmgr = new XmlNamespaceManager(document.NameTable);
                nsmgr.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
                nsmgr.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");

                var node = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", nsmgr);
                var updateDate = DateTime.ParseExact(node.Attributes["time"].Value, "yyyy-MM-dd", null);

                var provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";
                provider.NumberGroupSeparator = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    exchangeRates.Add(new Core.Domain.Directory.ExchangeRate()
                    {
                        CurrencyCode = node2.Attributes["currency"].Value,
                        Rate = decimal.Parse(node2.Attributes["rate"].Value, provider),
                        UpdatedOn = updateDate
                    }
                    );
                }
            }

            return await Task.FromResult(exchangeRates);
        }

        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO", "You can use ECB (European central bank) exchange rate provider only when the primary exchange rate currency is set to EURO");
            await base.Install();
        }

        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO");
            await base.Uninstall();
        }
    }
}