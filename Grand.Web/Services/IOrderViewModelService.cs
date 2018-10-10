using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Web.Models.Order;

namespace Grand.Web.Services
{
    public partial interface IOrderViewModelService
    {
        CustomerOrderListModel PrepareCustomerOrderList();
        OrderDetailsModel PrepareOrderDetails(Order order);
        ShipmentDetailsModel PrepareShipmentDetails(Shipment shipment);

        CustomerRewardPointsModel PrepareCustomerRewardPoints(Customer customer);

    }
}