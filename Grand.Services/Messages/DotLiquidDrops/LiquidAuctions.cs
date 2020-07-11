using DotLiquid;
using Grand.Domain.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAuctions : Drop
    {
        private Product _product;
        private Bid _bid;

        public LiquidAuctions(Product product, Bid bid = null)
        {
            _product = product;
            _bid = bid;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ProductName
        {
            get { return _product.Name; }
        }

        public string Price { get; set; }

        public string EndTime { get; set; }

        public string ProductSeName
        {
            get { return _product.SeName; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}