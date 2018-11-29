using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAttributeCombination : Drop
    {
        private ProductAttributeCombination _combination;
        private string _languageId;
        private Product _product;

        private readonly IWorkContext _workContext;
        private readonly IProductAttributeParser _productAttributeParser;

        public LiquidAttributeCombination(
            IWorkContext workContext,
            IProductAttributeParser productAttributeParser)
        {
            this._workContext = workContext;
            this._productAttributeParser = productAttributeParser;
        }

        public void SetProperties(ProductAttributeCombination combination, string languageId)
        {
            this._combination = combination;
            this._languageId = languageId;
            this._product = EngineContext.Current.Resolve<IProductService>().GetProductById(combination.ProductId);
        }

        public string Formatted
        {
            get
            {
                var productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();
                string attributes = productAttributeFormatter.FormatAttributes(_product,
                    _combination.AttributesXml,
                    _workContext.CurrentCustomer,
                    renderPrices: false);

                return attributes;
            }
        }

        public string SKU
        {
            get { return _product.FormatSku(_combination.AttributesXml, _productAttributeParser); }
        }

        public string StockQuantity
        {
            get { return _combination.StockQuantity.ToString(); }
        }
    }
}
