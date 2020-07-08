using Grand.Domain.Localization;
using Grand.Domain.Logging;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Payments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class CancelRecurringPaymentCommandHandler : IRequestHandler<CancelRecurringPaymentCommand, IList<string>>
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILogger _logger;

        private readonly LocalizationSettings _localizationSettings;

        public CancelRecurringPaymentCommandHandler(
            IPaymentService paymentService,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            ILogger logger,
            LocalizationSettings localizationSettings)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _logger = logger;
            _localizationSettings = localizationSettings;
        }

        public async Task<IList<string>> Handle(CancelRecurringPaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.RecurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var initialOrder = request.RecurringPayment.InitialOrder;
            if (initialOrder == null)
                return new List<string> { "Initial order could not be loaded" };


            var requestresult = new CancelRecurringPaymentRequest();
            CancelRecurringPaymentResult result = null;
            try
            {
                requestresult.Order = initialOrder;
                result = await _paymentService.CancelRecurringPayment(requestresult);
                if (result.Success)
                {
                    //update recurring payment
                    request.RecurringPayment.IsActive = false;
                    await _orderService.UpdateRecurringPayment(request.RecurringPayment);

                    //add a note
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "Recurring payment has been cancelled",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = initialOrder.Id,

                    });

                    //notify a store owner
                    await _workflowMessageService
                        .SendRecurringPaymentCancelledStoreOwnerNotification(request.RecurringPayment,
                        _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CancelRecurringPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = string.Format("Unable to cancel recurring payment. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = initialOrder.Id,

                });

                //log it
                var logError = string.Format("Error cancelling recurring payment. Order #{0}. Error: {1}", initialOrder.OrderNumber, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;


        }
    }
}
