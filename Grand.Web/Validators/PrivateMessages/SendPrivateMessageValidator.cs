using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.PrivateMessages;
using System.Collections.Generic;

namespace Grand.Web.Validators.PrivateMessages
{
    public class SendPrivateMessageValidator : BaseGrandValidator<SendPrivateMessageModel>
    {
        public SendPrivateMessageValidator(
            IEnumerable<IValidatorConsumer<SendPrivateMessageModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("PrivateMessages.SubjectCannotBeEmpty"));
            RuleFor(x => x.Message).NotEmpty().WithMessage(localizationService.GetResource("PrivateMessages.MessageCannotBeEmpty"));
        }
    }
}