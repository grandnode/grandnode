using Grand.Domain.Shipping;
using MediatR;

namespace Grand.Services.Queries.Models.Orders
{
    public class GetPickupPointById : IRequest<PickupPoint>
    {
        public string Id { get; set; }
    }
}
