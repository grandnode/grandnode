using Grand.Domain.Customers;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class CalculateRewardPointsCommand : IRequest<int>
    {
        public Customer Customer { get; set; }
        public decimal Amount { get; set; }
    }
}
