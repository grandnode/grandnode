using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Messages;
using Grand.Services.Notifications.Orders;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Vendors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ProcessOrderPaidCommandHandler : IRequestHandler<ProcessOrderPaidCommand, bool>
    {
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;
        private readonly OrderSettings _orderSettings;
        private readonly LocalizationSettings _localizationSettings;

        public ProcessOrderPaidCommandHandler(
            IWorkflowMessageService workflowMessageService, 
            IMediator mediator, 
            IPdfService pdfService, 
            OrderSettings orderSettings, 
            LocalizationSettings localizationSettings)
        {
            _workflowMessageService = workflowMessageService;
            _mediator = mediator;
            _pdfService = pdfService;
            _orderSettings = orderSettings;
            _localizationSettings = localizationSettings;
        }

        public async Task<bool> Handle(ProcessOrderPaidCommand request, CancellationToken cancellationToken)
        {
            await ProcessOrderPaid(request.Order);
            return true;
        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual async Task ProcessOrderPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //raise event
            await _mediator.Publish(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
                    await _pdfService.PrintOrderToPdf(order, "")
                    : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
                    "order.pdf" : null;

                var orderPaidAttachments = _orderSettings.AttachPdfInvoiceToOrderPaidEmail && _orderSettings.AttachPdfInvoiceToBinary ?
                    new List<string> { await _pdfService.SaveOrderToBinary(order, "") } : new List<string>();

                await _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName, orderPaidAttachments);

                await _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                if (order.OrderItems.Any(x => !string.IsNullOrEmpty(x.VendorId)))
                {
                    var vendors = await _mediator.Send(new GetVendorsInOrderQuery() { Order = order });
                    foreach (var vendor in vendors)
                    {
                        await _workflowMessageService.SendOrderPaidVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                    }
                }
                //TODO add "order paid email sent" order note
            }
        }
    }
}
