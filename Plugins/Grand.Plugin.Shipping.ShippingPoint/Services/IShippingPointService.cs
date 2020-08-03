using Grand.Domain;
using Grand.Plugin.Shipping.ShippingPoint.Domain;
using System.Threading.Tasks;

namespace Grand.Plugin.Shipping.ShippingPoint.Services
{
    /// <summary>
    /// Store pickup point service interface
    /// </summary>
    public interface IShippingPointService
    {
        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        Task<IPagedList<ShippingPoints>> GetAllStoreShippingPoint(string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointName"></param>
        /// <returns></returns>
        Task<ShippingPoints> GetStoreShippingPointByPointName(string pointName);

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="ShippingPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        Task<ShippingPoints> GetStoreShippingPointById(string ShippingPointId);

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        Task InsertStoreShippingPoint(Domain.ShippingPoints ShippingPoint);

        /// <summary>
        /// Updates a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        Task UpdateStoreShippingPoint(Domain.ShippingPoints ShippingPoint);

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        Task DeleteStoreShippingPoint(Domain.ShippingPoints ShippingPoint);
    }
}
