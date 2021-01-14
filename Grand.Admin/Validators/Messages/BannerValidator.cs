using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Messages;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Messages
{
    public class BannerValidator : BaseGrandValidator<BannerModel>
    {
        public BannerValidator(
            IEnumerable<IValidatorConsumer<BannerModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Banners.Fields.Name.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Banners.Fields.Body.Required"));
        }
    }
}