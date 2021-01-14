using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Catalog
{
    public class ProductReviewValidator : BaseGrandValidator<ProductReviewModel>
    {
        public ProductReviewValidator(
            IEnumerable<IValidatorConsumer<ProductReviewModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.Title.Required"));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.ReviewText.Required"));
        }}
}