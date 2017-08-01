using FluentValidation;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class StateProvinceValidator : BaseGrandValidator<StateProvinceModel>
    {
        public StateProvinceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Countries.States.Fields.Name.Required"));
        }
    }
}