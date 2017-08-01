using FluentValidation;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Shipping
{
    public class ShippingMethodValidator : BaseGrandValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Name.Required"));
        }
    }
}