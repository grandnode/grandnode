using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Extensions;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Media;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ManufacturerValidator : BaseGrandValidator<ManufacturerDto>
    {
        public ManufacturerValidator(IEnumerable<IValidatorConsumer<ManufacturerDto>> validators, 
            ILocalizationService localizationService, IPictureService pictureService, IManufacturerService manufacturerService, IManufacturerTemplateService manufacturerTemplateService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Catalog.Manufacturer.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(FluentValidationUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Api.Catalog.Manufacturer.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.PictureId))
                {
                    var picture = await pictureService.GetPictureById(x.PictureId);
                    if (picture == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Manufacturer.Fields.PictureId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.ManufacturerTemplateId))
                {
                    var template = await manufacturerTemplateService.GetManufacturerTemplateById(x.ManufacturerTemplateId);
                    if (template == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Manufacturer.Fields.ManufacturerTemplateId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var manufacturer = await manufacturerService.GetManufacturerById(x.Id);
                    if (manufacturer == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Manufacturer.Fields.Id.NotExists"));
        }
    }
}
