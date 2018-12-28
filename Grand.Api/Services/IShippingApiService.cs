using Grand.Api.DTOs.Shipping;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface IShippingApiService
    {
        IMongoQueryable<WarehouseDto> GetWarehouses();
        IMongoQueryable<DeliveryDateDto> GetDeliveryDates();
        IMongoQueryable<PickupPointDto> GetPickupPoints();
        IMongoQueryable<ShippingMethodDto> GetShippingMethods();
    }
}
