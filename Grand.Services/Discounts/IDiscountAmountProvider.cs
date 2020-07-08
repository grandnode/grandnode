using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using Grand.Core.Plugins;
using System.Threading.Tasks;

namespace Grand.Services.Discounts
{
    public partial interface IDiscountAmountProvider : IPlugin
    {
        Task<decimal> DiscountAmount(Discount discount, Customer customer, Product product, decimal amount);
    }
}
