using FluentValidation;
using Grand.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerActionValidator : BaseNopValidator<CustomerActionModel>
    {
        public CustomerActionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAction.Fields.Name.Required"));            
        }
    }
}