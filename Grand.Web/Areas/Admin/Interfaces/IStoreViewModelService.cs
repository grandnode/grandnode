using Grand.Core.Domain.Stores;
using Grand.Web.Areas.Admin.Models.Stores;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IStoreViewModelService
    {
        void PrepareLanguagesModel(StoreModel model);
        void PrepareWarehouseModel(StoreModel model);
        StoreModel PrepareStoreModel();
        Store InsertStoreModel(StoreModel model);
        Store UpdateStoreModel(Store store, StoreModel model);
        void DeleteStore(Store store);

    }
}
