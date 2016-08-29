using FluentValidation;
using Grand.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Directory
{
    public class StateProvinceValidator : BaseNopValidator<StateProvinceModel>
    {
        public StateProvinceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Countries.States.Fields.Name.Required"));
        }
    }
}