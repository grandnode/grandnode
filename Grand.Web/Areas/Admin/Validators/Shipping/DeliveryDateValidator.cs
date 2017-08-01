using FluentValidation;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Shipping
{
    public class DeliveryDateValidator : BaseGrandValidator<DeliveryDateModel>
    {
        public DeliveryDateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Fields.Name.Required"));
        }
    }
}