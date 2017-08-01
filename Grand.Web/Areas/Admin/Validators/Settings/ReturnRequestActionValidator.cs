using FluentValidation;
using Grand.Web.Areas.Admin.Models.Settings;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class ReturnRequestActionValidator : BaseGrandValidator<ReturnRequestActionModel>
    {
        public ReturnRequestActionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.Order.ReturnRequestActions.Name.Required"));
        }
    }
}