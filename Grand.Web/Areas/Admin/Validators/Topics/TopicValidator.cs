using FluentValidation;
using Grand.Web.Areas.Admin.Models.Topics;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Topics
{
    public class TopicValidator : BaseGrandValidator<TopicModel>
    {
        public TopicValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SystemName).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Topics.Fields.SystemName.Required"));
        }
    }
}