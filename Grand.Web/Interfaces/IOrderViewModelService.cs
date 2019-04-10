using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Web.Models.Order;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IOrderViewModelService
    {
        Task<CustomerOrderListModel> PrepareCustomerOrderList();
        Task<OrderDetailsModel> PrepareOrderDetails(Order order);
        Task<ShipmentDetailsModel> PrepareShipmentDetails(Shipment shipment);
        Task<CustomerRewardPointsModel> PrepareCustomerRewardPoints(Customer customer);
        Task InsertOrderNote(AddOrderNoteModel model);
    }
}