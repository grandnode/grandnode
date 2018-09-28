using Grand.Core.Domain.Orders;
using Grand.Web.Models.Order;

namespace Grand.Web.Services
{
    public partial interface IReturnRequestViewModelService
    {
        SubmitReturnRequestModel PrepareReturnRequest(SubmitReturnRequestModel model, Order order);
        ReturnRequestDetailsModel PrepareReturnRequestDetails(ReturnRequest returnRequest, Order order);
        CustomerReturnRequestsModel PrepareCustomerReturnRequests();
    }
}