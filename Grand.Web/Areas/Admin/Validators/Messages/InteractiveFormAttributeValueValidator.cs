using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class InteractiveFormAttributeValueValidator : BaseGrandValidator<InteractiveFormAttributeValueModel>
    {
        public InteractiveFormAttributeValueValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.Name.Required"));
        }
    }
}