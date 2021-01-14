using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Orders
{
    public class CheckoutAttributeValueValidator : BaseGrandValidator<CheckoutAttributeValueModel>
    {
        public CheckoutAttributeValueValidator(
            IEnumerable<IValidatorConsumer<CheckoutAttributeValueModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name.Required"));
        }
    }
}