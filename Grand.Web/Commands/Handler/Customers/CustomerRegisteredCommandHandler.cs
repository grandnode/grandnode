using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Messages;
using Grand.Domain.Tax;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Messages;
using Grand.Services.Tax;
using Grand.Web.Commands.Models.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Customers
{
    public class CustomerRegisteredCommandHandler : IRequestHandler<CustomerRegisteredCommand, bool>
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IVatService _checkVatService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActionEventService _customerActionEventService;

        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly LocalizationSettings _localizationSettings;

        public CustomerRegisteredCommandHandler(
            IGenericAttributeService genericAttributeService,
            IVatService checkVatService, 
            IWorkflowMessageService workflowMessageService, 
            INewsLetterSubscriptionService newsLetterSubscriptionService, 
            IAddressService addressService, 
            ICustomerService customerService, 
            ICustomerActionEventService customerActionEventService, 
            DateTimeSettings dateTimeSettings, 
            TaxSettings taxSettings, 
            CustomerSettings customerSettings, 
            LocalizationSettings localizationSettings)
        {
            _genericAttributeService = genericAttributeService;
            _checkVatService = checkVatService;
            _workflowMessageService = workflowMessageService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressService = addressService;
            _customerService = customerService;
            _customerActionEventService = customerActionEventService;
            _dateTimeSettings = dateTimeSettings;
            _taxSettings = taxSettings;
            _customerSettings = customerSettings;
            _localizationSettings = localizationSettings;
        }

        public async Task<bool> Handle(CustomerRegisteredCommand request, CancellationToken cancellationToken)
        {

            //properties
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
            {
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.TimeZoneId, request.Model.TimeZoneId);
            }
            //VAT number
            if (_taxSettings.EuVatEnabled)
            {
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.VatNumber, request.Model.VatNumber);

                var vat = await _checkVatService.GetVatNumberStatus(request.Model.VatNumber);

                await _genericAttributeService.SaveAttribute(request.Customer,
                    SystemCustomerAttributeNames.VatNumberStatusId,
                    (int)vat.status);

                //send VAT number admin notification
                if (!String.IsNullOrEmpty(request.Model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                    await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(request.Customer, request.Store, request.Model.VatNumber, vat.address, _localizationSettings.DefaultAdminLanguageId);

            }

            //form fields
            if (_customerSettings.GenderEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.Gender, request.Model.Gender);
            await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.FirstName, request.Model.FirstName);
            await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.LastName, request.Model.LastName);
            if (_customerSettings.DateOfBirthEnabled)
            {
                DateTime? dateOfBirth = request.Model.ParseDateOfBirth();
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
            }
            if (_customerSettings.CompanyEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.Company, request.Model.Company);
            if (_customerSettings.StreetAddressEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.StreetAddress, request.Model.StreetAddress);
            if (_customerSettings.StreetAddress2Enabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.StreetAddress2, request.Model.StreetAddress2);
            if (_customerSettings.ZipPostalCodeEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.ZipPostalCode, request.Model.ZipPostalCode);
            if (_customerSettings.CityEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.City, request.Model.City);
            if (_customerSettings.CountryEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.CountryId, request.Model.CountryId);
            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.StateProvinceId, request.Model.StateProvinceId);
            if (_customerSettings.PhoneEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.Phone, request.Model.Phone);
            if (_customerSettings.FaxEnabled)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.Fax, request.Model.Fax);

            //newsletter
            if (_customerSettings.NewsletterEnabled)
            {
                var categories = new List<string>();
                foreach (string formKey in request.Form.Keys)
                {
                    if (formKey.Contains("customernewsletterCategory_"))
                    {
                        try
                        {
                            var category = formKey.Split('_')[1];
                            categories.Add(category);
                        }
                        catch { }
                    }
                }

                //save newsletter value
                var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(request.Model.Email, request.Store.Id);
                if (newsletter != null)
                {
                    newsletter.Categories.Clear();
                    categories.ForEach(x => newsletter.Categories.Add(x));
                    if (request.Model.Newsletter)
                    {
                        newsletter.Active = true;
                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                    }
                }
                else
                {
                    if (request.Model.Newsletter)
                    {
                        var newsLetterSubscription = new NewsLetterSubscription {
                            NewsLetterSubscriptionGuid = Guid.NewGuid(),
                            Email = request.Model.Email,
                            CustomerId = request.Customer.Id,
                            Active = true,
                            StoreId = request.Store.Id,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        categories.ForEach(x => newsLetterSubscription.Categories.Add(x));
                        await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription);
                    }
                }
            }

            //save customer attributes
            await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.CustomCustomerAttributes, request.CustomerAttributesXml);

            //insert default address (if possible)
            var defaultAddress = new Address {
                FirstName = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName),
                LastName = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName),
                Email = request.Customer.Email,
                Company = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company),
                VatNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber),
                CountryId = !string.IsNullOrEmpty(request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId)) ?
                            request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId) : "",
                StateProvinceId = !string.IsNullOrEmpty(request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId)) ?
                    request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId) : "",
                City = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City),
                Address1 = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress),
                Address2 = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2),
                ZipPostalCode = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode),
                PhoneNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone),
                FaxNumber = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax),
                CreatedOnUtc = request.Customer.CreatedOnUtc,
            };

            if (await _addressService.IsAddressValid(defaultAddress))
            {
                //set default address
                defaultAddress.CustomerId = request.Customer.Id;
                request.Customer.Addresses.Add(defaultAddress);
                await _customerService.InsertAddress(defaultAddress);
                request.Customer.BillingAddress = defaultAddress;
                await _customerService.UpdateBillingAddress(defaultAddress);
                request.Customer.ShippingAddress = defaultAddress;
                await _customerService.UpdateShippingAddress(defaultAddress);
            }

            //notifications
            if (_customerSettings.NotifyNewCustomerRegistration)
                await _workflowMessageService.SendCustomerRegisteredNotificationMessage(request.Customer, request.Store, _localizationSettings.DefaultAdminLanguageId);

            //New customer has a free shipping for the first order
            if (_customerSettings.RegistrationFreeShipping)
                await _customerService.UpdateFreeShipping(request.Customer.Id, true);

            await _customerActionEventService.Registration(request.Customer);

            return true;
        }
    }
}
