using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Directory;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class StateProvinceValidator : BaseGrandValidator<StateProvinceModel>
    {
        public StateProvinceValidator(
            IEnumerable<IValidatorConsumer<StateProvinceModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Countries.States.Fields.Name.Required"));
        }
    }
}