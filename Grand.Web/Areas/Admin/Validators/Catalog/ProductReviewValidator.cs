using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Catalog
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