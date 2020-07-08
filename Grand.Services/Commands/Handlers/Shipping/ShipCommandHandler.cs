using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Commands.Models.Shipping;
using Grand.Services.Events.Extensions;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Shipping
{
    public class ShipCommandHandler : IRequestHandler<ShipCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductService _productService;
        private readonly IWorkflowMessageService _workflowMessageService;

        public ShipCommandHandler(
            IMediator mediator,
            IOrderService orderService,
            IShipmentService shipmentService,
            IProductService productService,
            IWorkflowMessageService workflowMessageService)
        {
            _mediator = mediator;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _productService = productService;
            _workflowMessageService = workflowMessageService;
        }

        public async Task<bool> Handle(ShipCommand request, CancellationToken cancellationToken)
        {
            if (request.Shipment == null)
                throw new ArgumentNullException("shipment");

            var order = await _orderService.GetOrderById(request.Shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            if (request.Shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is already shipped");

            request.Shipment.ShippedDateUtc = DateTime.UtcNow;
            await _shipmentService.UpdateShipment(request.Shipment);

            //process products with "Multiple warehouse" support enabled
            foreach (var item in request.Shipment.ShipmentItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault();
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                await _productService.BookReservedInventory(product, item.AttributeXML, item.WarehouseId, -item.Quantity);
            }

            //check whether we have more items to ship
            if (await order.HasItemsToAddToShipment(_orderService, _shipmentService, _productService) ||
                await order.HasItemsToShip(_orderService, _shipmentService, _productService))
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            else
                order.ShippingStatusId = (int)ShippingStatus.Shipped;

            await _orderService.UpdateOrder(order);

            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = $"Shipment #{request.Shipment.ShipmentNumber} has been sent",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            if (request.NotifyCustomer)
            {
                //notify customer
                int queuedEmailId = await _workflowMessageService.SendShipmentSentCustomerNotification(request.Shipment, order);
                if (queuedEmailId > 0)
                {
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "\"Shipped\" email (to customer) has been queued.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
            }
            //event
            await _mediator.PublishShipmentSent(request.Shipment);

            //check order status
            await _mediator.Send(new CheckOrderStatusCommand() { Order = order });

            return true;
        }
    }
}
