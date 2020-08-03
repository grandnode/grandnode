using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Forums;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Forums
{
    public class ForumValidator : BaseGrandValidator<ForumModel>
    {
        public ForumValidator(
            IEnumerable<IValidatorConsumer<ForumModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.Name.Required"));
            RuleFor(x => x.ForumGroupId).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required"));
        }
    }
}