using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Forums;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Forums
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