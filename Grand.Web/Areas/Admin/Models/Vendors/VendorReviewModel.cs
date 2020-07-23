using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorReviewModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Vendor")]
        public string VendorId { get; set; }
        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Vendor")]
        public string VendorName { get; set; }

        public string Ids {
            get {
                return Id.ToString() + ":" + VendorId;
            }
        }
        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Customer")]
        public string CustomerInfo { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Title")]
        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.Fields.Rating")]
        public int Rating { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
