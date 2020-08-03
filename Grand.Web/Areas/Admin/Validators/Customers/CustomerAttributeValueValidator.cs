using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerAttributeValueValidator : BaseGrandValidator<CustomerAttributeValueModel>
    {
        public CustomerAttributeValueValidator(
            IEnumerable<IValidatorConsumer<CustomerAttributeValueModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Values.Fields.Name.Required"));
        }
    }
}