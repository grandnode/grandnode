using FluentValidation;
using Grand.Api.DTOs.Customers;
using Grand.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;

namespace Grand.Api.Validators.Customers
{
    public class AddressValidator : BaseGrandValidator<AddressDto>
    {
        public AddressValidator(
            IEnumerable<IValidatorConsumer<AddressDto>> validators,
            ILocalizationService localizationService, IStateProvinceService stateProvinceService, AddressSettings addressSettings)
            : base(validators)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.FirstName.Required"));
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.LastName.Required"));
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Email.Required"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Api.Customers.Address.Common.WrongEmail"));
            if (addressSettings.CountryEnabled)
            {
                RuleFor(x => x.CountryId)
                    .NotNull()
                    .WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Country.Required"));
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Country.Required"));
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
                }).WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.StateProvince.Required"));
            }
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Company.Required"));
            }
            if (addressSettings.VatNumberRequired && addressSettings.VatNumberEnabled)
            {
                RuleFor(x => x.VatNumber).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.VatNumber.Required"));
            }
            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.StreetAddress.Required"));
            }
            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.Address2).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.StreetAddress2.Required"));
            }
            if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.ZipPostalCode.Required"));
            }
            if (addressSettings.CityRequired && addressSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.City.Required"));
            }
            if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Phone.Required"));
            }
            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.FaxNumber).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Address.Fields.Fax.Required"));
            }
        }
    }

}
