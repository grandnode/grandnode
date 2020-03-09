﻿using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Validators.Catalog
{
    public class ProductReviewsValidator : BaseGrandValidator<ProductReviewsModel>
    {
        public ProductReviewsValidator(
            IEnumerable<IValidatorConsumer<ProductReviewsModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.AddProductReview.Title).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.Title.Required")).When(x => x.AddProductReview != null);
            RuleFor(x => x.AddProductReview.Title).Length(1, 200).WithMessage(string.Format(localizationService.GetResource("Reviews.Fields.Title.MaxLengthValidation"), 200)).When(x => x.AddProductReview != null && !string.IsNullOrEmpty(x.AddProductReview.Title));
            RuleFor(x => x.AddProductReview.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.ReviewText.Required")).When(x => x.AddProductReview != null);
        }
    }
}