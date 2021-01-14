using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Shipping;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Shipping
{
    public class DeliveryDateValidator : BaseGrandValidator<DeliveryDateModel>
    {
        public DeliveryDateValidator(
            IEnumerable<IValidatorConsumer<DeliveryDateModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Fields.Name.Required"));
        }
    }
}