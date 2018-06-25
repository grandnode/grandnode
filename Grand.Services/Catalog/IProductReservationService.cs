using Grand.Core;
using Grand.Core.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product reservation service interface
    /// </summary>
    public partial interface IProductReservationService
    {
        /// <summary>
        /// Deletes a product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        void DeleteProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Adds product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        void InsertProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        void UpdateProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Gets product reservations for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Product reservations</returns>
        IPagedList<ProductReservation> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? date,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets product reservation for specified Id
        /// </summary>
        /// <param name="Id">Product Id</param>
        /// <returns>Product reservation</returns>
        ProductReservation GetProductReservation(string Id);

        /// <summary>
        /// Adds customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        void InsertCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Deletes customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        void DeleteCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Cancel reservations by orderId 
        /// </summary>
        /// <param name="orderId"></param>
        void CancelReservationsByOrderId(string orderId);

        /// <summary>
        /// Gets customer reservations helper by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>CustomerReservationsHelper</returns>
        CustomerReservationsHelper GetCustomerReservationsHelperById(string Id);

        /// <summary>
        /// Gets customer reservations helpers
        /// </summary>
        /// <returns>List<CustomerReservationsHelper></returns>
        List<CustomerReservationsHelper> GetCustomerReservationsHelpers();

        /// <summary>
        /// Gets customer reservations helper by Shopping Cart Item id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>List<CustomerReservationsHelper></returns>
        List<CustomerReservationsHelper> GetCustomerReservationsHelperBySciId(string sciId);
    }
}
