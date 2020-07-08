using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Discounts;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductService _productService;
        private readonly IProductReservationService _productReservationService;
        private readonly IAuctionService _auctionService;
        private readonly IDiscountService _discountService;
        private readonly OrderSettings _orderSettings;

        public DeleteOrderCommandHandler(
            IMediator mediator,
            IOrderService orderService,
            IShipmentService shipmentService,
            IProductService productService,
            IProductReservationService productReservationService,
            IAuctionService auctionService,
            IDiscountService discountService,
            OrderSettings orderSettings)
        {
            _mediator = mediator;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _productService = productService;
            _productReservationService = productReservationService;
            _auctionService = auctionService;
            _discountService = discountService;
            _orderSettings = orderSettings;
        }

        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            //check whether the order wasn't cancelled before
            //if it already was cancelled, then there's no need to make the following adjustments
            //(such as reward points, inventory, recurring payments)
            //they already was done when cancelling the order
            if (request.Order.OrderStatus != OrderStatus.Cancelled)
            {
                //return (add) back redeemded reward points
                await _mediator.Send(new ReturnBackRedeemedRewardPointsCommand() { Order = request.Order });

                //reduce (cancel) back reward points (previously awarded for this order)
                await _mediator.Send(new ReduceRewardPointsCommand() { Order = request.Order });

                //cancel recurring payments
                var recurringPayments = await _orderService.SearchRecurringPayments(initialOrderId: request.Order.Id);
                foreach (var rp in recurringPayments)
                {
                    var errors = await _mediator.Send(new CancelRecurringPaymentCommand() { RecurringPayment = rp });
                    //use "errors" variable?
                }

                //Adjust inventory for already shipped shipments
                //only products with "use multiple warehouses"
                foreach (var shipment in await _shipmentService.GetShipmentsByOrder(request.Order.Id))
                {
                    foreach (var shipmentItem in shipment.ShipmentItems)
                    {
                        var product = await _productService.GetProductById(shipmentItem.ProductId);
                        shipmentItem.ShipmentId = shipment.Id;
                        if (product != null)
                            await _productService.ReverseBookedInventory(product, shipment, shipmentItem);
                    }
                }
                //Adjust inventory
                foreach (var orderItem in request.Order.OrderItems)
                {
                    var product = await _productService.GetProductById(orderItem.ProductId);
                    if (product != null)
                        await _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);
                }

                //cancel reservations
                await _productReservationService.CancelReservationsByOrderId(request.Order.Id);

                //cancel bid
                await _auctionService.CancelBidByOrder(request.Order.Id);
            }

            //deactivate gift cards
            if (_orderSettings.DeactivateGiftCardsAfterDeletingOrder)
                await _mediator.Send(new ActivatedValueForPurchasedGiftCardsCommand() { Order = request.Order, Activate = false });

            request.Order.Deleted = true;
            //now delete an order
            await _orderService.UpdateOrder(request.Order);

            //cancel discounts 
            await _discountService.CancelDiscount(request.Order.Id);

            return true;
        }
    }
}
