using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Discounts;
using Grand.Core.Plugins;
using System.Threading.Tasks;

namespace Grand.Services.Discounts
{
    public partial interface IDiscountAmountProvider : IPlugin
    {
        Task<decimal> DiscountAmount(Discount discount, Customer customer, Product product, decimal amount);
    }
}
