using FluentValidation;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Shipping
{
    public class PickupPointValidator : BaseGrandValidator<PickupPointModel>
    {
        public PickupPointValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.PickupPoints.Fields.Name.Required"));
        }
    }
}