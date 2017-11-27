using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Vendors;
using Grand.Web.Validators.Common;
using Grand.Core.Domain.Vendors;
using Grand.Services.Directory;

namespace Grand.Web.Validators.Vendors
{
    public class ApplyVendorValidator : BaseGrandValidator<ApplyVendorModel>
    {
        public ApplyVendorValidator(ILocalizationService localizationService, IStateProvinceService stateProvinceService,
            VendorSettings addressSettings)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Vendors.ApplyAccount.Name.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Vendors.ApplyAccount.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Address).SetValidator(new VendorAddressValidator(localizationService, stateProvinceService, addressSettings));
        }
    }
}