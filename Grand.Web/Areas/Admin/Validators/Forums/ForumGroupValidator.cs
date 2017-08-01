using FluentValidation;
using Grand.Web.Areas.Admin.Models.Forums;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Forums
{
    public class ForumGroupValidator : BaseGrandValidator<ForumGroupModel>
    {
        public ForumGroupValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required"));
        }
    }
}