using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Boards;

namespace Grand.Web.Validators.Boards
{
    public class EditForumTopicValidator : BaseGrandValidator<EditForumTopicModel>
    {
        public EditForumTopicValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("Forum.TopicSubjectCannotBeEmpty"));
            RuleFor(x => x.Text).NotEmpty().WithMessage(localizationService.GetResource("Forum.TextCannotBeEmpty"));
        }
    }
}