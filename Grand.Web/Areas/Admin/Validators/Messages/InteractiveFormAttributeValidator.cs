using FluentValidation;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class InteractiveFormAttributeValidator : BaseGrandValidator<InteractiveFormAttributeModel>
    {
        public InteractiveFormAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Fields.Name.Required"));
            RuleFor(x => x.SystemName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Fields.SystemName.Required"));
        }
    }
}