using Grand.Domain.Shipping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    public interface IDeliveryDateService
    {
        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        Task<DeliveryDate> GetDeliveryDateById(string deliveryDateId);

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        Task<IList<DeliveryDate>> GetAllDeliveryDates();

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        Task InsertDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        Task UpdateDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        Task DeleteDeliveryDate(DeliveryDate deliveryDate);
    }
}
