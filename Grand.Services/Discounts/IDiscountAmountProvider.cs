using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Discounts;
using Grand.Core.Plugins;

namespace Grand.Services.Discounts
{
    public partial interface IDiscountAmountProvider : IPlugin
    {
        decimal DiscountAmount(Discount discount, Customer customer, Product product, decimal amount);
    }
}
