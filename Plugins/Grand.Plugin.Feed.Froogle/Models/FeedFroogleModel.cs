using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Feed.Froogle.Models
{
    public class FeedFroogleModel
    {
        public FeedFroogleModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCurrencies = new List<SelectListItem>();
            AvailableGoogleCategories = new List<SelectListItem>();
            GeneratedFiles = new List<GeneratedFileModel>();
        }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.ProductPictureSize")]
        public int ProductPictureSize { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.Currency")]
        public string CurrencyId { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.DefaultGoogleCategory")]
        public string DefaultGoogleCategory { get; set; }
        public IList<SelectListItem> AvailableGoogleCategories { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.PassShippingInfoWeight")]
        public bool PassShippingInfoWeight { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.PassShippingInfoDimensions")]
        public bool PassShippingInfoDimensions { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.PricesConsiderPromotions")]
        public bool PricesConsiderPromotions { get; set; }

        [GrandResourceDisplayName("Plugins.Feed.Froogle.StaticFilePath")]
        public IList<GeneratedFileModel> GeneratedFiles { get; set; }
        
        public class GeneratedFileModel : BaseNopModel
        {
            public string StoreName { get; set; }
            public string FileUrl { get; set; }
        }

        public class GoogleProductModel : BaseNopModel
        {
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.ProductName")]
            public string ProductName { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.GoogleCategory")]
            public string GoogleCategory { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.Gender")]
            public string Gender { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.AgeGroup")]
            public string AgeGroup { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.Color")]
            public string Color { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.Size")]
            public string GoogleSize { get; set; }

            [GrandResourceDisplayName("Plugins.Feed.Froogle.Products.CustomGoods")]
            public bool CustomGoods { get; set; }
        }
    }
}