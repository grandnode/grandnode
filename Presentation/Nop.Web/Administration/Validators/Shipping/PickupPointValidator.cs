using FluentValidation;
using Nop.Admin.Models.Shipping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Shipping
{
    public class PickupPointValidator : BaseNopValidator<PickupPointModel>
    {
        public PickupPointValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.PickupPoints.Fields.Name.Required"));
        }
    }
}