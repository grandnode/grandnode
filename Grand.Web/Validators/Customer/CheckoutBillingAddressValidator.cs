using Grand.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Checkout;
using Grand.Web.Validators.Common;
using System.Collections.Generic;

namespace Grand.Web.Validators.Customer
{
    public class CheckoutBillingAddressValidator : BaseGrandValidator<CheckoutBillingAddressModel>
    {
        public CheckoutBillingAddressValidator(
            IEnumerable<IValidatorConsumer<CheckoutBillingAddressModel>> validators,
            IEnumerable<IValidatorConsumer<Models.Common.AddressModel>> addressvalidators,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings)
            : base(validators)
        {
            RuleFor(x => x.NewAddress).SetValidator(new AddressValidator(addressvalidators, localizationService, stateProvinceService, addressSettings));
        }
    }
}
