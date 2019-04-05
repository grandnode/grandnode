using Grand.Core.Domain.Orders;
using Grand.Web.Models.Order;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IReturnRequestViewModelService
    {
        Task<SubmitReturnRequestModel> PrepareReturnRequest(SubmitReturnRequestModel model, Order order);
        Task<ReturnRequestDetailsModel> PrepareReturnRequestDetails(ReturnRequest returnRequest, Order order);
        Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequests();
    }
}