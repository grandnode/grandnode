using FluentValidation;
using Grand.Core.Domain.Customers;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Customer;

namespace Grand.Web.Validators.Customer
{
    public class DeleteAccountValidator : BaseGrandValidator<DeleteAccountModel>
    {
        public DeleteAccountValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.DeleteAccount.Fields.Password.Required"));
        }}
}