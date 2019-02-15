using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IManufacturerViewModelService
    {
        void PrepareTemplatesModel(ManufacturerModel model);
        void PrepareDiscountModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties);
        Manufacturer InsertManufacturerModel(ManufacturerModel model);
        Manufacturer UpdateManufacturerModel(Manufacturer manufacturer, ManufacturerModel model);
        void DeleteManufacturer(Manufacturer manufacturer);
        ManufacturerModel.AddManufacturerProductModel PrepareAddManufacturerProductModel();
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ManufacturerModel.AddManufacturerProductModel model, int pageIndex, int pageSize);
        (IEnumerable<ManufacturerModel.ManufacturerProductModel> manufacturerProductModels, int totalCount) PrepareManufacturerProductModel(string manufacturerId, int pageIndex, int pageSize);
        void ProductUpdate(ManufacturerModel.ManufacturerProductModel model);
        void ProductDelete(string id, string productId);
        void InsertManufacturerProductModel(ManufacturerModel.AddManufacturerProductModel model);
        (IEnumerable<ManufacturerModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string manufacturerId, int pageIndex, int pageSize);
    }
}
