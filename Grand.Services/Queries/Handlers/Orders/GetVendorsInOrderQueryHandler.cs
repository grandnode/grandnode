using Grand.Domain.Orders;
using Grand.Domain.Vendors;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Vendors;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Queries.Handlers.Orders
{
    public class GetVendorsInOrderQueryHandler : IRequestHandler<GetVendorsInOrderQuery, IList<Vendor>>
    {
        private readonly IVendorService _vendorService;

        public GetVendorsInOrderQueryHandler(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        public async Task<IList<Vendor>> Handle(GetVendorsInOrderQuery request, CancellationToken cancellationToken)
        {
            return await GetVendorsInOrder(request.Order);
        }

        protected virtual async Task<IList<Vendor>> GetVendorsInOrder(Order order)
        {
            var vendors = new List<Vendor>();
            foreach (var orderItem in order.OrderItems)
            {
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == orderItem.VendorId);
                if (vendor == null && !string.IsNullOrEmpty(orderItem.VendorId))
                {
                    //not found. load by Id
                    vendor = await _vendorService.GetVendorById(orderItem.VendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        vendors.Add(vendor);
                    }
                }
            }

            return vendors;
        }
    }
}
