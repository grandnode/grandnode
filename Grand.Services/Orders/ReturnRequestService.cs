using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Orders;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="returnRequestRepository">Return request repository</param>
        /// <param name="returnRequestActionRepository">Return request action repository</param>
        /// <param name="returnRequestReasonRepository">Return request reason repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ReturnRequestService(IRepository<ReturnRequest> returnRequestRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IEventPublisher eventPublisher)
        {
            this._returnRequestRepository = returnRequestRepository;
            this._returnRequestActionRepository = returnRequestActionRepository;
            this._returnRequestReasonRepository = returnRequestReasonRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a return request
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        public virtual void DeleteReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            _returnRequestRepository.Delete(returnRequest);

            //event notification
            _eventPublisher.EntityDeleted(returnRequest);
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        public virtual ReturnRequest GetReturnRequestById(string returnRequestId)
        {
            return _returnRequestRepository.GetById(returnRequestId);
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        public virtual ReturnRequest GetReturnRequestById(int id)
        {
            return _returnRequestRepository.Table.Where(x=>x.ReturnNumber == id).FirstOrDefault();
        }

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; null to load all entries</param>
        /// <param name="orderItemId">Order item identifier; 0 to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Return requests</returns>
        public virtual IPagedList<ReturnRequest> SearchReturnRequests(string storeId = "", string customerId = "",
            string orderItemId = "", ReturnRequestStatus? rs = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _returnRequestRepository.Table;
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(rr => storeId == rr.StoreId);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(rr => customerId == rr.CustomerId);
            if (rs.HasValue)
            {
                var returnStatusId = (int)rs.Value;
                query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
            }
            if (!String.IsNullOrEmpty(orderItemId))
                query = query.Where(rr => rr.ReturnRequestItems.Any(x => x.OrderItemId == orderItemId));

            query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);

            var returnRequests = new PagedList<ReturnRequest>(query, pageIndex, pageSize);
            return returnRequests;
        }

        /// <summary>
        /// Delete a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual void DeleteReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            _returnRequestActionRepository.Delete(returnRequestAction);

            //event notification
            _eventPublisher.EntityDeleted(returnRequestAction);
        }

        /// <summary>
        /// Gets all return request actions
        /// </summary>
        /// <returns>Return request actions</returns>
        public virtual IList<ReturnRequestAction> GetAllReturnRequestActions()
        {
            var query = from rra in _returnRequestActionRepository.Table
                        orderby rra.DisplayOrder, rra.Id
                        select rra;
            return query.ToList();
        }

        /// <summary>
        /// Gets a return request action
        /// </summary>
        /// <param name="returnRequestActionId">Return request action identifier</param>
        /// <returns>Return request action</returns>
        public virtual ReturnRequestAction GetReturnRequestActionById(string returnRequestActionId)
        {
            return _returnRequestActionRepository.GetById(returnRequestActionId);
        }

        /// <summary>
        /// Inserts a return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual void InsertReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            _returnRequestActionRepository.Insert(returnRequestAction);

            //event notification
            _eventPublisher.EntityInserted(returnRequestAction);
        }

        /// <summary>
        /// Inserts a return request 
        /// </summary>
        /// <param name="returnRequest">Return request </param>
        public virtual void InsertReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            lock (_locker)
            {
                var requestExists = _returnRequestRepository.Table.FirstOrDefault();
                var requestNumber = requestExists != null ? _returnRequestRepository.Table.Max(x => x.ReturnNumber) + 1 : 1;
                returnRequest.ReturnNumber = requestNumber;
                _returnRequestRepository.Insert(returnRequest);
            }

            //event notification
            _eventPublisher.EntityInserted(returnRequest);
        }
        /// <summary>
        /// Updates the  return request action
        /// </summary>
        /// <param name="returnRequestAction">Return request action</param>
        public virtual void UpdateReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException("returnRequestAction");

            _returnRequestActionRepository.Update(returnRequestAction);

            //event notification
            _eventPublisher.EntityUpdated(returnRequestAction);
        }




        /// <summary>
        /// Delete a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reason</param>
        public virtual void DeleteReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            _returnRequestReasonRepository.Delete(returnRequestReason);

            //event notification
            _eventPublisher.EntityDeleted(returnRequestReason);
        }

        /// <summary>
        /// Gets all return request reaspns
        /// </summary>
        /// <returns>Return request reaspns</returns>
        public virtual IList<ReturnRequestReason> GetAllReturnRequestReasons()
        {
            var query = from rra in _returnRequestReasonRepository.Table
                        orderby rra.DisplayOrder, rra.Id
                        select rra;
            return query.ToList();
        }

        /// <summary>
        /// Gets a return request reaspn
        /// </summary>
        /// <param name="returnRequestReasonId">Return request reaspn identifier</param>
        /// <returns>Return request reaspn</returns>
        public virtual ReturnRequestReason GetReturnRequestReasonById(string returnRequestReasonId)
        {
            return _returnRequestReasonRepository.GetById(returnRequestReasonId);
        }

        /// <summary>
        /// Inserts a return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reaspn</param>
        public virtual void InsertReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            _returnRequestReasonRepository.Insert(returnRequestReason);

            //event notification
            _eventPublisher.EntityInserted(returnRequestReason);
        }

        /// <summary>
        /// Updates the  return request reaspn
        /// </summary>
        /// <param name="returnRequestReason">Return request reaspn</param>
        public virtual void UpdateReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException("returnRequestReason");

            _returnRequestReasonRepository.Update(returnRequestReason);

            //event notification
            _eventPublisher.EntityUpdated(returnRequestReason);
        }

        #endregion
    }
}
