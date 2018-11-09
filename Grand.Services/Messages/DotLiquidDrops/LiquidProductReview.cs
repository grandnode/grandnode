using DotLiquid;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProductReview : Drop
    {
        private readonly ProductReview _productReview;

        public LiquidProductReview(ProductReview productReview)
        {
            this._productReview = productReview;
        }

        public string ProductName
        {
            get
            {
                var product = EngineContext.Current.Resolve<IProductService>().GetProductById(_productReview.ProductId);
                return product.Name;
            }
        }
    }
}