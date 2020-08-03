using Grand.Domain;
using Grand.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task DeleteReturnRequest(ReturnRequest returnRequest);

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        Task<ReturnRequest> GetReturnRequestById(string returnRequestId);

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="id">Return request number</param>t
        /// <returns>Return request</returns>
        Task<ReturnRequest> GetReturnRequestById(int id);

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; 0 to load all entries</param>
        /// <param name="orderItemId">Order item identifier; 0 to load all entries</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="ownerId">Owner identifier</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>        
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Return requests</returns>
        Task<IPagedList<ReturnRequest>> SearchReturnRequests(string storeId = "", string customerId = "",
            string orderItemId = "", string vendorId = "", string ownerId = "", ReturnRequestStatus? rs = null,
            int pageIndex = 0, int pageSize = int.MaxValue, DateTime? createdFromUtc = null, DateTime? createdToUtc = null);

        /// <summary>
        /// Delete a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        Task DeleteReturnRequestAction(ReturnRequestAction returnRequestAction);

        /// <summary>
        /// Gets all return request actions
        /// </summary>
        /// <returns>Return request actions</returns>
        Task<IList<ReturnRequestAction>> GetAllReturnRequestActions();

        /// <summary>
        /// Gets a return request action
        /// </summary>
        /// <param name="returnRequestActionId">Return request action identifier</param>
        /// <returns>Return request action</returns>
        Task<ReturnRequestAction> GetReturnRequestActionById(string returnRequestActionId);

        /// <summary>
        /// Inserts a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        Task InsertReturnRequestAction(ReturnRequestAction returnRequestAction);

        /// <summary>
        /// Inserts a return request
        /// </summary>
        /// <param name="returnRequest">Return request </param>
        Task InsertReturnRequest(ReturnRequest returnRequest);
        /// <summary>
        /// Updates the  return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        Task UpdateReturnRequestAction(ReturnRequestAction returnRequestAction);

        /// <summary>
        /// Delete a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        Task DeleteReturnRequestReason(ReturnRequestReason returnRequestReason);

        /// <summary>
        /// Gets all return request reaspns
        /// </summary>
        /// <returns>Return request reaspns</returns>
        Task<IList<ReturnRequestReason>> GetAllReturnRequestReasons();

        /// <summary>
        /// Gets a return request reaspn
        /// </summary>
        /// <param name="returnRequestReasonId">Return request reason identifier</param>
        /// <returns>Return request reaspn</returns>
        Task<ReturnRequestReason> GetReturnRequestReasonById(string returnRequestReasonId);

        /// <summary>
        /// Inserts a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        Task InsertReturnRequestReason(ReturnRequestReason returnRequestReason);

        /// <summary>
        /// Updates the return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        Task UpdateReturnRequestReason(ReturnRequestReason returnRequestReason);

        /// <summary>
        /// Update the return request
        /// </summary>
        /// <param name="returnRequest"></param>
        Task UpdateReturnRequest(ReturnRequest returnRequest);

        #region Return request notes

        /// <summary>
        /// Deletes a return request note
        /// </summary>
        /// <param name="returnRequestNote">The return request note</param>
        Task DeleteReturnRequestNote(ReturnRequestNote returnRequestNote);

        /// <summary>
        /// Insert a return request note
        /// </summary>
        /// <param name="returnRequestNote">The return request note</param>
        Task InsertReturnRequestNote(ReturnRequestNote returnRequestNote);

        /// <summary>
        /// Get notes for return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>ReturnRequestNote</returns>
        Task<IList<ReturnRequestNote>> GetReturnRequestNotes(string returnRequestId);

        /// <summary>
        /// Get return request note by id
        /// </summary>
        /// <param name="returnRequestNoteId">Return request note identifier</param>
        /// <returns>ReturnRequestNote</returns>
        Task<ReturnRequestNote> GetReturnRequestNote(string returnRequestNoteId);

        #endregion
    }
}
