using FluentValidation;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;
using Grand.Web.Models.Catalog;

namespace Grand.Web.Validators.Catalog
{
    public class ProductAskQuestionValidator : BaseNopValidator<ProductAskQuestionModel>
    {
        public ProductAskQuestionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}