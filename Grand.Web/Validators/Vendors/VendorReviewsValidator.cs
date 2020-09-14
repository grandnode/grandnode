﻿using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Vendors;
using System.Collections.Generic;

namespace Grand.Web.Validators.Vendors
{
    public class VendorReviewsValidator : BaseGrandValidator<VendorReviewsModel>
    {
        public VendorReviewsValidator(
            IEnumerable<IValidatorConsumer<VendorReviewsModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.AddVendorReview.Title).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.Title.Required")).When(x => x.AddVendorReview != null);
            RuleFor(x => x.AddVendorReview.Title).Length(1, 200).WithMessage(string.Format(localizationService.GetResource("Reviews.Fields.Title.MaxLengthValidation"), 200)).When(x => x.AddVendorReview != null && !string.IsNullOrEmpty(x.AddVendorReview.Title));
            RuleFor(x => x.AddVendorReview.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.ReviewText.Required")).When(x => x.AddVendorReview != null);
        }
    }
}
