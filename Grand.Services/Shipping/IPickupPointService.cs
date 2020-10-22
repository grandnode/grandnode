using Grand.Domain.Shipping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    public interface IPickupPointService
    {

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>PickupPoint</returns>
        Task<PickupPoint> GetPickupPointById(string pickupPointId);

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        Task<IList<PickupPoint>> GetAllPickupPoints();

        /// <summary>
        /// Gets active pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        Task<IList<PickupPoint>> LoadActivePickupPoints(string storeId = "");

        /// <summary>
        /// Inserts a pickupPoint
        /// </summary>
        /// <param name="PickupPoint">PickupPoint</param>
        Task InsertPickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Updates the pickupPoint
        /// </summary>
        /// <param name="pickupPoint">PickupPoint</param>
        Task UpdatePickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Deletes a pickupPoint
        /// </summary>
        /// <param name="pickupPoint">The pickupPoint</param>
        Task DeletePickupPoint(PickupPoint pickuppoint);
    }
}
