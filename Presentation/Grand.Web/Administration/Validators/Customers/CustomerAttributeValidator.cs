using FluentValidation;
using Grand.Admin.Models.Customers;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Customers
{
    public class CustomerAttributeValidator : BaseNopValidator<CustomerAttributeModel>
    {
        public CustomerAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Fields.Name.Required"));
        }
    }
}