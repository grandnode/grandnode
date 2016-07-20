using FluentValidation;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;
using Grand.Web.Models.PrivateMessages;

namespace Grand.Web.Validators.PrivateMessages
{
    public class SendPrivateMessageValidator : BaseNopValidator<SendPrivateMessageModel>
    {
        public SendPrivateMessageValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("PrivateMessages.SubjectCannotBeEmpty"));
            RuleFor(x => x.Message).NotEmpty().WithMessage(localizationService.GetResource("PrivateMessages.MessageCannotBeEmpty"));
        }
    }
}