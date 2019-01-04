using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Services.Media;

namespace Grand.Api.Validators.Catalog
{
    public class ProductPictureValidator : BaseGrandValidator<ProductPictureDto>
    {
        public ProductPictureValidator(ILocalizationService localizationService, IPictureService pictureService)
        {
            RuleFor(x => x).Must((x, context) =>
            {
                var picture = pictureService.GetPictureById(x.PictureId);
                if (picture == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductPicture.Fields.PictureId.NotExists"));
        }
    }
}
