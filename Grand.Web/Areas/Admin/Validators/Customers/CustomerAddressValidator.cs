using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Validators.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerAddressValidator : BaseGrandValidator<CustomerAddressModel>
    {
        public CustomerAddressValidator(
            IEnumerable<IValidatorConsumer<CustomerAddressModel>> validators,
            IEnumerable<IValidatorConsumer<Models.Common.AddressModel>> addressvalidators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Address).SetValidator(new AddressValidator(addressvalidators, localizationService));
        }
    }
}
