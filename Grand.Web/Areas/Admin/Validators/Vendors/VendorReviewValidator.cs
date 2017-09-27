using FluentValidation;
using Grand.Web.Areas.Admin.Models.Vendors;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Vendors
{
    public class VendorReviewValidator : BaseGrandValidator<VendorReviewModel>
    {
        public VendorReviewValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.VendorReviews.Fields.Title.Required"));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Admin.VendorReviews.Fields.ReviewText.Required"));
        }
    }
}
