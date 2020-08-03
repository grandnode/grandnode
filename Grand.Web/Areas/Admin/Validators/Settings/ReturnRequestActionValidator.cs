using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Settings;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class ReturnRequestActionValidator : BaseGrandValidator<ReturnRequestActionModel>
    {
        public ReturnRequestActionValidator(
            IEnumerable<IValidatorConsumer<ReturnRequestActionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Name.Required"));
        }
    }
}