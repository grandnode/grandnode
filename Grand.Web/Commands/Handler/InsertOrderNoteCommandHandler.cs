﻿using Grand.Core.Domain.Orders;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Web.Commands.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler
{
    public class InsertOrderNoteCommandHandler : IRequestHandler<InsertOrderNoteCommandModel, OrderNote>
    {
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderService _orderService;

        public InsertOrderNoteCommandHandler(IWorkflowMessageService workflowMessageService, IOrderService orderService)
        {
            _workflowMessageService = workflowMessageService;
            _orderService = orderService;
        }

        public async Task<OrderNote> Handle(InsertOrderNoteCommandModel request, CancellationToken cancellationToken)
        {
            var orderNote = new OrderNote {
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = true,
                Note = request.OrderNote.Note,
                OrderId = request.OrderNote.OrderId,
                CreatedByCustomer = true
            };
            await _orderService.InsertOrderNote(orderNote);

            //email
            await _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                orderNote, request.Language.Id);

            return orderNote;
        }
    }
}
