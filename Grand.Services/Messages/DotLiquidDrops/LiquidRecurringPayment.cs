using DotLiquid;
using Grand.Core.Domain.Orders;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidRecurringPayment : Drop
    {
        private readonly RecurringPayment _recurringPayment;

        public LiquidRecurringPayment(RecurringPayment recurringPayment)
        {
            this._recurringPayment = recurringPayment;
        }

        public string Id
        {
            get { return _recurringPayment.Id.ToString(); }
        }
    }
}