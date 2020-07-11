using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Plugin.ExchangeRate.McExchange
{
    internal class EcbExchange : IRateProvider
    {
        public async Task<IList<Domain.Directory.ExchangeRate>> GetCurrencyLiveRates()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml");
            using (var response = await request.GetResponseAsync())
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

                var exchangeRates = new List<Grand.Domain.Directory.ExchangeRate>();
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    exchangeRates.Add(new Domain.Directory.ExchangeRate()
                        {
                            CurrencyCode = node2.Attributes["currency"].Value,
                            Rate = decimal.Parse(node2.Attributes["rate"].Value, provider),
                            UpdatedOn = updateDate
                        }
                    );
                }
                return exchangeRates;
            }
        }
    }
}
