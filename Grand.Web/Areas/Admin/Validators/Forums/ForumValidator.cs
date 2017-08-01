using FluentValidation;
using Grand.Web.Areas.Admin.Models.Forums;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Forums
{
    public class ForumValidator : BaseGrandValidator<ForumModel>
    {
        public ForumValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.Name.Required"));
            RuleFor(x => x.ForumGroupId).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required"));
        }
    }
}