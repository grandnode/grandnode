using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Orders;
using Grand.Services.Events;
using Grand.Services.Queries.Models.Orders;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Return request service
    /// </summary>
    public partial class ReturnRequestService : IReturnRequestService
    {
        #region Fields
        private static readonly Object _locker = new object();

        private readonly IRepository<ReturnRequest> _returnRequestRepository;
        private readonly IRepository<ReturnRequestAction> _returnRequestActionRepository;
        private readonly IRepository<ReturnRequestReason> _returnRequestReasonRepository;
        private readonly IRepository<ReturnRequestNote> _returnRequestNoteRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="returnRequestRepository">Return request repository</param>
        /// <param name="returnRequestActionRepository">Return request action repository</param>
        /// <param name="returnRequestReasonRepository">Return request reason repository</param>
        /// <param name="returnRequestNoteRepository">Return request note repository</param>
        /// <param name="mediator">Mediator</param>
        public ReturnRequestService(IRepository<ReturnRequest> returnRequestRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IRepository<ReturnRequestNote> returnRequestNoteRepository,
            IMediator mediator)
        {
            _returnRequestRepository = returnRequestRepository;
            _returnRequestActionRepository = returnRequestActionRepository;
            _returnRequestReasonRepository = returnRequestReasonRepository;
            _returnRequestNoteRepository = returnRequestNoteRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a return request
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        public virtual async Task DeleteReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            await _returnRequestRepository.DeleteAsync(returnRequest);

            //event notification
            await _mediator.EntityDeleted(returnRequest);
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        public virtual Task<ReturnRequest> GetReturnRequestById(string returnRequestId)
        {
            return _returnRequestRepository.GetByIdAsync(returnRequestId);
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="id">Return request identifier</param>
        /// <returns>Return request</returns>
        public virtual Task<ReturnRequest> GetReturnRequestById(int id)
        {
            return _returnRequestRepository.Table.Where(x=>x.ReturnNumber == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; null to load all entries</param>
        /// <param name="orderItemId">Order item identifier; 0 to load all entries</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="ownerId">Owner identifier</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Return requests</returns>
        public virtual async Task<IPagedList<ReturnRequest>> SearchReturnRequests(string storeId = "", string customerId = "",
            string orderItemId = "", string vendorId = "", string ownerId = "", ReturnRequestStatus? rs = null,
            int pageIndex = 0, int pageSize = int.MaxValue, DateTime? createdFromUtc = null, DateTime? createdToUtc = null)
        {
            var model = new GetReturnRequestQuery() {
                CreatedFromUtc = createdFromUtc,
                CreatedToUtc = createdToUtc,
                PageIndex = pageIndex,
                PageSize = pageSize,
                CustomerId = customerId,
                VendorId = vendorId,
                OwnerId = ownerId,
                StoreId = storeId,
                OrderItemId = orderItemId,
                Rs = rs,
            };

            var query = await _mediator.Send(model);
            return await PagedList<ReturnRequest>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Delete a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual async Task DeleteReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            await _returnRequestActionRepository.DeleteAsync(returnRequestAction);

            //event notification
            await _mediator.EntityDeleted(returnRequestAction);
        }

        /// <summary>
        /// Gets all return request actions
        /// </summary>
        /// <returns>Return request actions</returns>
        public virtual async Task<IList<ReturnRequestAction>> GetAllReturnRequestActions()
        {
            var query = from rra in _returnRequestActionRepository.Table
                        orderby rra.DisplayOrder, rra.Id
                        select rra;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a return request action
        /// </summary>
        /// <param name="returnRequestActionId">Return request action identifier</param>
        /// <returns>Return request action</returns>
        public virtual Task<ReturnRequestAction> GetReturnRequestActionById(string returnRequestActionId)
        {
            return _returnRequestActionRepository.GetByIdAsync(returnRequestActionId);
        }

        /// <summary>
        /// Inserts a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual async Task InsertReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            await _returnRequestActionRepository.InsertAsync(returnRequestAction);

            //event notification
            await _mediator.EntityInserted(returnRequestAction);
        }

        /// <summary>
        /// Inserts a return request 
        /// </summary>
        /// <param name="returnRequest">Return request </param>
        public virtual async Task InsertReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            lock (_locker)
            {
                var requestExists = _returnRequestRepository.Table.FirstOrDefault();
                var requestNumber = requestExists != null ? _returnRequestRepository.Table.Max(x => x.ReturnNumber) + 1 : 1;
                returnRequest.ReturnNumber = requestNumber;
            }
            await _returnRequestRepository.InsertAsync(returnRequest);

            //event notification
            await _mediator.EntityInserted(returnRequest);
        }
        /// <summary>
        /// Updates the  return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual async Task UpdateReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            await _returnRequestActionRepository.UpdateAsync(returnRequestAction);

            //event notification
            await _mediator.EntityUpdated(returnRequestAction);
        }




        /// <summary>
        /// Delete a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        public virtual async Task DeleteReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            await _returnRequestReasonRepository.DeleteAsync(returnRequestReason);

            //event notification
            await _mediator.EntityDeleted(returnRequestReason);
        }

        /// <summary>
        /// Gets all return request reaspns
        /// </summary>
        /// <returns>Return request reaspns</returns>
        public virtual async Task<IList<ReturnRequestReason>> GetAllReturnRequestReasons()
        {
            var query = from rra in _returnRequestReasonRepository.Table
                        orderby rra.DisplayOrder, rra.Id
                        select rra;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a return request reaspn
        /// </summary>
        /// <param name="returnRequestReasonId">Return request reaspn identifier</param>
        /// <returns>Return request reaspn</returns>
        public virtual Task<ReturnRequestReason> GetReturnRequestReasonById(string returnRequestReasonId)
        {
            return _returnRequestReasonRepository.GetByIdAsync(returnRequestReasonId);
        }

        /// <summary>
        /// Inserts a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reaspn</param>
        public virtual async Task InsertReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            await _returnRequestReasonRepository.InsertAsync(returnRequestReason);

            //event notification
            await _mediator.EntityInserted(returnRequestReason);
        }

        /// <summary>
        /// Updates the  return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reaspn</param>
        public virtual async Task UpdateReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            await _returnRequestReasonRepository.UpdateAsync(returnRequestReason);

            //event notification
            await _mediator.EntityUpdated(returnRequestReason);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnRequest"></param>
        public virtual async Task UpdateReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            await _returnRequestRepository.UpdateAsync(returnRequest);

            //event notification
            await _mediator.EntityUpdated(returnRequest);
        }

        #region Return request notes

        /// <summary>
        /// Deletes a return request note
        /// </summary>
        /// <param name="returnRequestNote">The return request note</param>
        public virtual async Task DeleteReturnRequestNote(ReturnRequestNote returnRequestNote)
        {
            if (returnRequestNote == null)
                throw new ArgumentNullException("returnRequestNote");

            await _returnRequestNoteRepository.DeleteAsync(returnRequestNote);

            //event notification
            await _mediator.EntityDeleted(returnRequestNote);
        }

        /// <summary>
        /// Inserts a return request note
        /// </summary>
        /// <param name="returnRequestNote">The return request note</param>
        public virtual async Task InsertReturnRequestNote(ReturnRequestNote returnRequestNote)
        {
            if (returnRequestNote == null)
                throw new ArgumentNullException("returnRequestNote");

            await _returnRequestNoteRepository.InsertAsync(returnRequestNote);

            //event notification
            await _mediator.EntityInserted(returnRequestNote);
        }

        /// <summary>
        /// Get notes related to return request
        /// </summary>
        /// <param name="returnRequestId">The return request identifier</param>
        /// <returns>List of return request notes</returns>
        public virtual async Task<IList<ReturnRequestNote>> GetReturnRequestNotes(string returnRequestId)
        {
            var query = from returnRequestNote in _returnRequestNoteRepository.Table
                        where returnRequestNote.ReturnRequestId == returnRequestId
                        orderby returnRequestNote.CreatedOnUtc descending
                        select returnRequestNote;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get return request note by id
        /// </summary>
        /// <param name="returnRequestNoteId">The return request note identifier</param>
        /// <returns>ReturnRequestNote</returns>
        public virtual Task<ReturnRequestNote> GetReturnRequestNote(string returnRequestNoteId)
        {
            return _returnRequestNoteRepository.Table.Where(x => x.Id == returnRequestNoteId).FirstOrDefaultAsync();
        }

        #endregion

        #endregion
    }
}
