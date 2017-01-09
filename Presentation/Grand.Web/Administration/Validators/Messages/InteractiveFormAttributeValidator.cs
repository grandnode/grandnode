using FluentValidation;
using Grand.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Messages
{
    public class InteractiveFormAttributeValidator : BaseNopValidator<InteractiveFormAttributeModel>
    {
        public InteractiveFormAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Fields.Name.Required"));
            RuleFor(x => x.SystemName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Fields.SystemName.Required"));
        }
    }
}