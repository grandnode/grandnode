using FluentValidation;
using Grand.Domain.Vendors;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Vendors;
using Grand.Web.Validators.Common;
using System.Collections.Generic;

namespace Grand.Web.Validators.Vendors
{
    public class VendorInfoValidator : BaseGrandValidator<VendorInfoModel>
    {
        public VendorInfoValidator(
            IEnumerable<IValidatorConsumer<VendorInfoModel>> validators,
            IEnumerable<IValidatorConsumer<VendorAddressModel>> addressvalidators,
            ILocalizationService localizationService, 
            IStateProvinceService stateProvinceService, VendorSettings addressSettings)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Name.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Address).SetValidator(new VendorAddressValidator(addressvalidators, localizationService, stateProvinceService, addressSettings));
        }
    }
}