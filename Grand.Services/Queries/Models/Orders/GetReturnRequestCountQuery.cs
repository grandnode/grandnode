using MediatR;
namespace Grand.Services.Queries.Models.Orders
{
    public class GetReturnRequestCountQuery : IRequest<int>
    {
        public int RequestStatusId { get; set; }
        public string StoreId { get; set; }
    }
}
