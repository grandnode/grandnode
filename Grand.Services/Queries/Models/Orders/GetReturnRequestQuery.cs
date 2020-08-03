using Grand.Domain.Orders;
using MediatR;
using MongoDB.Driver.Linq;
using System;

namespace Grand.Services.Queries.Models.Orders
{
    public class GetReturnRequestQuery : IRequest<IMongoQueryable<ReturnRequest>>
    {
        public string StoreId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string VendorId { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public string OrderItemId { get; set; } = "";
        public ReturnRequestStatus? Rs { get; set; } = null;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = int.MaxValue;
        public DateTime? CreatedFromUtc { get; set; } = null;
        public DateTime? CreatedToUtc { get; set; } = null;
    }
}
