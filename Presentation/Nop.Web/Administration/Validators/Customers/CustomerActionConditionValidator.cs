using FluentValidation;
using Nop.Admin.Models.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Customers
{
    public class CustomerActionConditionValidator : BaseNopValidator<CustomerActionConditionModel>
    {
        public CustomerActionConditionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerActionCondition.Fields.Name.Required"));
        }
    }
}