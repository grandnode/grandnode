using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductReviewListModel : BaseGrandModel
    {
        public ProductReviewListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.List.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.List.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.List.SearchText")]
        
        public string SearchText { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.List.SearchStore")]
        public string SearchStoreId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.List.SearchProduct")]
        public string SearchProductId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}