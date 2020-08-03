using FluentValidation;
using Grand.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Customers
{
    public class CustomerValidator : BaseGrandValidator<CustomerModel>
    {
        public CustomerValidator(
            IEnumerable<IValidatorConsumer<CustomerModel>> validators,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            CustomerSettings customerSettings)
            : base(validators)
        {
            //customer email
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Email.Required"));
            
            //form fields
            if (customerSettings.CountryEnabled && customerSettings.CountryRequired)
            {
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Account.Fields.Country.Required"));
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
                }).WithMessage(localizationService.GetResource("Account.Fields.StateProvince.Required"));
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Company.Required"));
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled) 
                RuleFor(x => x.StreetAddress).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StreetAddress.Required"));
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
                RuleFor(x => x.StreetAddress2).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StreetAddress2.Required"));
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.ZipPostalCode.Required"));
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.City.Required"));
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
                RuleFor(x => x.Phone).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Phone.Required"));
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled) 
                RuleFor(x => x.Fax).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Fax.Required"));
        }
    }
}