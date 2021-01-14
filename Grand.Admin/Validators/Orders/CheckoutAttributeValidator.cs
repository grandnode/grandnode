using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Orders
{
    public class CheckoutAttributeValidator : BaseGrandValidator<CheckoutAttributeModel>
    {
        public CheckoutAttributeValidator(
            IEnumerable<IValidatorConsumer<CheckoutAttributeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required"));
        }
    }
}