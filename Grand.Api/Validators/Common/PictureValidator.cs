using FluentValidation;
using Grand.Api.DTOs.Common;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Api.Validators.Common
{
    public class PictureValidator : BaseGrandValidator<PictureDto>
    {
        public PictureValidator(
            IEnumerable<IValidatorConsumer<PictureDto>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.PictureBinary).NotEmpty().WithMessage(localizationService.GetResource("Api.Common.Picture.Fields.PictureBinary.Required"));
            RuleFor(x => x.MimeType).NotEmpty().WithMessage(localizationService.GetResource("Api.Common.Picture.Fields.MimeType.Required"));
            RuleFor(x => x.Id).Empty().WithMessage(localizationService.GetResource("Api.Common.Picture.Fields.Id.NotRequired"));
        }
    }
}
