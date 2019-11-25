using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Boards;
using System.Collections.Generic;

namespace Grand.Web.Validators.Boards
{
    public class EditForumPostValidator : BaseGrandValidator<EditForumPostModel>
    {
        public EditForumPostValidator(
            IEnumerable<IValidatorConsumer<EditForumPostModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Text).NotEmpty().WithMessage(localizationService.GetResource("Forum.TextCannotBeEmpty"));
        }
    }
}