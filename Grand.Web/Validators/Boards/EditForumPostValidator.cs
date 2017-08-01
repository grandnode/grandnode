using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Boards;

namespace Grand.Web.Validators.Boards
{
    public class EditForumPostValidator : BaseGrandValidator<EditForumPostModel>
    {
        public EditForumPostValidator(ILocalizationService localizationService)
        {            
            RuleFor(x => x.Text).NotEmpty().WithMessage(localizationService.GetResource("Forum.TextCannotBeEmpty"));
        }
    }
}