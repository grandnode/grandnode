using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Tax;
using Grand.Framework.Security.Captcha;
using Grand.Services.Authentication;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Tax;
using Grand.Web.Controllers;
using Grand.Web.Interfaces;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Tests.Controllers
{
    [TestClass]
    public class CustommerControllerTest
    {
        private ICustomerViewModelService _customerViewModelService;
        private IGrandAuthenticationService _authenticationService;
        private ILocalizationService _localizationService;
        private IWorkContext _workContext;
        private IStoreContext _storeContext;
        private ICustomerService _customerService;
        private ICustomerAttributeParser _customerAttributeParser;
        private IGenericAttributeService _genericAttributeService;
        private ICustomerRegistrationService _customerRegistrationService;
        private ITaxService _taxService;
        private ICountryService _countryService;
        private INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private ICustomerActivityService _customerActivityService;
        private IAddressViewModelService _addressViewModelService;
        private IMediator _mediator;
        private IWorkflowMessageService _workflowMessageService;
        private CustomerSettings _customerSettings;
        private DateTimeSettings _dateTimeSettings;
        private TaxSettings _taxSettings;
        private LocalizationSettings _localizationSettings;
        private CaptchaSettings _captchaSettings;
        private GrandConfig _grandConfig;
        private ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private CustomerController _customerController;

        [TestInitialize]
        public void TestInitialize()
        {
            _customerViewModelService = new Mock<ICustomerViewModelService>().Object;
            _authenticationService = new Mock<IGrandAuthenticationService>().Object;
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(p => p.GetResource("Account.WrongCredentials.WrongSecurityCode"))
                .Returns("Error");
            _localizationService = localizationService.Object;

            var workContext = new Mock<IWorkContext>();
            workContext.Setup(p => p.CurrentCustomer)
                .Returns(new Customer { Email = "test@email.com"});
            _workContext = workContext.Object;
            
            _storeContext = new Mock<IStoreContext>().Object;
            _customerService = new Mock<ICustomerService>().Object;
            _customerAttributeParser = new Mock<ICustomerAttributeParser>().Object;
            _genericAttributeService = new Mock<IGenericAttributeService>().Object;
            _customerRegistrationService = new Mock<ICustomerRegistrationService>().Object;
            _taxService = new Mock<ITaxService>().Object;
            _countryService = new Mock<ICountryService>().Object;
            _newsLetterSubscriptionService = new Mock<INewsLetterSubscriptionService>().Object;
            _customerActivityService = new Mock<ICustomerActivityService>().Object;
            _addressViewModelService = new Mock<IAddressViewModelService>().Object;
            _mediator = new Mock<IMediator>().Object;
            _workflowMessageService = new Mock<IWorkflowMessageService>().Object;
            _customerSettings = new Mock<CustomerSettings>().Object;
            _dateTimeSettings = new Mock<DateTimeSettings>().Object;
            _taxSettings = new Mock<TaxSettings>().Object;
            _localizationSettings = new Mock<LocalizationSettings>().Object;
            _captchaSettings = new Mock<CaptchaSettings>().Object;
            _grandConfig = new Mock<GrandConfig>().Object;
            
            var twoFactorAuthenticationService = new Mock<ITwoFactorAuthenticationService>();
            twoFactorAuthenticationService.Setup(p => p.AuthenticateTwoFactor("PJWUMZKAUUFQKJBAMD6VGJ6RULFVW4ZH", "234333"))
                .Returns(true);
            var secretKey = Guid.NewGuid().ToString();
            twoFactorAuthenticationService.Setup(p => p.GenerateQrCodeSetup(It.IsAny<string>()))
                .Returns(new QrCodeSetup { QrCodeImageUrl = "image_url", ManualEntryQrCode = "123456" });
            _twoFactorAuthenticationService = twoFactorAuthenticationService.Object;

            _customerController = new CustomerController(
                _customerViewModelService,
                _authenticationService,
                _localizationService,
                _workContext,
                _storeContext,
                _customerService,
                _customerAttributeParser,
                _genericAttributeService,
                _customerRegistrationService,
                _taxService,
                _countryService,
                _newsLetterSubscriptionService,
                _customerActivityService,
                _addressViewModelService,
                _mediator,
                _workflowMessageService,
                _captchaSettings,
                _customerSettings,
                _dateTimeSettings,
                _localizationSettings,
                _taxSettings,
                _grandConfig,
                _twoFactorAuthenticationService);


        }

        [TestMethod]
        public async Task Post_TwoFactorAuth_Success_CorrectRedirect()
        {
            //assert
            var shoppingCartService = new Mock<IShoppingCartService>().Object;
            //var twoFA = new TwoFactorAuthenticator();
            var secretKey = "PJWUMZKAUUFQKJBAMD6VGJ6RULFVW4ZH";
            //var token = twoFA.GetCurrentPIN(secretKey);

            var model = new CustomerInfoModel.TwoFactorAuthenticationModel {
                UserName = "Test",
                HasAuthenticator = true,
                Is2faEnabled = true,
                SecretKey = secretKey,
                Code = "234333",
            };
                        
            //action
            var result = await _customerController.TwoFactorAuthenticate(model, shoppingCartService);
            var okObjectResult = result as RedirectToRouteResult;
            //result
            Assert.That.IsType<RedirectToRouteResult>(okObjectResult);
        }

        [TestMethod]
        public async Task Post_TwoFactorAuth_ModelErrorReturnViewResultWithCorrectModel()
        {
            //assert
            var shoppingCartService = new Mock<IShoppingCartService>().Object;
            var model = new CustomerInfoModel.TwoFactorAuthenticationModel();
            _customerController.ModelState.AddModelError("", "Field Code is required");

            //action
            var result = await _customerController.TwoFactorAuthenticate(model, shoppingCartService);
            var okObjectResult = result as ViewResult;
            //result
            Assert.That.IsType<ViewResult>(okObjectResult);
            Assert.AreEqual(model, okObjectResult.Model);
        }

        [TestMethod]
        public void Get_EnableTwoFactorAuthenticator_Return_CorrectModel()
        {
            //assert
            _customerSettings.TwoFactorAuthenticationEnabled = true;
            
            //action
            var result = _customerController.EnableTwoFactorAuthenticator();
            var okObjectResult = result as ViewResult;
            
            //result
            Assert.That.IsType<ViewResult>(okObjectResult);
            Assert.That.IsType<CustomerInfoModel.TwoFactorAuthenticationModel>(okObjectResult.Model);
        }

        [TestMethod]
        public void Get_EnableTwoFactorAuthenticator_If_DisableTFA_Correct_RedirectTo()
        {
            //assert
            _customerSettings.TwoFactorAuthenticationEnabled = false;
            
            //action
            var result = _customerController.EnableTwoFactorAuthenticator();
            var okObjectResult = result as RedirectToRouteResult;
            
            //result
            Assert.That.IsType<RedirectToRouteResult>(okObjectResult);
            Assert.AreEqual("CustomerInfo", okObjectResult.RouteName);
        }
    }
}
