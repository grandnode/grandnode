using Grand.Core.Domain.Orders;
using Grand.Web.Models.Order;

namespace Grand.Web.Services
{
    public partial interface IReturnRequestWebService
    {
        SubmitReturnRequestModel PrepareReturnRequest(SubmitReturnRequestModel model, Order order);
        CustomerReturnRequestsModel PrepareCustomerReturnRequests();
    }
}