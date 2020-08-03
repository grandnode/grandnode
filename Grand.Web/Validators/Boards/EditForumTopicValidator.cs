using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Boards;
using System.Collections.Generic;

namespace Grand.Web.Validators.Boards
{
    public class EditForumTopicValidator : BaseGrandValidator<EditForumTopicModel>
    {
        public EditForumTopicValidator(
            IEnumerable<IValidatorConsumer<EditForumTopicModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("Forum.TopicSubjectCannotBeEmpty"));
            RuleFor(x => x.Text).NotEmpty().WithMessage(localizationService.GetResource("Forum.TextCannotBeEmpty"));
        }
    }
}