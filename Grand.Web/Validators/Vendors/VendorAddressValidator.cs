using FluentValidation;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using System;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Vendors;

namespace Grand.Web.Validators.Common
{
    public class VendorAddressValidator : BaseGrandValidator<VendorAddressModel>
    {
        public VendorAddressValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            VendorSettings addressSettings)
        {
            if (addressSettings.CountryEnabled)
            {
                RuleFor(x => x.CountryId)
                    .NotNull()
                    .WithMessage(localizationService.GetResource("Vendor.Account.Fields.Country.Required"));
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Vendor.Account.Fields.Country.Required"));
            }
            if (addressSettings.CountryEnabled && addressSettings.StateProvinceEnabled)
            {
                RuleFor(x => x.StateProvinceId).Must((x, context) =>
                {
                    //does selected country has states?
                    var countryId = !String.IsNullOrEmpty(x.CountryId) ? x.CountryId : "";
                    var hasStates = stateProvinceService.GetStateProvincesByCountryId(countryId).Count > 0;
                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (String.IsNullOrEmpty(x.StateProvinceId))
                        {
                            return false;
                        }
                    }
                    return true;
                }).WithMessage(localizationService.GetResource("Vendor.Account.Fields.StateProvince.Required"));
            }
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.Company.Required"));
            }
            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.StreetAddress.Required"));
            }
            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.Address2).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.StreetAddress2.Required"));
            }
            if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.ZipPostalCode.Required"));
            }
            if (addressSettings.CityRequired && addressSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.City.Required"));
            }
            if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.Phone.Required"));
            }
            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.FaxNumber).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Account.Fields.Fax.Required"));
            }
        }
    }
}