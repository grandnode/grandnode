using FluentValidation;
using Grand.Web.Areas.Admin.Models.Polls;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Polls
{
    public class PollValidator : BaseGrandValidator<PollModel>
    {
        public PollValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Polls.Fields.Name.Required"));
        }
    }
}