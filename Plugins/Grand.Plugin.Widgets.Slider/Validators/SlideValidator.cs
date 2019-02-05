using FluentValidation;
using Grand.Framework.Validators;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Localization;

namespace Grand.Plugin.Widgets.Slider.Validators
{
    public class SliderValidator : BaseGrandValidator<SlideModel>
    {
        public SliderValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Plugins.Widgets.Slider.Name.Required"));
            RuleFor(x => x.SliderTypeId == (int)SliderType.Category && string.IsNullOrEmpty(x.CategoryId)).Equal(false).WithMessage(localizationService.GetResource("Plugins.Widgets.Slider.Category.Required"));
            RuleFor(x => x.SliderTypeId == (int)SliderType.Manufacturer && string.IsNullOrEmpty(x.ManufacturerId)).Equal(false).WithMessage(localizationService.GetResource("Plugins.Widgets.Slider.Manufacturer.Required"));
        }
    }
}