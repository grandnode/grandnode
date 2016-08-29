using FluentValidation;
using Grand.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Catalog
{
    public class SpecificationAttributeOptionValidator : BaseNopValidator<SpecificationAttributeOptionModel>
    {
        public SpecificationAttributeOptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name.Required"));
        }
    }
}