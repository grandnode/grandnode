using FluentValidation;
using Grand.Api.DTOs.Customers;
using Grand.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Api.Validators.Customers
{
    public class CustomerValidator : BaseGrandValidator<CustomerDto>
    {
        public CustomerValidator(
            IEnumerable<IValidatorConsumer<CustomerDto>> validators,
            ILocalizationService localizationService, IStateProvinceService stateProvinceService, 
            ICustomerService customerService, IStoreService storeService, CustomerSettings customerSettings)
            : base(validators)
        {

            RuleFor(x => x).MustAsync(async (x, context) =>
            {
                var customer = await customerService.GetCustomerByEmail(x.Email);
                if (customer != null && customer.Id != x.Id)
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.Email.Registered"));

            RuleFor(x => x).MustAsync(async (x, context) =>
            {
                var username = await customerService.GetCustomerByUsername(x.Username);
                if (username != null && username.Id != x.Id && customerSettings.UsernamesEnabled)
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.Username.Registered"));

            RuleFor(x => x).MustAsync(async (x, context) =>
            {
                var customer = await customerService.GetCustomerByGuid(x.CustomerGuid);
                if (customer != null && customer.Id != x.Id)
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.Guid.Exists"));

            RuleFor(x => x).MustAsync(async (x, context) =>
            {
                var store = await storeService.GetStoreById(x.StoreId);
                if (store != null)
                {
                    return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.StoreId.NotExists"));


            //form fields
            if (customerSettings.CountryEnabled && customerSettings.CountryRequired)
            {
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.Country.Required"));
            }
            if (customerSettings.CountryEnabled &&
                customerSettings.StateProvinceEnabled &&
                customerSettings.StateProvinceRequired)
            {

                RuleFor(x => x.StateProvinceId).MustAsync(async (x, y, context) =>
                {
                    //does selected country has states?
                    var countryId = !string.IsNullOrEmpty(x.CountryId) ? x.CountryId : "";
                    var hasStates = (await stateProvinceService.GetStateProvincesByCountryId(countryId)).Count > 0;
                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (string.IsNullOrEmpty(y))
                        {
                            return false;
                        }
                    }
                    return true;
                }).WithMessage(localizationService.GetResource("pi.Customers.Customer.Fields.StateProvince.Required"));
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Company.Required"));
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
                RuleFor(x => x.StreetAddress).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.StreetAddress.Required"));
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
                RuleFor(x => x.StreetAddress2).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.StreetAddress2.Required"));
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.ZipPostalCode.Required"));
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.City.Required"));
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
                RuleFor(x => x.Phone).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Phone.Required"));
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
                RuleFor(x => x.Fax).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Fax.Required"));
        }
    }
}
