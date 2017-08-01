using FluentValidation;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerActionConditionValidator : BaseGrandValidator<CustomerActionConditionModel>
    {
        public CustomerActionConditionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerActionCondition.Fields.Name.Required"));
        }
    }
}