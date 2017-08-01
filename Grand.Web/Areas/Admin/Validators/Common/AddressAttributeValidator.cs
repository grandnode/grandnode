using FluentValidation;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Common
{
    public class AddressAttributeValidator : BaseGrandValidator<AddressAttributeModel>
    {
        public AddressAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.AddressAttributes.Fields.Name.Required"));
        }
    }
}