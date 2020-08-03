using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Vendors
{
    public partial class VendorReviewOverviewModel : BaseGrandModel
    {
        public string VendorId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public bool AllowCustomerReviews { get; set; }
    }

    public partial class VendorReviewsModel : BaseGrandModel
    {
        public VendorReviewsModel()
        {
            Items = new List<VendorReviewModel>();
            AddVendorReview = new AddVendorReviewModel();
        }
        public string VendorId { get; set; }

        public string VendorName { get; set; }

        public string VendorSeName { get; set; }

        public IList<VendorReviewModel> Items { get; set; }
        public AddVendorReviewModel AddVendorReview { get; set; }
    }

    public partial class VendorReviewModel : BaseGrandEntityModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool AllowViewingProfiles { get; set; }

        public string Title { get; set; }

        public string ReviewText { get; set; }

        public int Rating { get; set; }

        public VendorReviewHelpfulnessModel Helpfulness { get; set; }

        public string WrittenOnStr { get; set; }
    }

    public partial class VendorReviewHelpfulnessModel : BaseGrandModel
    {
        public string VendorReviewId { get; set; }
        public string VendorId { get; set; }

        public int HelpfulYesTotal { get; set; }

        public int HelpfulNoTotal { get; set; }
    }

    public partial class AddVendorReviewModel : BaseGrandModel
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