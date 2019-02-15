using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerActionViewModelService
    {
        void PrepareReactObjectModel(CustomerActionModel model);
        CustomerActionModel PrepareCustomerActionModel();
        CustomerAction InsertCustomerActionModel(CustomerActionModel model);
        CustomerAction UpdateCustomerActionModel(CustomerAction customeraction, CustomerActionModel model);
        SerializeCustomerActionHistory PrepareHistoryModelForList(CustomerActionHistory history);
        CustomerActionConditionModel PrepareCustomerActionConditionModel(string customerActionId);
        (string customerActionId, string conditionId) InsertCustomerActionConditionModel(CustomerActionConditionModel model);
        CustomerAction UpdateCustomerActionConditionModel(CustomerAction customeraction, CustomerAction.ActionCondition actionCondition, CustomerActionConditionModel model);
        void ConditionDelete(string Id, string customerActionId);
        void ConditionDeletePosition(string id, string customerActionId, string conditionId);
        CustomerActionConditionModel.AddProductToConditionModel PrepareAddProductToConditionModel(string customerActionId, string conditionId);
        void InsertProductToConditionModel(CustomerActionConditionModel.AddProductToConditionModel model);
        void InsertCategoryConditionModel(CustomerActionConditionModel.AddCategoryConditionModel model);
        void InsertManufacturerConditionModel(CustomerActionConditionModel.AddManufacturerConditionModel model);
        void InsertCustomerRoleConditionModel(CustomerActionConditionModel.AddCustomerRoleConditionModel model);
        void InsertStoreConditionModel(CustomerActionConditionModel.AddStoreConditionModel model);
        void InsertVendorConditionModel(CustomerActionConditionModel.AddVendorConditionModel model);
        void InsertCustomerTagConditionModel(CustomerActionConditionModel.AddCustomerTagConditionModel model);
        void InsertProductAttributeConditionModel(CustomerActionConditionModel.AddProductAttributeConditionModel model);
        void UpdateProductAttributeConditionModel(CustomerActionConditionModel.AddProductAttributeConditionModel model);
        void InsertProductSpecificationConditionModel(CustomerActionConditionModel.AddProductSpecificationConditionModel model);
        void InsertCustomerRegisterConditionModel(CustomerActionConditionModel.AddCustomerRegisterConditionModel model);
        void UpdateCustomerRegisterConditionModel(CustomerActionConditionModel.AddCustomerRegisterConditionModel model);
        void InsertCustomCustomerAttributeConditionModel(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model);
        void UpdateCustomCustomerAttributeConditionModel(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model);
        void InsertUrlConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        void UpdateUrlConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        void InsertUrlCurrentConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        void UpdateUrlCurrentConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerActionConditionModel.AddProductToConditionModel model, int pageIndex, int pageSize);

    }
}
