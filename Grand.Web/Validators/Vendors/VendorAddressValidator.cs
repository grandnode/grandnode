using FluentValidation;
using Grand.Domain.Vendors;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Vendors;
using System;
using System.Collections.Generic;

namespace Grand.Web.Validators.Common
{
    public class VendorAddressValidator : BaseGrandValidator<VendorAddressModel>
    {
        public VendorAddressValidator(
            IEnumerable<IValidatorConsumer<VendorAddressModel>> validators,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            VendorSettings addressSettings)
            : base(validators)
        {
            if (addressSettings.CountryEnabled)
            {
                RuleFor(x => x.CountryId)
                    .NotNull()
                    .WithMessage(localizationService.GetResource("Account.VendorInfo.Country.Required"));
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Account.VendorInfo.Country.Required"));
            }
            if (addressSettings.CountryEnabled && addressSettings.StateProvinceEnabled)
            {
                RuleFor(x => x.StateProvinceId).MustAsync(async (x, y, context) =>
                {
                    //does selected country has states?
                    var countryId = !String.IsNullOrEmpty(x.CountryId) ? x.CountryId : "";
                    var hasStates = (await stateProvinceService.GetStateProvincesByCountryId(countryId)).Count > 0;
                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (String.IsNullOrEmpty(y))
                        {
                            return false;
                        }
                    }
                    return true;
                }).WithMessage(localizationService.GetResource("Account.VendorInfo.StateProvince.Required"));
            }
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Company.Required"));
            }
            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Address1.Required"));
            }
            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.Address2).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Address2.Required"));
            }
            if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.ZipPostalCode.Required"));
            }
            if (addressSettings.CityRequired && addressSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.City.Required"));
            }
            if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.phonenumber.Required"));
            }
            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.FaxNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.faxnumber.Required"));
            }
        }
    }
}