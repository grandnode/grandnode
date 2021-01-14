using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerActionConditionValidator : BaseGrandValidator<CustomerActionConditionModel>
    {
        public CustomerActionConditionValidator(
            IEnumerable<IValidatorConsumer<CustomerActionConditionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerActionCondition.Fields.Name.Required"));
        }
    }
}