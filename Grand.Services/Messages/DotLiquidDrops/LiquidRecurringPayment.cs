using DotLiquid;
using Grand.Core.Domain.Orders;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidRecurringPayment : Drop
    {
        private RecurringPayment _recurringPayment;

        public void SetProperties(RecurringPayment recurringPayment)
        {
            this._recurringPayment = recurringPayment;
        }

        public string Id
        {
            get { return _recurringPayment.Id.ToString(); }
        }
    }
}