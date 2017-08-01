using FluentValidation;
using Grand.Web.Areas.Admin.Models.Templates;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Templates
{
    public class TopicTemplateValidator : BaseGrandValidator<TopicTemplateModel>
    {
        public TopicTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Topic.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Topic.ViewPath.Required"));
        }
    }
}