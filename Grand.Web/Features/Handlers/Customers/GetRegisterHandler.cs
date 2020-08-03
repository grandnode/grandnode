using Grand.Domain.Customers;
using Grand.Domain.Security;
using Grand.Domain.Tax;
using Grand.Framework.Security.Captcha;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using Grand.Web.Models.Newsletter;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetRegisterHandler : IRequestHandler<GetRegister, RegisterModel>
    {

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IMediator _mediator;

        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly CaptchaSettings _captchaSettings;

        public GetRegisterHandler(
            IDateTimeHelper dateTimeHelper,
            INewsletterCategoryService newsletterCategoryService,
            ILocalizationService localizationService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IMediator mediator,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            SecuritySettings securitySettings,
            CaptchaSettings captchaSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _newsletterCategoryService = newsletterCategoryService;
            _localizationService = localizationService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _mediator = mediator;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _securitySettings = securitySettings;
            _captchaSettings = captchaSettings;
        }

        public async Task<RegisterModel> Handle(GetRegister request, CancellationToken cancellationToken)
        {
            var model = new RegisterModel();
            if (request.Model != null)
                model = request.Model;
            else
                //enable newsletter by default
                model.Newsletter = _customerSettings.NewsletterTickedByDefault;

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if (model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (request.ExcludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });

                foreach (var c in await _countryService.GetAllCountries(request.Language.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem {
                        Text = c.GetLocalized(x => x.Name, request.Language.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, request.Language.Id);
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name, request.Language.Id), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = ""
                        });
                    }

                }
            }

            //custom customer attributes
            var customAttributes = await _mediator.Send(new GetCustomAttributes() {
                Customer = request.Customer,
                Language = request.Language,
                OverrideAttributesXml = request.OverrideCustomCustomerAttributesXml
            });
            foreach (var item in customAttributes)
            {
                model.CustomerAttributes.Add(item);
            }

            //newsletter categories
            var newsletterCategories = await _newsletterCategoryService.GetNewsletterCategoriesByStore(request.Store.Id);
            foreach (var item in newsletterCategories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name, request.Language.Id),
                    Description = item.GetLocalized(x => x.Description, request.Language.Id),
                    Selected = item.Selected
                });
            }
            return model;
        }
    }
}
