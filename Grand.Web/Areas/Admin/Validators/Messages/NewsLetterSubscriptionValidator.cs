using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class NewsLetterSubscriptionValidator : BaseGrandValidator<NewsLetterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator(
            IEnumerable<IValidatorConsumer<NewsLetterSubscriptionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
        }
    }
}