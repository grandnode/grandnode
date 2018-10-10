using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Catalog;

namespace Grand.Web.Validators.Catalog
{
    public class ProductAskQuestionValidator : BaseGrandValidator<ProductAskQuestionModel>
    {
        public ProductAskQuestionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Message).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.Message.Required"));
            RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.FullName.Required"));
        }
    }
}