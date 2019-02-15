using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerReminderViewModelService
    {
        CustomerReminderModel PrepareCustomerReminderModel();
        CustomerReminder InsertCustomerReminderModel(CustomerReminderModel model);
        CustomerReminder UpdateCustomerReminderModel(CustomerReminder customerReminder, CustomerReminderModel model);
        void RunReminder(CustomerReminder customerReminder);
        void DeleteCustomerReminder(CustomerReminder customerReminder);
        SerializeCustomerReminderHistoryModel PrepareHistoryModelForList(SerializeCustomerReminderHistory history);
        CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder);
        CustomerReminder.ReminderCondition InsertConditionModel(CustomerReminder customerReminder, CustomerReminderModel.ConditionModel model);
        CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition);
        CustomerReminder.ReminderCondition UpdateConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition, CustomerReminderModel.ConditionModel model);
        void ConditionDelete(string Id, string customerReminderId);
        void ConditionDeletePosition(string id, string customerReminderId, string conditionId);
        void InsertCategoryConditionModel(CustomerReminderModel.ConditionModel.AddCategoryConditionModel model);
        void InsertManufacturerConditionModel(CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model);
        void InsertProductToConditionModel(CustomerReminderModel.ConditionModel.AddProductToConditionModel model);
        void InsertCustomerTagConditionModel(CustomerReminderModel.ConditionModel.AddCustomerTagConditionModel model);
        void InsertCustomerRoleConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRoleConditionModel model);
        void InsertCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model);
        void UpdateCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model);
        void InsertCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model);
        void UpdateCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model);
        void PrepareReminderLevelModel(CustomerReminderModel.ReminderLevelModel model, CustomerReminder customerReminder);
        CustomerReminder.ReminderLevel InsertReminderLevel(CustomerReminder customerReminder, CustomerReminderModel.ReminderLevelModel model);
        CustomerReminder.ReminderLevel UpdateReminderLevel(CustomerReminder customerReminder, CustomerReminder.ReminderLevel customerReminderLevel, CustomerReminderModel.ReminderLevelModel model);
        void DeleteLevel(string Id, string customerReminderId);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerActionConditionModel.AddProductToConditionModel model, int pageIndex, int pageSize);
        CustomerReminderModel.ConditionModel.AddProductToConditionModel PrepareProductToConditionModel(string customerReminderId, string conditionId);
    }
}
