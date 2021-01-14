using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerAttributeValidator : BaseGrandValidator<CustomerAttributeModel>
    {
        public CustomerAttributeValidator(
            IEnumerable<IValidatorConsumer<CustomerAttributeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Fields.Name.Required"));
        }
    }
}