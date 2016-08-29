using FluentValidation;
using Grand.Admin.Models.Messages;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Messages
{
    public class EmailAccountValidator : BaseNopValidator<EmailAccountModel>
    {
        public EmailAccountValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            
            RuleFor(x => x.DisplayName).NotEmpty();
        }
    }
}