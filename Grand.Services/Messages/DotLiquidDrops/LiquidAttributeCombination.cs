using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAttributeCombination : Drop
    {
        private ProductAttributeCombination _combination;
        private Product _product;
        private Customer _customer;

        public LiquidAttributeCombination(Customer customer, Product product, ProductAttributeCombination combination)
        {
            this._customer = customer;
            this._product = product;
            this._combination = combination;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Formatted { get; set; }

        public string SKU { get; set; }

        public string StockQuantity
        {
            get { return _combination.StockQuantity.ToString(); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
