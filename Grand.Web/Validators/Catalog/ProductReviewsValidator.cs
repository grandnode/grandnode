using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Validators.Catalog
{
    public class ProductReviewsValidator : BaseGrandValidator<AddProductReviewModel>
    {
        public ProductReviewsValidator(
            IEnumerable<IValidatorConsumer<AddProductReviewModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.Title.Required"));
            RuleFor(x => x.Title).Length(1, 200).WithMessage(string.Format(localizationService.GetResource("Reviews.Fields.Title.MaxLengthValidation"), 200));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.ReviewText.Required"));
        }
    }
}