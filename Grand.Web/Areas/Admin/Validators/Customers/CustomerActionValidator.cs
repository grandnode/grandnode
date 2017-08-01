using FluentValidation;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerActionValidator : BaseGrandValidator<CustomerActionModel>
    {
        public CustomerActionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAction.Fields.Name.Required"));            
        }
    }
}