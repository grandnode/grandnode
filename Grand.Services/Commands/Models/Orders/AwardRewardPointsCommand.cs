using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class AwardRewardPointsCommand : IRequest<bool>
    {
        public Order Order { get; set; }
    }
}
