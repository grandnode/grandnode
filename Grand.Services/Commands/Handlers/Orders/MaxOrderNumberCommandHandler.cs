using Grand.Domain.Data;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class MaxOrderNumberCommandHandler : IRequestHandler<MaxOrderNumberCommand, int?>
    {
        private readonly IRepository<Order> _orderRepository;

        public MaxOrderNumberCommandHandler(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<int?> Handle(MaxOrderNumberCommand request, CancellationToken cancellationToken)
        {
            if (_orderRepository.Table.Count() == 0)
                return null;

            var max = _orderRepository.Table.Max(x => x.OrderNumber);
            if (request.OrderNumber.HasValue)
            {
                if (_orderRepository.Table.Count() > 0)
                    if (request.OrderNumber.Value > max)
                    {
                        await _orderRepository.InsertAsync(new Order() { OrderNumber = request.OrderNumber.Value, Deleted = true, CreatedOnUtc = DateTime.UtcNow });
                        max = request.OrderNumber.Value;
                    }
            }
            return max;
        }
    }
}
