using Grand.Domain.Shipping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    public interface IWarehouseService
    {

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        Task<Warehouse> GetWarehouseById(string warehouseId);

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        Task<IList<Warehouse>> GetAllWarehouses();

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        Task InsertWarehouse(Warehouse warehouse);

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        Task UpdateWarehouse(Warehouse warehouse);

        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        Task DeleteWarehouse(Warehouse warehouse);


    }
}
