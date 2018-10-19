using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product reservation service
    /// </summary>
    public partial class ProductReservationService : IProductReservationService
    {
        private readonly IRepository<ProductReservation> _productReservationRepository;
        private readonly IRepository<CustomerReservationsHelper> _customerReservationsHelperRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public ProductReservationService(IRepository<ProductReservation> productReservationRepository,
            IRepository<CustomerReservationsHelper> customerReservationsHelperRepository,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _productReservationRepository = productReservationRepository;
            _customerReservationsHelperRepository = customerReservationsHelperRepository;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        /// <summary>
        /// Deletes a product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual void DeleteProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productReservation");

            _productReservationRepository.Delete(productReservation);
            _eventPublisher.EntityDeleted(productReservation);
        }

        /// <summary>
        /// Gets product reservations for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Product reservations</returns>
        public virtual IPagedList<ProductReservation> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? date,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productReservationRepository.Table.Where(x => x.ProductId == productId);

            if (showVacant.HasValue)
            {
                if (showVacant.Value)
                {
                    query = query.Where(x => (x.OrderId == "" || x.OrderId == null));
                }
                else
                {
                    query = query.Where(x => (x.OrderId != "" && x.OrderId != null));
                }
            }

            if (date.HasValue)
            {
                var min = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0, 0);
                var max = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59, 999);

                query = query.Where(x => x.Date >= min && x.Date <= max);
            }

            query = query.OrderBy(x => x.Date);

            return new PagedList<ProductReservation>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Adds product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual void InsertProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productAttribute");

            _productReservationRepository.Insert(productReservation);
            _eventPublisher.EntityInserted(productReservation);
        }

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual void UpdateProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productAttribute");

            _productReservationRepository.Update(productReservation);
            _eventPublisher.EntityInserted(productReservation);
        }

        /// <summary>
        /// Gets product reservation for specified Id
        /// </summary>
        /// <param name="Id">Product Id</param>
        /// <returns>Product reservation</returns>
        public virtual ProductReservation GetProductReservation(string Id)
        {
            return _productReservationRepository.GetById(Id);
        }

        /// <summary>
        /// Adds customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        public virtual void InsertCustomerReservationsHelper(CustomerReservationsHelper crh)
        {
            if (crh == null)
                throw new ArgumentNullException("CustomerReservationsHelper");

            _customerReservationsHelperRepository.Insert(crh);
            _eventPublisher.EntityInserted(crh);
        }

        /// <summary>
        /// Deletes customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        public virtual void DeleteCustomerReservationsHelper(CustomerReservationsHelper crh)
        {
            if (crh == null)
                throw new ArgumentNullException("CustomerReservationsHelper");

            _customerReservationsHelperRepository.Delete(crh);
            _eventPublisher.EntityDeleted(crh);
        }


        /// <summary>
        /// Cancel reservations by orderId 
        /// </summary>
        /// <param name="orderId"></param>
        public virtual void CancelReservationsByOrderId(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var update = new UpdateDefinitionBuilder<ProductReservation>().Set(x => x.OrderId, "");
                var result = _productReservationRepository.Collection.UpdateManyAsync(x => x.OrderId == orderId, update).Result;
            }
        }

        /// <summary>
        /// Gets customer reservations helper by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>CustomerReservationsHelper</returns>
        public virtual CustomerReservationsHelper GetCustomerReservationsHelperById(string Id)
        {
            return _customerReservationsHelperRepository.GetById(Id);
        }

        /// <summary>
        /// Gets customer reservations helpers
        /// </summary>
        /// <returns>List<CustomerReservationsHelper></returns>
        public virtual List<CustomerReservationsHelper> GetCustomerReservationsHelpers()
        {
            return _customerReservationsHelperRepository.Table.Where(x => x.CustomerId == _workContext.CurrentCustomer.Id).ToList();
        }

        /// <summary>
        /// Gets customer reservations helper by Shopping Cart Item id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>List<CustomerReservationsHelper></returns>
        public virtual List<CustomerReservationsHelper> GetCustomerReservationsHelperBySciId(string sciId)
        {
            return _customerReservationsHelperRepository.Table.Where(x => x.ShoppingCartItemId == sciId).ToList();
        }
    }
}
