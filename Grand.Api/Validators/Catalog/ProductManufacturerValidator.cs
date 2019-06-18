using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;

namespace Grand.Api.Validators.Catalog
{
    public class ProductManufacturerValidator : BaseGrandValidator<ProductManufacturerDto>
    {
        public ProductManufacturerValidator(ILocalizationService localizationService, IManufacturerService manufacturerService)
        {
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                var manufacturer = await manufacturerService.GetManufacturerById(x.ManufacturerId);
                if (manufacturer == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductManufacturer.Fields.ManufacturerId.NotExists"));
        }
    }
}
