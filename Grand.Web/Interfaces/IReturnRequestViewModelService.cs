using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Order;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IReturnRequestViewModelService
    {
        Task<SubmitReturnRequestModel> PrepareReturnRequest(SubmitReturnRequestModel model, Order order);
        Task<ReturnRequestDetailsModel> PrepareReturnRequestDetails(ReturnRequest returnRequest, Order order);
        Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequests();
        Task<(SubmitReturnRequestModel model, ReturnRequest rr)> ReturnRequestSubmit(SubmitReturnRequestModel model, Order order, Address address, DateTime pickupDate, IFormCollection form);
    }
}