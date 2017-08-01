using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductReviewValidator : BaseGrandValidator<ProductReviewModel>
    {
        public ProductReviewValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.Title.Required"));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.ReviewText.Required"));
        }}
}