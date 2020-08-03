using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class UserApiValidator : BaseGrandValidator<UserApiModel>
    {
        public UserApiValidator(
            IEnumerable<IValidatorConsumer<UserApiModel>> validators,
            ILocalizationService localizationService, ICustomerService customerService)
            : base(validators)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.UserApi.Email.Required"));
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.Email))
                {
                    var customer = await customerService.GetCustomerByEmail(x.Email.ToLowerInvariant());
                    if (customer != null && customer.Active && !customer.IsSystemAccount)
                        return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Admin.System.UserApi.Email.CustomerNotExist")); ;
        }
    }
}