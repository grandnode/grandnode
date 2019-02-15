using Grand.Api.DTOs.Shipping;
using Grand.Api.Interfaces;
using Grand.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public partial class ShippingApiService : IShippingApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IMongoCollection<WarehouseDto> _warehouseDto;
        private readonly IMongoCollection<DeliveryDateDto> _deliveryDateDto;
        private readonly IMongoCollection<PickupPointDto> _pickupPointDto;
        private readonly IMongoCollection<ShippingMethodDto> _shippingMethodDto;

        public ShippingApiService(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
            _warehouseDto = _mongoDBContext.Database().GetCollection<WarehouseDto>(typeof(Core.Domain.Shipping.Warehouse).Name);
            _deliveryDateDto = _mongoDBContext.Database().GetCollection<DeliveryDateDto>(typeof(Core.Domain.Shipping.DeliveryDate).Name);
            _pickupPointDto = _mongoDBContext.Database().GetCollection<PickupPointDto>(typeof(Core.Domain.Shipping.PickupPoint).Name);
            _shippingMethodDto = _mongoDBContext.Database().GetCollection<ShippingMethodDto>(typeof(Core.Domain.Shipping.ShippingMethod).Name);
        }

        public virtual IMongoQueryable<WarehouseDto> GetWarehouses()
        {
            return _warehouseDto.AsQueryable();
        }
        public virtual IMongoQueryable<DeliveryDateDto> GetDeliveryDates()
        {
            return _deliveryDateDto.AsQueryable();
        }
        public virtual IMongoQueryable<PickupPointDto> GetPickupPoints()
        {
            return _pickupPointDto.AsQueryable();
        }
        public virtual IMongoQueryable<ShippingMethodDto> GetShippingMethods()
        {
            return _shippingMethodDto.AsQueryable();
        }
    }
}
