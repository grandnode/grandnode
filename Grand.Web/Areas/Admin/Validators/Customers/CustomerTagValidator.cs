using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerTagValidator : BaseGrandValidator<CustomerTagModel>
    {
        public CustomerTagValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerTags.Fields.Name.Required"));
        }
    }
}