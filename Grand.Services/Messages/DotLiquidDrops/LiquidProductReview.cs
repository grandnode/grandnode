using DotLiquid;
using Grand.Domain.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProductReview : Drop
    {
        private ProductReview _productReview;
        private Product _product;

        public LiquidProductReview(Product product, ProductReview productReview)
        {
            _productReview = productReview;
            _product = product;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ProductName
        {
            get
            {
                return _product.Name;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}