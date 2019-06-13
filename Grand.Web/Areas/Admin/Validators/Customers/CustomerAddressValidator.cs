using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Validators.Common;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerAddressValidator : BaseGrandValidator<CustomerAddressModel>
    {
        public CustomerAddressValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Address).SetValidator(new AddressValidator(localizationService));
        }
    }
}
