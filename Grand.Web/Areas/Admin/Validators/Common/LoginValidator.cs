using FluentValidation;
using Grand.Domain.Customers;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Common
{
    public class LoginValidator : BaseGrandValidator<LoginModel>
    {
        public LoginValidator(
            IEnumerable<IValidatorConsumer<LoginModel>> validators,
            ILocalizationService localizationService, CustomerSettings customerSettings)
            : base(validators)
        {
            if (!customerSettings.UsernamesEnabled)
            {
                //login by email
                RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Email.Required"));
                RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            }
        }
    }
}