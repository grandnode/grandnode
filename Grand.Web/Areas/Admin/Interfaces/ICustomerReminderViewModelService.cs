using Grand.Domain.Customers;
using Grand.Services.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerReminderViewModelService
    {
        CustomerReminderModel PrepareCustomerReminderModel();
        Task<CustomerReminder> InsertCustomerReminderModel(CustomerReminderModel model);
        Task<CustomerReminder> UpdateCustomerReminderModel(CustomerReminder customerReminder, CustomerReminderModel model);
        Task RunReminder(CustomerReminder customerReminder);
        Task DeleteCustomerReminder(CustomerReminder customerReminder);
        Task<SerializeCustomerReminderHistoryModel> PrepareHistoryModelForList(SerializeCustomerReminderHistory history);
        CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder);
        Task<CustomerReminder.ReminderCondition> InsertConditionModel(CustomerReminder customerReminder, CustomerReminderModel.ConditionModel model);
        CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition);
        Task<CustomerReminder.ReminderCondition> UpdateConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition, CustomerReminderModel.ConditionModel model);
        Task ConditionDelete(string Id, string customerReminderId);
        Task ConditionDeletePosition(string id, string customerReminderId, string conditionId);
        Task InsertCategoryConditionModel(CustomerReminderModel.ConditionModel.AddCategoryConditionModel model);
        Task InsertManufacturerConditionModel(CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model);
        Task InsertProductToConditionModel(CustomerReminderModel.ConditionModel.AddProductToConditionModel model);
        Task InsertCustomerTagConditionModel(CustomerReminderModel.ConditionModel.AddCustomerTagConditionModel model);
        Task InsertCustomerRoleConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRoleConditionModel model);
        Task InsertCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model);
        Task UpdateCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model);
        Task InsertCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model);
        Task UpdateCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model);
        Task PrepareReminderLevelModel(CustomerReminderModel.ReminderLevelModel model, CustomerReminder customerReminder);
        Task<CustomerReminder.ReminderLevel> InsertReminderLevel(CustomerReminder customerReminder, CustomerReminderModel.ReminderLevelModel model);
        Task<CustomerReminder.ReminderLevel> UpdateReminderLevel(CustomerReminder customerReminder, CustomerReminder.ReminderLevel customerReminderLevel, CustomerReminderModel.ReminderLevelModel model);
        Task DeleteLevel(string Id, string customerReminderId);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerActionConditionModel.AddProductToConditionModel model, int pageIndex, int pageSize);
        Task<CustomerReminderModel.ConditionModel.AddProductToConditionModel> PrepareProductToConditionModel(string customerReminderId, string conditionId);
    }
}
