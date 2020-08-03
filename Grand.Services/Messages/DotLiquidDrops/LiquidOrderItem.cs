using DotLiquid;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Net;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidOrderItem : Drop
    {
        private OrderItem _orderItem;
        private Order _order;
        private Product _product;
        private Language _language;
        private Store _store;
        private Vendor _vendor;

        public LiquidOrderItem(OrderItem orderItem, Product product, Order order, Language language, Currency currency, Store store, Vendor vendor)
        {
            _orderItem = orderItem;
            _store = store;
            _language = language;
            _order = order;
            _product = product;
            _vendor = vendor;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string UnitPrice { get; set; }

        public string TotalPrice { get; set; }


        public string ProductSku { get; set; }


        public bool ShowSkuOnProductDetailsPage { get; set; }

        public bool IsDownloadAllowed { get; set; }

        public bool IsLicenseDownloadAllowed { get; set; }

        public string DownloadUrl
        {
            get
            {
                var storeId = _order?.StoreId;
                string downloadUrl = string.Format("{0}download/getdownload/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _orderItem.OrderItemGuid);
                return downloadUrl;
            }
        }

        public string LicenseUrl
        {
            get
            {
                string licenseUrl = string.Format("{0}download/getlicense/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _orderItem.OrderItemGuid);
                return licenseUrl;
            }
        }

        public Guid OrderItemGuid0
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
                    name = WebUtility.HtmlEncode(_product.GetLocalized(x => x.Name, _language.Id));

                return name;
            }
        }

        public string ProductSeName 
        {
            get {
                string name = "";

                if (_product != null)
                    name = _product.GetLocalized(x => x.SeName, _language.Id);
                return name;
            }
        }
        public string ProductShortDescription
        {
            get
            {
                string desc = "";

                if (_product != null)
                    desc = WebUtility.HtmlEncode(_product.GetLocalized(x => x.ShortDescription, _language.Id));

                return desc;
            }
        }

        public string ProductFullDescription
        {
            get
            {
                string desc = "";

                if (_product != null)
                    desc = WebUtility.HtmlDecode(_product.GetLocalized(x => x.FullDescription, _language.Id));

                return desc;
            }
        }

        public string ProductOldPrice { get; set; }



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

        public string VendorName {
            get {
                return _vendor?.Name;
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