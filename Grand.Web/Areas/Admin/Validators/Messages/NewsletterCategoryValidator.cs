using FluentValidation;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class NewsletterCategoryValidator : BaseGrandValidator<NewsletterCategoryModel>
    {
        public NewsletterCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.NewsletterCategory.Fields.Name.Required"));
        }
    }
}