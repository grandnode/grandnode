using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.Feed.GoogleShopping.Models
{
    public class FeedGoogleShoppingModel
    {
        public FeedGoogleShoppingModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCurrencies = new List<SelectListItem>();
            AvailableGoogleCategories = new List<SelectListItem>();
            GeneratedFiles = new List<GeneratedFileModel>();
        }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.ProductPictureSize")]
        public int ProductPictureSize { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Currency")]
        public string CurrencyId { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.DefaultGoogleCategory")]
        public string DefaultGoogleCategory { get; set; }
        public IList<SelectListItem> AvailableGoogleCategories { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.PassShippingInfoWeight")]
        public bool PassShippingInfoWeight { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.PassShippingInfoDimensions")]
        public bool PassShippingInfoDimensions { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.PricesConsiderPromotions")]
        public bool PricesConsiderPromotions { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.StaticFilePath")]
        public IList<GeneratedFileModel> GeneratedFiles { get; set; }

        public class GeneratedFileModel : BaseModel
        {
            public string StoreName { get; set; }
            public string FileUrl { get; set; }
        }

        public class GoogleProductModel : BaseModel
        {
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.ProductName")]
            public string ProductName { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.GoogleCategory")]
            public string GoogleCategory { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Gender")]
            public string Gender { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.AgeGroup")]
            public string AgeGroup { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Color")]
            public string Color { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Size")]
            public string GoogleSize { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.GoogleShopping.Products.CustomGoods")]
            public bool CustomGoods { get; set; }
        }
    }
}