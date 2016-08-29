using FluentValidation;
using Grand.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Shipping
{
    public class DeliveryDateValidator : BaseNopValidator<DeliveryDateModel>
    {
        public DeliveryDateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Fields.Name.Required"));
        }
    }
}