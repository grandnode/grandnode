using Grand.Domain.Customers;
using Grand.Domain.Orders;

namespace Grand.Web.Extensions
{
    public static class OrderExtensions
    {
        public static bool Access(this Order order, Customer customer)
        {
            if (order == null || order.Deleted)
                return false;

            //owner
            if (string.IsNullOrEmpty(customer.OwnerId) && (customer.Id == order.CustomerId || customer.Id == order.OwnerId))
                return true;

            //subaccount
            if (!string.IsNullOrEmpty(customer.OwnerId) && customer.Id == order.CustomerId)
                return true;

            return false;
        }
    }
}
