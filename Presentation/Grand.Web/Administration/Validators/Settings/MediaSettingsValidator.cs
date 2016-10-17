using FluentValidation;
using Grand.Web.Framework.Validators;
using Grand.Admin.Models.Settings;
using Grand.Services.Localization;

namespace Grand.Admin.Validators.Settings
{
    public class MediaSettingsValidator : BaseNopValidator<MediaSettingsModel>
    {
        public MediaSettingsValidator(ILocalizationService localizationService)
        {
            //Watermark Text
            RuleFor(x => x.WatermarkPositionXPercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkPositionYPercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkFontSizePercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkOpacityPercent).InclusiveBetween(1, 100);

            //Watermark Misc Options
            RuleFor(x => x.WatermarkForPicturesAboveSize).GreaterThan(1);

            //Watermark Overlay
            RuleFor(x => x.WatermarkOverlayPositionXPercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkOverlayPositionYPercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkOverlaySizePercent).InclusiveBetween(1, 100);
            RuleFor(x => x.WatermarkOverlayOpacityPercent).InclusiveBetween(1, 100);
        }
    }
}