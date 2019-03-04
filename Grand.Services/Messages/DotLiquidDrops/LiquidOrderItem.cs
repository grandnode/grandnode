using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidOrderItem : Drop
    {
        private OrderItem _orderItem;
        private string _languageId;
        private Order _order;
        private Product _product;

        private readonly IProductService _productService;
        private readonly IDownloadService _downloadService;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly CatalogSettings _catalogSettings;

        public LiquidOrderItem(OrderItem orderItem, Order order, string lanugageId)
        {
            this._productService = EngineContext.Current.Resolve<IProductService>();
            this._downloadService = EngineContext.Current.Resolve<IDownloadService>();
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._orderService = EngineContext.Current.Resolve<IOrderService>();
            this._productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            this._priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();
            this._currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            this._languageService = EngineContext.Current.Resolve<ILanguageService>();
            this._catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

            this._orderItem = orderItem;
            this._languageId = lanugageId;
            this._order = _orderService.GetOrderByOrderItemId(_orderItem.Id);
            this._product = _productService.GetProductById(orderItem.ProductId);

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string UnitPrice
        {
            get
            {
                string unitPriceStr;
                if (_order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(_orderItem.UnitPriceInclTax, _order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, _order.CustomerCurrencyCode,
                        _languageService.GetLanguageById(_languageId), true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(_orderItem.UnitPriceExclTax, _order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, _order.CustomerCurrencyCode,
                        _languageService.GetLanguageById(_languageId), false);
                }

                return unitPriceStr;
            }
        }

        public string TotalPrice
        {
            get
            {
                string priceStr;
                if (_order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(_orderItem.PriceInclTax, _order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, _order.CustomerCurrencyCode,
                        _languageService.GetLanguageById(_languageId), true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(_orderItem.PriceExclTax, _order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, _order.CustomerCurrencyCode,
                        _languageService.GetLanguageById(_languageId), false);
                }

                return priceStr;
            }
        }

        public string ProductSku
        {
            get
            {
                string sku = "";

                if (_product != null)
                    sku = _product.FormatSku(_orderItem.AttributesXml, _productAttributeParser);

                return WebUtility.HtmlEncode(sku);
            }
        }

        public bool ShowSkuOnProductDetailsPage
        {
            get
            {
                return _catalogSettings.ShowSkuOnProductDetailsPage;
            }
        }

        public bool IsDownloadAllowed
        {
            get
            {
                return _downloadService.IsDownloadAllowed(_orderItem);
            }
        }

        public bool IsLicenseDownloadAllowed
        {
            get
            {
                return _downloadService.IsLicenseDownloadAllowed(_orderItem);
            }
        }

        public string DownloadUrl
        {
            get
            {
                var storeId = _order?.StoreId;
                string downloadUrl = string.Format("{0}download/getdownload/{1}", _storeService.GetStoreUrl(storeId), _orderItem.OrderItemGuid);
                return downloadUrl;
            }
        }

        public string LicenseUrl
        {
            get
            {
                var storeId = _order?.StoreId;
                string licenseUrl = string.Format("{0}download/getlicense/{1}", _storeService.GetStoreUrl(storeId), _orderItem.OrderItemGuid);
                return licenseUrl;
            }
        }

        public Guid OrderItemGuid
        {
            get
            {
                return _orderItem.OrderItemGuid;
            }
        }

        public string ProductName
        {
            get
            {
                string name = "";

                if (_product != null)
                    name = WebUtility.HtmlEncode(_product.GetLocalized(x => x.Name, _languageId));

                return name;
            }
        }

        public string ProductShortDescription
        {
            get
            {
                string desc = "";

                if (_product != null)
                    desc = WebUtility.HtmlEncode(_product.GetLocalized(x => x.ShortDescription, _languageId));

                return desc;
            }
        }

        public string ProductFullDescription
        {
            get
            {
                string desc = "";

                if (_product != null)
                    desc = WebUtility.HtmlDecode(_product.GetLocalized(x => x.FullDescription, _languageId));

                return desc;
            }
        }

        public string ProductPrice
        {
            get
            {
                string price = "";

                if (_product != null)
                    price = _priceFormatter.FormatPrice(_product.Price, true, _order.CustomerCurrencyCode, _languageService.GetLanguageById(_languageId), true);

                return price;
            }
        }

        public string ProductOldPrice
        {
            get
            {
                string price = "";

                if (_product != null)
                    price = _priceFormatter.FormatPrice(_product.OldPrice, true, _order.CustomerCurrencyCode, _languageService.GetLanguageById(_languageId), true);

                return price;
            }
        }

        public string ProductId
        {
            get
            {
                return _orderItem.ProductId;
            }
        }

        public string VendorId
        {
            get
            {
                return _orderItem.VendorId;
            }
        }

        public string WarehouseId
        {
            get
            {
                return _orderItem.WarehouseId;
            }
        }

        public int Quantity
        {
            get
            {
                return _orderItem.Quantity;
            }
        }

        public decimal UnitPriceWithoutDiscInclTax
        {
            get
            {
                return _orderItem.UnitPriceWithoutDiscInclTax;
            }
        }

        public decimal UnitPriceWithoutDiscExclTax
        {
            get
            {
                return _orderItem.UnitPriceWithoutDiscExclTax;
            }
        }

        public decimal UnitPriceInclTax
        {
            get
            {
                return _orderItem.UnitPriceInclTax;
            }
        }

        public decimal UnitPriceExclTax
        {
            get
            {
                return _orderItem.UnitPriceExclTax;
            }
        }

        public decimal PriceInclTax
        {
            get
            {
                return _orderItem.PriceInclTax;
            }
        }

        public decimal PriceExclTax
        {
            get
            {
                return _orderItem.PriceExclTax;
            }
        }

        public decimal DiscountAmountInclTax
        {
            get
            {
                return _orderItem.DiscountAmountInclTax;
            }
        }

        public decimal DiscountAmountExclTax
        {
            get
            {
                return _orderItem.DiscountAmountExclTax;
            }
        }

        public decimal OriginalProductCost
        {
            get
            {
                return _orderItem.OriginalProductCost;
            }
        }

        public string AttributeDescription
        {
            get
            {
                return _orderItem.AttributeDescription;
            }
        }

        public string AttributesXml
        {
            get
            {
                return _orderItem.AttributesXml;
            }
        }

        public int DownloadCount
        {
            get
            {
                return _orderItem.DownloadCount;
            }
        }

        public bool IsDownloadActivated
        {
            get
            {
                return _orderItem.IsDownloadActivated;
            }
        }

        public string LicenseDownloadId
        {
            get
            {
                return _orderItem.LicenseDownloadId;
            }
        }

        public decimal? ItemWeight
        {
            get
            {
                return _orderItem.ItemWeight;
            }
        }

        public DateTime? RentalStartDateUtc
        {
            get
            {
                return _orderItem.RentalStartDateUtc;
            }
        }

        public DateTime? RentalEndDateUtc
        {
            get
            {
                return _orderItem.RentalEndDateUtc;
            }
        }

        public DateTime CreatedOnUtc
        {
            get
            {
                return _orderItem.CreatedOnUtc;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}