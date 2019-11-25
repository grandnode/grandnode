using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Common
{
    public class ReviewValidator : BaseGrandValidator<ReviewModel>
    {
        public ReviewValidator(
            IEnumerable<IValidatorConsumer<ReviewModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Review.Fields.Title.Required"));
            RuleFor(x => x.ReviewText)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Review.Fields.ReviewText.Required"));
        }
    }
}