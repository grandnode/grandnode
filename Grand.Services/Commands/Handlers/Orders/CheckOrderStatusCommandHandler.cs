using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Grand.Services.Commands.Handlers.Orders
{
    public class CheckOrderStatusCommandHandler : IRequestHandler<CheckOrderStatusCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;

        public CheckOrderStatusCommandHandler(
            IMediator mediator,
            IOrderService orderService,
            OrderSettings orderSettings)
        {
            _mediator = mediator;
            _orderService = orderService;
            _orderSettings = orderSettings;
        }

        public async Task<bool> Handle(CheckOrderStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            if (request.Order.PaymentStatus == PaymentStatus.Paid && !request.Order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                request.Order.PaidDateUtc = DateTime.UtcNow;
                await _orderService.UpdateOrder(request.Order);
            }

            if (request.Order.OrderStatus == OrderStatus.Pending)
            {
                if (request.Order.PaymentStatus == PaymentStatus.Authorized ||
                    request.Order.PaymentStatus == PaymentStatus.Paid)
                {
                    await _mediator.Send(new SetOrderStatusCommand {
                        Order = request.Order,
                        Os = OrderStatus.Processing,
                        NotifyCustomer = false,
                        NotifyStoreOwner = false
                    });
                }

                if (request.Order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                    request.Order.ShippingStatus == ShippingStatus.Shipped ||
                    request.Order.ShippingStatus == ShippingStatus.Delivered)
                {
                    await _mediator.Send(new SetOrderStatusCommand {
                        Order = request.Order,
                        Os = OrderStatus.Processing,
                        NotifyCustomer = false,
                        NotifyStoreOwner = false
                    });
                }
            }

            if (request.Order.OrderStatus != OrderStatus.Cancelled &&
                request.Order.OrderStatus != OrderStatus.Complete)
            {
                if (request.Order.PaymentStatus == PaymentStatus.Paid)
                {
                    var completed = false;
                    if (request.Order.ShippingStatus == ShippingStatus.ShippingNotRequired)
                    {
                        completed = true;
                    }
                    else
                    {
                        if (_orderSettings.CompleteOrderWhenDelivered)
                        {
                            completed = request.Order.ShippingStatus == ShippingStatus.Delivered;
                        }
                        else
                        {
                            completed = request.Order.ShippingStatus == ShippingStatus.Shipped ||
                                request.Order.ShippingStatus == ShippingStatus.Delivered;
                        }
                    }
                    if (completed)
                    {
                        await _mediator.Send(new SetOrderStatusCommand {
                            Order = request.Order,
                            Os = OrderStatus.Complete,
                            NotifyCustomer = true,
                            NotifyStoreOwner = false
                        });
                    }
                }
            }
            return true;
        }
    }
}
