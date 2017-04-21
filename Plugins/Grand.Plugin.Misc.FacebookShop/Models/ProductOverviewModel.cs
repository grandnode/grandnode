using System;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Misc.FacebookShop.Models
{
    //just a copy of \Grand.Web\Models\Catalog\ProductOverviewModel.cs file (removed some redundant code)
    public partial class ProductOverviewModel : BaseNopEntityModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }
        //picture
        public PictureModel DefaultPictureModel { get; set; }

        #region Nested Classes

        public partial class ProductPriceModel : BaseGrandModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }
        }

        #endregion
    }
}