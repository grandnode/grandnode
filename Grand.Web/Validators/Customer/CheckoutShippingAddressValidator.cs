using Grand.Core.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Checkout;
using Grand.Web.Validators.Common;

namespace Grand.Web.Validators.Customer
{
    public class CheckoutShippingAddressValidator : BaseGrandValidator<CheckoutShippingAddressModel>
    {
        public CheckoutShippingAddressValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings)
        {
            RuleFor(x => x.NewAddress).SetValidator(new AddressValidator(localizationService, stateProvinceService, addressSettings));
        }
    }
}
