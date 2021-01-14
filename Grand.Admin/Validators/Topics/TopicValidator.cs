using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Topics;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Topics
{
    public class TopicValidator : BaseGrandValidator<TopicModel>
    {
        public TopicValidator(
            IEnumerable<IValidatorConsumer<TopicModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.SystemName).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Topics.Fields.SystemName.Required"));
        }
    }
}