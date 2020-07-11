using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Plugin.ExchangeRate.McExchange
{
    internal class NbpExchange : IRateProvider
    {
        public async Task<IList<Domain.Directory.ExchangeRate>> GetCurrencyLiveRates()
        {
            var currentDate = DateTime.Today.AddDays(-1);
            var request = (HttpWebRequest)WebRequest.Create($"http://api.nbp.pl/api/exchangerates/tables/A/{currentDate.AddDays(-7):yyyy-MM-dd}/{currentDate:yyyy-MM-dd}");
            request.Headers.Add("Accept", MediaTypeNames.Application.Xml);
            using (var response = await request.GetResponseAsync())
            {
                var document = new XmlDocument();
                document.Load(response.GetResponseStream());

                var node = document.SelectNodes("//EffectiveDate")
                    .Cast<XmlElement>()
                    .OrderByDescending(x => x.InnerText)
                    .First();

                var updateDate = DateTime.ParseExact(node.InnerText, "yyyy-MM-dd", null);
                var ratesNode = node.ParentNode.SelectSingleNode("Rates");

                var provider = new NumberFormatInfo();
                provider.CurrencyDecimalSeparator = ".";
                provider.NumberGroupSeparator = "";

                var exchangeRates = new List<Domain.Directory.ExchangeRate>();
                foreach (XmlNode node2 in ratesNode.ChildNodes)
                {
                    var rate = decimal.Parse(node2.SelectSingleNode("Mid").InnerText, provider);
                    exchangeRates.Add(new Domain.Directory.ExchangeRate {
                        CurrencyCode = node2.SelectSingleNode("Code").InnerText,
                        Rate = Math.Round(1m / rate, 4, MidpointRounding.AwayFromZero),
                        UpdatedOn = updateDate
                    });
                }
                return exchangeRates;
            }
        }
    }
}
