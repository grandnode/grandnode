using Grand.Core.Configuration;
using Grand.Domain.Customers;
using Grand.Services.Authentication;
using Grand.Services.Common;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Authentication
{
    [TestClass()]
    public class CookieAuthenticationServiceTests
    {
        private Mock<ICustomerService> _customerServiceMock;
        private Mock<IHttpContextAccessor> _httpAccessorMock;
        private CustomerSettings _customerSettings;
        private CookieAuthenticationService _cookieAuthService;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IServiceProvider> serviceProviderMock;
        private DefaultHttpContext _httpContext;
        private GrandConfig _config;
        private IGenericAttributeService _genericAttributeService;

        [TestInitialize()]
        public void Init()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _genericAttributeService = new Mock<IGenericAttributeService>().Object;
            _httpAccessorMock = new Mock<IHttpContextAccessor>();
            _customerSettings = new CustomerSettings();
            _config = new GrandConfig();
            _config.CookieClaimsIssuer = "grandnode";
            _config.CookiePrefix = ".Grand.";
            _cookieAuthService = new CookieAuthenticationService(_customerSettings, _customerServiceMock.Object, _genericAttributeService, _httpAccessorMock.Object, _config);
            //For mock HttpContext extension methods like SignOutAsync ,SignInAsync etc..
            _authServiceMock = new Mock<IAuthenticationService>();
            serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(_authServiceMock.Object);
            _httpContext = new DefaultHttpContext() { RequestServices = serviceProviderMock.Object };
            _httpAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);
        }

        [TestMethod()]
        public async Task SignOut()
        {
            _authServiceMock.Setup(c => c.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()));
            await _cookieAuthService.SignOut();
            _authServiceMock.Verify(c => c.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }



        [TestMethod()]
        public async Task SignIn_NullCustomer_ThrowException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _cookieAuthService.SignIn(null, false));
        }


        [TestMethod()]
        public async Task SignIn_ValidCustomer()
        {
            _authServiceMock.Setup(c => c.SignInAsync(It.IsAny<HttpContext>(),
                It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(() => Task.CompletedTask);
            await _cookieAuthService.SignIn(new Customer(), false);
            _authServiceMock.Verify(c => c.SignInAsync(It.IsAny<HttpContext>(),
                It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [TestMethod()]
        public async Task GetAuthenticatedCustomer_AuthenticatedFaild_ReturnNull()
        {
            _authServiceMock.Setup(c => c.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(AuthenticateResult.Fail(new Exception())));
            var customer =await _cookieAuthService.GetAuthenticatedCustomer();
            Assert.IsNull(customer);
        }

        [TestMethod()]
        public async Task GetAuthenticatedCustomer_UsernameEnableRegisterd_ReturnCustomer()
        {
            var expectedCustomer = new Customer() { Username = "John",Active=true};
            expectedCustomer.CustomerRoles.Add(new CustomerRole { SystemName= SystemCustomerRoleNames.Registered,Active=true});
            _customerSettings.UsernamesEnabled = true;
            var cliaim = new Claim(ClaimTypes.Name, "Johny","", "grandnode");
            IList <Claim> claims = new List<Claim>
            {
                 new Claim(ClaimTypes.Name,ClaimTypes.Name,"", "grandnode")
              };
            var principals = new ClaimsPrincipal(new ClaimsIdentity(claims, GrandCookieAuthenticationDefaults.AuthenticationScheme));
            _authServiceMock.Setup(c => c.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult<AuthenticateResult>(AuthenticateResult.Success(new AuthenticationTicket(principals,""))));
            _customerServiceMock.Setup(c => c.GetCustomerByUsername(It.IsAny<string>())).Returns(() => Task.FromResult(expectedCustomer));
  
            var customer = await _cookieAuthService.GetAuthenticatedCustomer();
            Assert.AreEqual(customer.Username, expectedCustomer.Username);
        }

        [TestMethod()]
        public async Task GetAuthenticatedCustomer_UsernameEnableGuests_ReturnNull()
        {
            var expectedCustomer = new Customer() { Username = "John", Active = true };
            expectedCustomer.CustomerRoles.Add(new CustomerRole { SystemName = SystemCustomerRoleNames.Guests ,Active = true });
            _customerSettings.UsernamesEnabled = true;
            var cliaim = new Claim(ClaimTypes.Name, "Johny", "", "grandnode");
            IList<Claim> claims = new List<Claim>
            {
                 new Claim(ClaimTypes.Name,ClaimTypes.Name,"", "grandnode")
              };
            var principals = new ClaimsPrincipal(new ClaimsIdentity(claims, GrandCookieAuthenticationDefaults.AuthenticationScheme));
            _authServiceMock.Setup(c => c.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult<AuthenticateResult>(AuthenticateResult.Success(new AuthenticationTicket(principals, ""))));
            _customerServiceMock.Setup(c => c.GetCustomerByUsername(It.IsAny<string>())).Returns(() => Task.FromResult(expectedCustomer));

            var customer = await _cookieAuthService.GetAuthenticatedCustomer();
            Assert.IsNull(customer);
            //_customerServiceMock.Verify(c => c.GetCustomerByUsername(It.IsAny<string>()), Times.Once);
        }

    }

}
