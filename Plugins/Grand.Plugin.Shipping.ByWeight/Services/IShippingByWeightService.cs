using Grand.Core;
using Grand.Plugin.Shipping.ByWeight.Domain;

namespace Grand.Plugin.Shipping.ByWeight.Services
{
    public partial interface IShippingByWeightService
    {
        void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        IPagedList<ShippingByWeightRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue);

        ShippingByWeightRecord FindRecord(string shippingMethodId,
            string storeId, string warehouseId,
            string countryId, string stateProvinceId, string zip, decimal weight);

        ShippingByWeightRecord GetById(string shippingByWeightRecordId);

        void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);
    }

}
