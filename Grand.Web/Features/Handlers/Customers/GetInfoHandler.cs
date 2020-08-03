using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Tax;
using Grand.Services.Authentication.External;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using Grand.Web.Models.Newsletter;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetInfoHandler : IRequestHandler<GetInfo, CustomerInfoModel>
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IMediator _mediator;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ForumSettings _forumSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;

        public GetInfoHandler(
            IDateTimeHelper dateTimeHelper,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INewsletterCategoryService newsletterCategoryService,
            ILocalizationService localizationService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IExternalAuthenticationService externalAuthenticationService,
            IMediator mediator,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            ForumSettings forumSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _newsletterCategoryService = newsletterCategoryService;
            _localizationService = localizationService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _externalAuthenticationService = externalAuthenticationService;
            _mediator = mediator;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _forumSettings = forumSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
        }

        public async Task<CustomerInfoModel> Handle(GetInfo request, CancellationToken cancellationToken)
        {
            var model = new CustomerInfoModel();
            if (request.Model != null)
                model = request.Model;

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if (model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (request.ExcludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!request.ExcludeProperties)
            {
                PrepareNotExludeModel(model, request);
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = request.Customer.Username;
            }

            //newsletter
            await PrepareNewsletter(model, request);

            //settings
            await PrepareModelSettings(model, request);

            //external authentication
            await PrepareExternalAuth(model, request);

            //custom customer attributes
            var customAttributes = await _mediator.Send(new GetCustomAttributes() {
                Customer = request.Customer,
                Language = request.Language,
                OverrideAttributesXml = request.OverrideCustomCustomerAttributesXml
            });
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            return model;
        }

        private void PrepareNotExludeModel(CustomerInfoModel model, GetInfo request)
        {
            model.VatNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber);
            model.FirstName = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName);
            model.LastName = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName);
            model.Gender = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender);
            var dateOfBirth = request.Customer.GetAttributeFromEntity<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
            if (dateOfBirth.HasValue)
            {
                model.DateOfBirthDay = dateOfBirth.Value.Day;
                model.DateOfBirthMonth = dateOfBirth.Value.Month;
                model.DateOfBirthYear = dateOfBirth.Value.Year;
            }
            model.Company = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company);
            model.StreetAddress = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress);
            model.StreetAddress2 = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2);
            model.ZipPostalCode = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode);
            model.City = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City);
            model.CountryId = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
            model.StateProvinceId = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId);
            model.Phone = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
            model.Fax = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax);
            model.Signature = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature);
            model.Email = request.Customer.Email;
            model.Username = request.Customer.Username;
        }

        private async Task PrepareNewsletter(CustomerInfoModel model, GetInfo request)
        {
            //newsletter
            var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(request.Customer.Email, request.Store.Id);
            if (newsletter == null)
                newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByCustomerId(request.Customer.Id);

            model.Newsletter = newsletter != null && newsletter.Active;

            var categories = (await _newsletterCategoryService.GetAllNewsletterCategory()).ToList();
            categories.ForEach(x => model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                Id = x.Id,
                Description = x.GetLocalized(y => y.Description, request.Language.Id),
                Name = x.GetLocalized(y => y.Name, request.Language.Id),
                Selected = newsletter == null ? false : newsletter.Categories.Contains(x.Id),
            }));
        }

        private async Task PrepareModelSettings(CustomerInfoModel model, GetInfo request)
        {
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

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)request.Customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId))
                .GetLocalizedEnum(_localizationService, request.Language.Id);
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
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;
            model.Is2faEnabled = request.Customer.GetAttributeFromEntity<bool>(SystemCustomerAttributeNames.TwoFactorEnabled);

        }

        private async Task PrepareExternalAuth(CustomerInfoModel model, GetInfo request)
        {
            model.NumberOfExternalAuthenticationProviders = _externalAuthenticationService
                          .LoadActiveExternalAuthenticationMethods(request.Customer, request.Store.Id).Count;
            foreach (var ear in await _externalAuthenticationService.GetExternalIdentifiersFor(request.Customer))
            {
                var authMethod = _externalAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalDisplayIdentifier,
                    AuthMethodName = authMethod.GetLocalizedFriendlyName(_localizationService, request.Language.Id)
                });
            }
        }
    }
}
