using Grand.Domain.Orders;
using Grand.Domain.Vendors;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Orders
{
    public class GetVendorsInOrderQuery : IRequest<IList<Vendor>>
    {
        public Order Order { get; set; }
    }
}
