using FluentValidation;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class BannerValidator : BaseGrandValidator<BannerModel>
    {
        public BannerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Banners.Fields.Name.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Banners.Fields.Body.Required"));
        }
    }
}