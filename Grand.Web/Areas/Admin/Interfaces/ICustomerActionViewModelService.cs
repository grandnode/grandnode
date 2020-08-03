using Grand.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerActionViewModelService
    {
        Task PrepareReactObjectModel(CustomerActionModel model);
        Task<CustomerActionModel> PrepareCustomerActionModel();
        Task<CustomerAction> InsertCustomerActionModel(CustomerActionModel model);
        Task<CustomerAction> UpdateCustomerActionModel(CustomerAction customeraction, CustomerActionModel model);
        Task<SerializeCustomerActionHistory> PrepareHistoryModelForList(CustomerActionHistory history);
        Task<CustomerActionConditionModel> PrepareCustomerActionConditionModel(string customerActionId);
        Task<(string customerActionId, string conditionId)> InsertCustomerActionConditionModel(CustomerActionConditionModel model);
        Task<CustomerAction> UpdateCustomerActionConditionModel(CustomerAction customeraction, CustomerAction.ActionCondition actionCondition, CustomerActionConditionModel model);
        Task ConditionDelete(string Id, string customerActionId);
        Task ConditionDeletePosition(string id, string customerActionId, string conditionId);
        Task<CustomerActionConditionModel.AddProductToConditionModel> PrepareAddProductToConditionModel(string customerActionId, string conditionId);
        Task InsertProductToConditionModel(CustomerActionConditionModel.AddProductToConditionModel model);
        Task InsertCategoryConditionModel(CustomerActionConditionModel.AddCategoryConditionModel model);
        Task InsertManufacturerConditionModel(CustomerActionConditionModel.AddManufacturerConditionModel model);
        Task InsertCustomerRoleConditionModel(CustomerActionConditionModel.AddCustomerRoleConditionModel model);
        Task InsertStoreConditionModel(CustomerActionConditionModel.AddStoreConditionModel model);
        Task InsertVendorConditionModel(CustomerActionConditionModel.AddVendorConditionModel model);
        Task InsertCustomerTagConditionModel(CustomerActionConditionModel.AddCustomerTagConditionModel model);
        Task InsertProductAttributeConditionModel(CustomerActionConditionModel.AddProductAttributeConditionModel model);
        Task UpdateProductAttributeConditionModel(CustomerActionConditionModel.AddProductAttributeConditionModel model);
        Task InsertProductSpecificationConditionModel(CustomerActionConditionModel.AddProductSpecificationConditionModel model);
        Task InsertCustomerRegisterConditionModel(CustomerActionConditionModel.AddCustomerRegisterConditionModel model);
        Task UpdateCustomerRegisterConditionModel(CustomerActionConditionModel.AddCustomerRegisterConditionModel model);
        Task InsertCustomCustomerAttributeConditionModel(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model);
        Task UpdateCustomCustomerAttributeConditionModel(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model);
        Task InsertUrlConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        Task UpdateUrlConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        Task InsertUrlCurrentConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        Task UpdateUrlCurrentConditionModel(CustomerActionConditionModel.AddUrlConditionModel model);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerActionConditionModel.AddProductToConditionModel model, int pageIndex, int pageSize);

    }
}
