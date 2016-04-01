using FluentValidation;
using Nop.Admin.Models.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Customers
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
            RuleFor(x => x.Hour+x.Day).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.DayHour.Required"));
        }
    }
}