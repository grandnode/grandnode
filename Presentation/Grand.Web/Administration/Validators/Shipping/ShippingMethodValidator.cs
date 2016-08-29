using FluentValidation;
using Grand.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Shipping
{
    public class ShippingMethodValidator : BaseNopValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Name.Required"));
        }
    }
}