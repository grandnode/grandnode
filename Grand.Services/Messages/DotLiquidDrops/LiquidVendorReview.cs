using DotLiquid;
using Grand.Domain.Vendors;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVendorReview : Drop
    {
        private VendorReview _vendorReview;
        private Vendor _vendor;
        public LiquidVendorReview(Vendor vendor, VendorReview vendorReview)
        {
            _vendorReview = vendorReview;
            _vendor = vendor;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string VendorName
        {
            get { return _vendor.Name; }
        }

        public string Title
        {
            get { return _vendorReview.Title; }
        }
        public string ReviewText
        {
            get { return _vendorReview.ReviewText; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}