using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductReviewOverviewModel : BaseGrandModel
    {
        public string ProductId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public bool AllowCustomerReviews { get; set; }
    }

    [Validator(typeof(ProductReviewsValidator))]
    public partial class ProductReviewsModel : BaseGrandModel
    {
        public ProductReviewsModel()
        {
            Items = new List<ProductReviewModel>();
            AddProductReview = new AddProductReviewModel();
        }
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public IList<ProductReviewModel> Items { get; set; }
        public AddProductReviewModel AddProductReview { get; set; }
    }

    public partial class ProductReviewModel : BaseGrandEntityModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public string ReplyText { get; set; }
        public string Signature { get; set; }
        public int Rating { get; set; }
        public ProductReviewHelpfulnessModel Helpfulness { get; set; }
        public string WrittenOnStr { get; set; }
    }


    public partial class ProductReviewHelpfulnessModel : BaseGrandModel
    {
        public string ProductReviewId { get; set; }
        public string ProductId { get; set; }

        public int HelpfulYesTotal { get; set; }

        public int HelpfulNoTotal { get; set; }
    }

    public partial class AddProductReviewModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Reviews.Fields.Title")]
        public string Title { get; set; }

        [GrandResourceDisplayName("Reviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [GrandResourceDisplayName("Reviews.Fields.Rating")]
        public int Rating { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool CanCurrentCustomerLeaveReview { get; set; }
        public bool SuccessfullyAdded { get; set; }
        public string Result { get; set; }
    }
}