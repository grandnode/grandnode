using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductReviewModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public string ProductName { get; set; }

        public string Ids {
            get {
                return Id + ":" + ProductId;
            }
        }

        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Store")]
        public string StoreName { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Customer")]
        public string CustomerInfo { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Title")]
        public string Title { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.ReviewText")]
        public string ReviewText { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.ReplyText")]
        public string ReplyText { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Signature")]
        public string Signature { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Rating")]
        public int Rating { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.IsApproved")]
        public bool IsApproved { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.ProductReviews.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}