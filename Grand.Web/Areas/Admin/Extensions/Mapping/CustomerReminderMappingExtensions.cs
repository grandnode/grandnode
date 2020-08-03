using Grand.Domain.Customers;
using Grand.Services.Helpers;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class CustomerReminderMappingExtensions
    {
        public static CustomerReminderModel ToModel(this CustomerReminder entity, IDateTimeHelper dateTimeHelper)
        {
            var reminder = entity.MapTo<CustomerReminder, CustomerReminderModel>();
            reminder.StartDateTime = entity.StartDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            reminder.EndDateTime = entity.EndDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            reminder.LastUpdateDate = entity.LastUpdateDate.ConvertToUserTime(dateTimeHelper);
            return reminder;

        }

        public static CustomerReminder ToEntity(this CustomerReminderModel model, IDateTimeHelper dateTimeHelper)
        {
            var reminder = model.MapTo<CustomerReminderModel, CustomerReminder>();
            reminder.StartDateTimeUtc = model.StartDateTime.ConvertToUtcTime(dateTimeHelper);
            reminder.EndDateTimeUtc = model.EndDateTime.ConvertToUtcTime(dateTimeHelper);
            reminder.LastUpdateDate = model.LastUpdateDate.ConvertToUtcTime(dateTimeHelper);
            return reminder;

        }

        public static CustomerReminder ToEntity(this CustomerReminderModel model, CustomerReminder destination, IDateTimeHelper dateTimeHelper)
        {
            var reminder = model.MapTo(destination);
            reminder.StartDateTimeUtc = model.StartDateTime.ConvertToUtcTime(dateTimeHelper);
            reminder.EndDateTimeUtc = model.EndDateTime.ConvertToUtcTime(dateTimeHelper);
            reminder.LastUpdateDate = model.LastUpdateDate.ConvertToUtcTime(dateTimeHelper);
            return reminder;
        }

        public static CustomerReminderModel.ReminderLevelModel ToModel(this CustomerReminder.ReminderLevel entity)
        {
            return entity.MapTo<CustomerReminder.ReminderLevel, CustomerReminderModel.ReminderLevelModel>();
        }

        public static CustomerReminder.ReminderLevel ToEntity(this CustomerReminderModel.ReminderLevelModel model)
        {
            return model.MapTo<CustomerReminderModel.ReminderLevelModel, CustomerReminder.ReminderLevel>();
        }

        public static CustomerReminder.ReminderLevel ToEntity(this CustomerReminderModel.ReminderLevelModel model, CustomerReminder.ReminderLevel destination)
        {
            return model.MapTo(destination);
        }

        public static CustomerReminderModel.ConditionModel ToModel(this CustomerReminder.ReminderCondition entity)
        {
            return entity.MapTo<CustomerReminder.ReminderCondition, CustomerReminderModel.ConditionModel>();
        }

        public static CustomerReminder.ReminderCondition ToEntity(this CustomerReminderModel.ConditionModel model)
        {
            return model.MapTo<CustomerReminderModel.ConditionModel, CustomerReminder.ReminderCondition>();
        }

        public static CustomerReminder.ReminderCondition ToEntity(this CustomerReminderModel.ConditionModel model, CustomerReminder.ReminderCondition destination)
        {
            return model.MapTo(destination);
        }
    }
}