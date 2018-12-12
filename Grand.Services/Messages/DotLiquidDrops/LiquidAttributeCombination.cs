using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAttributeCombination : Drop
    {
        private ProductAttributeCombination _combination;
        private string _languageId;
        private Product _product;

        private readonly IWorkContext _workContext;
        private readonly IProductAttributeParser _productAttributeParser;

        public LiquidAttributeCombination(ProductAttributeCombination combination, string languageId)
        {
            this._workContext = EngineContext.Current.Resolve<IWorkContext>();
            this._productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();

            this._combination = combination;
            this._languageId = languageId;
            this._product = EngineContext.Current.Resolve<IProductService>().GetProductById(combination.ProductId);
            
            AdditionalTokens = new Dictionary<string, string>();
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
