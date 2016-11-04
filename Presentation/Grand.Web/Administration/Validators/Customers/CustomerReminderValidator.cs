using FluentValidation;
using Grand.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerReminderValidator : BaseNopValidator<CustomerReminderModel>
    {
        public CustomerReminderValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Fields.Name.Required"));
        }
    }
    public class CustomerReminderLevelValidator : BaseNopValidator<CustomerReminderModel.ReminderLevelModel>
    {
        public CustomerReminderLevelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Name.Required"));
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Subject.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Body.Required"));
            RuleFor(x => x.Hour+x.Day+ x.Minutes).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.DayHourMin.Required"));
        }
    }
}