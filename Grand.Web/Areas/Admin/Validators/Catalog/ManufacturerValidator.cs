using FluentValidation;
using Grand.Framework.Extensions;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ManufacturerValidator : BaseGrandValidator<ManufacturerModel>
    {
        public ManufacturerValidator(
            IEnumerable<IValidatorConsumer<ManufacturerModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(FluentValidationUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
        }
    }
} 
