﻿using System;
using System.Collections.Generic;
using Grand.Web.Framework.Mvc;
using Grand.Web.Models.Media;
using Grand.Core.Domain.Catalog;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductOverviewModel : BaseNopEntityModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }
        public ProductType ProductType { get; set; }
        public bool MarkAsNew { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }
        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

		#region Nested Classes

        public partial class ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price {get;set;}
            public decimal PriceValue { get; set; }
            public string BasePricePAngV { get; set; }
            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

		#endregion
    }
}