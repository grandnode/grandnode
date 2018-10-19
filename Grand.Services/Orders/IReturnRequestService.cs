using Grand.Core;
using Grand.Core.Domain.Orders;
using System.Collections.Generic;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Return request service interface
    /// </summary>
    public partial interface IReturnRequestService
    {
        /// <summary>
        /// Deletes a return request
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        void DeleteReturnRequest(ReturnRequest returnRequest);

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        ReturnRequest GetReturnRequestById(string returnRequestId);

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="id">Return request number</param>t
        /// <returns>Return request</returns>
        ReturnRequest GetReturnRequestById(int id);

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; 0 to load all entries</param>
        /// <param name="orderItemId">Order item identifier; 0 to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Return requests</returns>
        IPagedList<ReturnRequest> SearchReturnRequests(string storeId = "", string customerId = "",
            string orderItemId = "", ReturnRequestStatus? rs = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Delete a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        void DeleteReturnRequestAction(ReturnRequestAction returnRequestAction);

        /// <summary>
        /// Gets all return request actions
        /// </summary>
        /// <returns>Return request actions</returns>
        IList<ReturnRequestAction> GetAllReturnRequestActions();

        /// <summary>
        /// Gets a return request action
        /// </summary>
        /// <param name="returnRequestActionId">Return request action identifier</param>
        /// <returns>Return request action</returns>
        ReturnRequestAction GetReturnRequestActionById(string returnRequestActionId);

        /// <summary>
        /// Inserts a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        void InsertReturnRequestAction(ReturnRequestAction returnRequestAction);

        /// <summary>
        /// Inserts a return request
        /// </summary>
        /// <param name="returnRequest">Return request </param>
        void InsertReturnRequest(ReturnRequest returnRequest);
        /// <summary>
        /// Updates the  return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        void UpdateReturnRequestAction(ReturnRequestAction returnRequestAction);




        /// <summary>
        /// Delete a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        void DeleteReturnRequestReason(ReturnRequestReason returnRequestReason);

        /// <summary>
        /// Gets all return request reaspns
        /// </summary>
        /// <returns>Return request reaspns</returns>
        IList<ReturnRequestReason> GetAllReturnRequestReasons();

        /// <summary>
        /// Gets a return request reaspn
        /// </summary>
        /// <param name="returnRequestReasonId">Return request reason identifier</param>
        /// <returns>Return request reaspn</returns>
        ReturnRequestReason GetReturnRequestReasonById(string returnRequestReasonId);

        /// <summary>
        /// Inserts a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        void InsertReturnRequestReason(ReturnRequestReason returnRequestReason);

        /// <summary>
        /// Updates the  return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        void UpdateReturnRequestReason(ReturnRequestReason returnRequestReason);
    }
}
