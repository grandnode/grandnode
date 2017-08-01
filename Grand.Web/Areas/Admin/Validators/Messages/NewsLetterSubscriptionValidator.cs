using FluentValidation;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class NewsLetterSubscriptionValidator : BaseGrandValidator<NewsLetterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
        }
    }
}