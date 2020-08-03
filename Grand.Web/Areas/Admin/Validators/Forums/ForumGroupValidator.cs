using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Forums;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Forums
{
    public class ForumGroupValidator : BaseGrandValidator<ForumGroupModel>
    {
        public ForumGroupValidator(
            IEnumerable<IValidatorConsumer<ForumGroupModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required"));
        }
    }
}