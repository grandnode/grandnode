using Grand.Api.DTOs.Shipping;
using Grand.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public partial class ShippingApiService : IShippingApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IMongoCollection<WarehouseDto> _warehouseDto;
        public ShippingApiService(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
            _warehouseDto = _mongoDBContext.Database().GetCollection<WarehouseDto>(typeof(Core.Domain.Shipping.Warehouse).Name);
        }

        public virtual IMongoQueryable<WarehouseDto> GetWarehouses()
        {
            return _warehouseDto.AsQueryable();
        }
    }
}
