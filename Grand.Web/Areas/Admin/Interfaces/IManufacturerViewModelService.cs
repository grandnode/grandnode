using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IManufacturerViewModelService
    {
        void PrepareSortOptionsModel(ManufacturerModel model);
        Task PrepareTemplatesModel(ManufacturerModel model);
        Task PrepareDiscountModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties);
        Task<Manufacturer> InsertManufacturerModel(ManufacturerModel model);
        Task<Manufacturer> UpdateManufacturerModel(Manufacturer manufacturer, ManufacturerModel model);
        Task DeleteManufacturer(Manufacturer manufacturer);
        Task<ManufacturerModel.AddManufacturerProductModel> PrepareAddManufacturerProductModel(string storeId);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ManufacturerModel.AddManufacturerProductModel model, int pageIndex, int pageSize);
        Task<(IEnumerable<ManufacturerModel.ManufacturerProductModel> manufacturerProductModels, int totalCount)> PrepareManufacturerProductModel(string manufacturerId, string storeId, int pageIndex, int pageSize);
        Task ProductUpdate(ManufacturerModel.ManufacturerProductModel model);
        Task ProductDelete(string id, string productId);
        Task InsertManufacturerProductModel(ManufacturerModel.AddManufacturerProductModel model);
        Task<(IEnumerable<ManufacturerModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string manufacturerId, int pageIndex, int pageSize);
    }
}
