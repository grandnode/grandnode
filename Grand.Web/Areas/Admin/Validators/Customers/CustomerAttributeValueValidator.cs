using FluentValidation;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerAttributeValueValidator : BaseGrandValidator<CustomerAttributeValueModel>
    {
        public CustomerAttributeValueValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Values.Fields.Name.Required"));
        }
    }
}