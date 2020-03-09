using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerReminderValidator : BaseGrandValidator<CustomerReminderModel>
    {
        public CustomerReminderValidator(
            IEnumerable<IValidatorConsumer<CustomerReminderModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Fields.Name.Required"));
        }
    }
    public class CustomerReminderLevelValidator : BaseGrandValidator<CustomerReminderModel.ReminderLevelModel>
    {
        public CustomerReminderLevelValidator(
            IEnumerable<IValidatorConsumer<CustomerReminderModel.ReminderLevelModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Name.Required"));
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Subject.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.Body.Required"));
            RuleFor(x => x.Hour+x.Day+ x.Minutes).NotEqual(0).WithMessage(localizationService.GetResource("Admin.Customers.CustomerReminder.Level.Fields.DayHourMin.Required"));
        }
    }
}