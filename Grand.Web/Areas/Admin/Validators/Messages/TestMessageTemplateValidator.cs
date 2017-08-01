using FluentValidation;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Messages
{
    public class TestMessageTemplateValidator : BaseGrandValidator<TestMessageTemplateModel>
    {
        public TestMessageTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SendTo).NotEmpty();
            RuleFor(x => x.SendTo).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
        }
    }
}