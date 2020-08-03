using Grand.Domain.Orders;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Commands.Models.Orders
{
    public class CancelRecurringPaymentCommand : IRequest<IList<string>>
    {
        public RecurringPayment RecurringPayment { get; set; }
    }
}
