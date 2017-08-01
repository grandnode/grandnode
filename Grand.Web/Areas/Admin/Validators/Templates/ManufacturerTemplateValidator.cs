using FluentValidation;
using Grand.Web.Areas.Admin.Models.Templates;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Templates
{
    public class ManufacturerTemplateValidator : BaseGrandValidator<ManufacturerTemplateModel>
    {
        public ManufacturerTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Manufacturer.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Manufacturer.ViewPath.Required"));
        }
    }
}