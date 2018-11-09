using DotLiquid;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Services.Vendors;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVendorReview : Drop
    {
        private readonly VendorReview _vendorReview;

        public LiquidVendorReview(VendorReview vendorReview)
        {
            this._vendorReview = vendorReview;
        }

        public string Name
        {
            get { return EngineContext.Current.Resolve<IVendorService>().GetVendorById(_vendorReview.VendorId).Name; }
        }
    }
}