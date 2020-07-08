using Grand.Domain.Catalog;
using Grand.Domain.Tax;
using Grand.Framework.Mvc.Models;
using Grand.Services.Discounts;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductOverviewModel : BaseGrandEntityModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SecondPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ProductAttributeModels = new List<ProductAttributeModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();
        }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }
        public ProductType ProductType { get; set; }
        public bool MarkAsNew { get; set; }
        public string Sku { get; set; }
        public string Flag { get; set; }
        public string Gtin { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool ShowSku { get; set; }
        public bool ShowQty { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EndTimeLocalTime { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }
        
        //picture
        public PictureModel DefaultPictureModel { get; set; }
        public PictureModel SecondPictureModel { get; set; }

        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }

        //product attributes 
        public IList<ProductAttributeModel> ProductAttributeModels { get; set; }
        
        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

		#region Nested Classes
        public partial class ProductPriceModel : BaseGrandModel
        {
            public ProductPriceModel()
            {
                AppliedDiscounts = new List<AppliedDiscount>();
            }

            public string OldPrice { get; set; }
            public decimal OldPriceValue { get; set; }
            public string CatalogPrice { get; set; }
            public string Price {get;set;}
            public decimal PriceValue { get; set; }
            public string StartPrice { get; set; }
            public decimal StartPriceValue { get; set; }
            public string HighestBid { get; set; }
            public decimal HighestBidValue { get; set; }
            public string BasePricePAngV { get; set; }
            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }
            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
            public bool ForceRedirectionAfterAddingToCart { get; set; }
            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }

            public List<AppliedDiscount> AppliedDiscounts { get; set; }
            public TierPrice PreferredTierPrice { get; set; }

        }
        
        public partial class ProductAttributeModel : BaseGrandModel
        {
            public ProductAttributeModel()
            {
                Values = new List<ProductAttributeValueModel>();
            }
            public string Name { get; set; }
            public string SeName { get; set; }
            public string TextPrompt { get; set; }
            public bool IsRequired { get; set; }
            public AttributeControlType AttributeControlType { get; set; }
            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public partial class ProductAttributeValueModel : BaseGrandModel
        {
            public ProductAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
                PictureModel = new PictureModel();
            }
            public string Name { get; set; }
            public string ColorSquaresRgb { get; set; }
            //picture model is used with "image square" attribute type
            public PictureModel ImageSquaresPictureModel { get; set; }            
            public PictureModel PictureModel { get; set; }
        }
        #endregion
    }
}