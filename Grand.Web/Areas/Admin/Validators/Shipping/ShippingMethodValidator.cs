using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Shipping;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Shipping
{
    public class ShippingMethodValidator : BaseGrandValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator(
            IEnumerable<IValidatorConsumer<ShippingMethodModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Name.Required"));
        }
    }
}