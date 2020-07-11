using Grand.Domain;
using Grand.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task DeleteProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Adds product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task InsertProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task UpdateProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Gets product reservations for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Product reservations</returns>
        Task<IPagedList<ProductReservation>> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? date,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets product reservation for specified Id
        /// </summary>
        /// <param name="Id">Product Id</param>
        /// <returns>Product reservation</returns>
        Task<ProductReservation> GetProductReservation(string Id);

        /// <summary>
        /// Adds customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        Task InsertCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Deletes customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        Task DeleteCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Cancel reservations by orderId 
        /// </summary>
        /// <param name="orderId"></param>
        Task CancelReservationsByOrderId(string orderId);

        /// <summary>
        /// Gets customer reservations helper by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>CustomerReservationsHelper</returns>
        Task<CustomerReservationsHelper> GetCustomerReservationsHelperById(string Id);

        /// <summary>
        /// Gets customer reservations helpers
        /// </summary>
        /// <param name="customerId">Customer ident</param>
        /// <returns>List<CustomerReservationsHelper></returns>
        Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelpers(string customerId);

        /// <summary>
        /// Gets customer reservations helper by Shopping Cart Item id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>List<CustomerReservationsHelper></returns>
        Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelperBySciId(string sciId);
    }
}
