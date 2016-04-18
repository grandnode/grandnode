using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Validators.Catalog
{
    public class ProductAskQuestionValidator : BaseNopValidator<ProductAskQuestionModel>
    {
        public ProductAskQuestionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}