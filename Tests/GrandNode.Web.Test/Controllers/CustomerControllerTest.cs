using System;
using System.Collections.Generic;
using System.Text;
using Grand.Services.Authentication;
using Grand.Web.Interfaces;
using Grand.Core;
using Grand.Services.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;
using Grand.Services.Localization;
using Grand.Services.Common;
using Grand.Services.Tax;
using Grand.Services.Directory;
using Grand.Services.Messages;
using Grand.Services.Logging;
using MediatR;
using Grand.Framework.Security.Captcha;
using Grand.Core.Domain.Customers;
using Grand.Services.Helpers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Tax;
using Grand.Core.Configuration;
using Grand.Web.Controllers;
using Grand.Web.Models.Customer;

namespace GrandNode.Web.Test.Controllers
{
    [TestClass()]
    public class CustomerControllerTest
    {
        private CustomerController _controller;
        [TestInitialize]
        public void TestInitialize()
        {
            var customerViewModelServiceMock = new Mock<ICustomerViewModelService>();
            var grandAuthenticationServiceMock = new Mock<IGrandAuthenticationService>();
            var workContextMock = new Mock<IWorkContext>();
            var localizationServiceMock = new Mock<ILocalizationService>();
            var storeContextMock = new Mock<IStoreContext>();
            var customerServiceMock = new Mock<ICustomerService>();
            var customerAttributeParserMock = new Mock<ICustomerAttributeParser>();
            var genericAttributeServiceMock = new Mock<IGenericAttributeService>();
            var customerRegistrationServiceMock = new Mock<ICustomerRegistrationService>();
            var taxServiceMock = new Mock<ITaxService>();
            var countryServiceMock = new Mock<ICountryService>();
            var newsLetterSubscriptionServiceMock = new Mock<INewsLetterSubscriptionService>();
            var customerActivityServiceMock = new Mock<ICustomerActivityService>();
            var addressViewModelServiceMock = new Mock<IAddressViewModelService>();
            var mediatorMock = new Mock<IMediator>();
            var workflowMessageServiceMock = new Mock<IWorkflowMessageService>();
            var captchaSettingsMock = new Mock<CaptchaSettings>();
            var customerSettingsMock = new Mock<CustomerSettings>();
            var dateTimeSettingsMock = new Mock<DateTimeSettings>();
            var localizationSettingsMock = new Mock<LocalizationSettings>();
            var taxSettingsMock = new Mock<TaxSettings>();
            var grandConfigMock = new Mock<GrandConfig>();
            var twoFactorAuthenticationServiceMock = new Mock<ITwoFactorAuthenticationService>();

            _controller = new CustomerController(
                customerViewModelServiceMock.Object,
                grandAuthenticationServiceMock.Object,
                localizationServiceMock.Object,
                workContextMock.Object,
                storeContextMock.Object,
                customerServiceMock.Object,
                customerAttributeParserMock.Object,
                genericAttributeServiceMock.Object,
                customerRegistrationServiceMock.Object,
                taxServiceMock.Object,
                countryServiceMock.Object,
                newsLetterSubscriptionServiceMock.Object,
                customerActivityServiceMock.Object,
                addressViewModelServiceMock.Object,
                mediatorMock.Object,
                workflowMessageServiceMock.Object,
                captchaSettingsMock.Object,
                customerSettingsMock.Object,
                dateTimeSettingsMock.Object,
                localizationSettingsMock.Object,
                taxSettingsMock.Object,
                grandConfigMock.Object,
                twoFactorAuthenticationServiceMock.Object
                );
        }
        /*
        [TestMethod()]
        public void ReturnCorrectTwoFactorAuthenticationModel()
        {
            var model = new TwoFactorAuthenticationModel { Email = "test@mail.com", UserUniqueKey = "test@mail.comqwerty123" };
            var result = _controller.TwoFactorAuthenticate(model);
            

        }
        */
    }
}
