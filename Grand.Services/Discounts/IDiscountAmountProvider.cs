using Grand.Core.Domain.Discounts;
using Grand.Core.Plugins;

namespace Grand.Services.Discounts
{
    public partial interface IDiscountAmountProvider : IPlugin
    {
        decimal DiscountAmount(Discount discount, decimal amount);
    }
}
