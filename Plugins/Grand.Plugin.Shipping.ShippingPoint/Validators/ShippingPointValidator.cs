using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Plugin.Shipping.ShippingPoint.Models;

namespace Grand.Plugin.Shipping.ShippingPoint.Validators
{
    class ShippingPointValidator : BaseGrandValidator<ShippingPointModel>
    {
        public ShippingPointValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ShippingPointName).NotEmpty().WithMessage(localizationService.GetResource("Plugins.Shipping.ShippingPoint.RequiredShippingPointName"));
            RuleFor(x => x.Description).NotEmpty().WithMessage(localizationService.GetResource("Plugins.Shipping.ShippingPoint.RequiredDescription"));
            RuleFor(x => x.OpeningHours).NotEmpty().WithMessage(localizationService.GetResource("Plugins.Shipping.ShippingPoint.RequiredOpeningHours"));
            RuleFor(x => x.CountryId).NotNull().WithMessage(localizationService.GetResource("Admin.Address.Fields.Country.Required"));
            RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.Fields.City.Required"));
            RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.Fields.Address1.Required"));
            RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.Fields.ZipPostalCode.Required"));
        }
    }

}