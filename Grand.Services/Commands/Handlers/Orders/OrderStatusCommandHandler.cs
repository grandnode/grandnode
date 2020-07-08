using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Vendors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class OrderStatusCommandHandler : IRequestHandler<SetOrderStatusCommand, bool>
    {
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly IMediator _mediator;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        public OrderStatusCommandHandler(
            IOrderService orderService,
            IPdfService pdfService,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            IMediator mediator,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings)
        {
            _orderService = orderService;
            _pdfService = pdfService;
            _workflowMessageService = workflowMessageService;
            _vendorService = vendorService;
            _mediator = mediator;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
        }

        public async Task<bool> Handle(SetOrderStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            OrderStatus prevOrderStatus = request.Order.OrderStatus;
            if (prevOrderStatus == request.Os)
                return false;

            //set and save new order status
            request.Order.OrderStatusId = (int)request.Os;
            await _orderService.UpdateOrder(request.Order);

            //order notes, notifications
            await _orderService.InsertOrderNote(new OrderNote {
                Note = string.Format("Order status has been changed to {0}", request.Os.ToString()),
                DisplayToCustomer = false,
                OrderId = request.Order.Id,
                CreatedOnUtc = DateTime.UtcNow
            });

            if (prevOrderStatus != OrderStatus.Complete &&
                request.Os == OrderStatus.Complete
                && request.NotifyCustomer)
            {
                //notification
                var orderCompletedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    await _pdfService.PrintOrderToPdf(request.Order, "") : null;
                var orderCompletedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    "order.pdf" : null;

                var orderCompletedAttachments = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail && _orderSettings.AttachPdfInvoiceToBinary ?
                    new List<string> { await _pdfService.SaveOrderToBinary(request.Order, "") } : new List<string>();

                int orderCompletedCustomerNotificationQueuedEmailId = await _workflowMessageService
                    .SendOrderCompletedCustomerNotification(request.Order, request.Order.CustomerLanguageId, orderCompletedAttachmentFilePath,
                    orderCompletedAttachmentFileName, orderCompletedAttachments);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "\"Order completed\" email (to customer) has been queued.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = request.Order.Id,
                    });
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                request.Os == OrderStatus.Cancelled
                && request.NotifyCustomer)
            {
                //notification customer
                int orderCancelledCustomerNotificationQueuedEmailId = await _workflowMessageService.SendOrderCancelledCustomerNotification(request.Order, request.Order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "\"Order cancelled\" email (to customer) has been queued.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = request.Order.Id,
                    });
                }
                //notification for vendor
                await VendorNotification(request.Order);
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                request.Os == OrderStatus.Cancelled
                && request.NotifyStoreOwner)
            {
                //notification store owner
                var orderCancelledStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendOrderCancelledStoreOwnerNotification(request.Order, request.Order.CustomerLanguageId);
                if (orderCancelledStoreOwnerNotificationQueuedEmailId > 0)
                {
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "\"Order cancelled\" by customer.",
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = request.Order.Id,
                    });
                }
                //notification for vendor
                await VendorNotification(request.Order);
            }

            //reward points
            if (_rewardPointsSettings.PointsForPurchases_Awarded == request.Order.OrderStatus)
            {
                await _mediator.Send(new AwardRewardPointsCommand() { Order = request.Order });
            }
            if (_rewardPointsSettings.ReduceRewardPointsAfterCancelOrder && request.Order.OrderStatus == OrderStatus.Cancelled)
            {
                await _mediator.Send(new ReduceRewardPointsCommand() { Order = request.Order });
            }

            //gift cards activation
            if (_orderSettings.GiftCards_Activated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Activated_OrderStatusId == (int)request.Order.OrderStatus)
            {
                await _mediator.Send(new ActivatedValueForPurchasedGiftCardsCommand() { Order = request.Order, Activate = true });
            }

            //gift cards deactivation
            if (_orderSettings.DeactivateGiftCardsAfterCancelOrder &&
                request.Order.OrderStatus == OrderStatus.Cancelled)
            {
                await _mediator.Send(new ActivatedValueForPurchasedGiftCardsCommand() { Order = request.Order, Activate = false });
            }

            return true;
        }

        private async Task<bool> VendorNotification(Order order)
        {
            //notification for vendor
            foreach (var orderItem in order.OrderItems)
            {
                if (!string.IsNullOrEmpty(orderItem.VendorId))
                {
                    var vendor = await _vendorService.GetVendorById(orderItem.VendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        await _workflowMessageService.SendOrderCancelledVendorNotification(order, vendor, order.CustomerLanguageId);
                    }
                }
            }

            return true;
        }
    }
}
