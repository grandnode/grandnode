using FluentValidation;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerAttributeValidator : BaseGrandValidator<CustomerAttributeModel>
    {
        public CustomerAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Fields.Name.Required"));
        }
    }
}