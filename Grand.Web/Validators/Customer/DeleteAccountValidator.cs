using FluentValidation;
using Grand.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Customer;
using System.Collections.Generic;

namespace Grand.Web.Validators.Customer
{
    public class DeleteAccountValidator : BaseGrandValidator<DeleteAccountModel>
    {
        public DeleteAccountValidator(
            IEnumerable<IValidatorConsumer<DeleteAccountModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.DeleteAccount.Fields.Password.Required"));
        }}
}