using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Forums;

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