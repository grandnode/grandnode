using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerActionValidator : BaseGrandValidator<CustomerActionModel>
    {
        public CustomerActionValidator(
            IEnumerable<IValidatorConsumer<CustomerActionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAction.Fields.Name.Required"));            
        }
    }
}