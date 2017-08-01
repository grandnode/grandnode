using FluentValidation;
using Grand.Web.Areas.Admin.Models.Polls;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Polls
{
    public class PollAnswerValidator : BaseGrandValidator<PollAnswerModel>
    {
        public PollAnswerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Polls.Answers.Fields.Name.Required"));
        }
    }
}