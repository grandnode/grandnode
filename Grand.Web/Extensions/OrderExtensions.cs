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
            if (customer.IsOwner() && (customer.Id == order.CustomerId || customer.Id == order.OwnerId))
                return true;

            //subaccount
            if (!customer.IsOwner() && customer.Id == order.CustomerId)
                return true;

            return false;
        }
        public static bool Access(this ReturnRequest returnRequest, Customer customer)
        {
            if (returnRequest == null)
                return false;

            //owner
            if (customer.IsOwner() && (customer.Id == returnRequest.CustomerId || customer.Id == returnRequest.OwnerId))
                return true;

            //subaccount
            if (!customer.IsOwner() && customer.Id == returnRequest.CustomerId)
                return true;

            return false;
        }
    }
}
