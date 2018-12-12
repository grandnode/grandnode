using DotLiquid;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Services.Vendors;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVendorReview : Drop
    {
        private VendorReview _vendorReview;

        public LiquidVendorReview(VendorReview vendorReview)
        {
            this._vendorReview = vendorReview;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Name
        {
            get { return EngineContext.Current.Resolve<IVendorService>().GetVendorById(_vendorReview.VendorId).Name; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}