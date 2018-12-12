using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class UserApiValidator : BaseGrandValidator<UserApiModel>
    {
        public UserApiValidator(ILocalizationService localizationService, ICustomerService customerService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.UserApi.Email.Required"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Email))
                {
                    var customer = customerService.GetCustomerByEmail(x.Email.ToLowerInvariant());
                    if (customer != null && customer.Active && !customer.IsSystemAccount)
                        return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Admin.System.UserApi.Email.CustomerNotExist")); ;
        }
    }
}