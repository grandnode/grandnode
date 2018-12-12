using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProductReview : Drop
    {
        private ProductReview _productReview;

        public LiquidProductReview(ProductReview productReview)
        {
            this._productReview = productReview;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ProductName
        {
            get
            {
                var product = EngineContext.Current.Resolve<IProductService>().GetProductById(_productReview.ProductId);
                return product.Name;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}