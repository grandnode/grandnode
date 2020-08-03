using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Polls;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Polls
{
    public class PollValidator : BaseGrandValidator<PollModel>
    {
        public PollValidator(
            IEnumerable<IValidatorConsumer<PollModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Polls.Fields.Name.Required"));
        }
    }
}