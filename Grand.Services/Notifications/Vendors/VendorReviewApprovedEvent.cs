using Grand.Domain.Vendors;
using MediatR;

namespace Grand.Services.Notifications.Vendors
{
    /// <summary>
    /// Vendor review approved event
    /// </summary>
    public class VendorReviewApprovedEvent : INotification
    {
        public VendorReviewApprovedEvent(VendorReview vendorReview)
        {
            VendorReview = vendorReview;
        }

        /// <summary>
        /// Vendor review
        /// </summary>
        public VendorReview VendorReview { get; private set; }
    }
}
