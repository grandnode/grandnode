using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Vendors;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Vendors
{
    public class VendorReviewValidator : BaseGrandValidator<VendorReviewModel>
    {
        public VendorReviewValidator(
            IEnumerable<IValidatorConsumer<VendorReviewModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.VendorReviews.Fields.Title.Required"));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Admin.VendorReviews.Fields.ReviewText.Required"));
        }
    }
}
