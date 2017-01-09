using FluentValidation;
using Grand.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Messages
{
    public class InteractiveFormAttributeValueValidator : BaseNopValidator<InteractiveFormAttributeValueModel>
    {
        public InteractiveFormAttributeValueValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.Name.Required"));
        }
    }
}