using Grand.Core;
using Grand.Plugin.Shipping.ShippingPoint.Domain;

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
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        IPagedList<Domain.ShippingPoints> GetAllStoreShippingPoint(string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointName"></param>
        /// <returns></returns>
        Domain.ShippingPoints GetStoreShippingPointByPointName(string pointName);

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="ShippingPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        Domain.ShippingPoints GetStoreShippingPointById(string ShippingPointId);

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        void InsertStoreShippingPoint(Domain.ShippingPoints ShippingPoint);

        /// <summary>
        /// Updates a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        void UpdateStoreShippingPoint(Domain.ShippingPoints ShippingPoint);

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="ShippingPoint">Pickup point</param>
        void DeleteStoreShippingPoint(Domain.ShippingPoints ShippingPoint);
    }
}
