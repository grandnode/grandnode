using Grand.Domain.Customers;
using Grand.Services.Authentication;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    public class ApiAuthenticationServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMoc;
        private Mock<ICustomerService> _customerService;
        private Mock<IUserApiService> _userApiServiceMock;
        private Mock<HttpContext> _httpContextMock;
        private IApiAuthenticationService _authService;

        [TestInitialize()]
        public void Init()
        {
            _httpContextAccessorMoc = new Mock<IHttpContextAccessor>();
            _customerService = new Mock<ICustomerService>();
            _userApiServiceMock = new Mock<IUserApiService>();
            _httpContextMock = new Mock<HttpContext>();
            _authService = new ApiAuthenticationService(_httpContextAccessorMoc.Object, _customerService.Object, _userApiServiceMock.Object);
        }

        [TestMethod()]
        public void SignIn_EmptyEmail_ThrowExcepiton()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _authService.SignIn(string.Empty));
        }


       

        [TestMethod()]
        public async Task Valid_NullEmail_ReturnFalse()
        {
            var httpContext = new Mock<HttpContext>();
            var context = new TokenValidatedContext(httpContext.Object,new AuthenticationScheme("","",typeof(AuthSchemaMock)), new JwtBearerOptions());
            context.Principal = new ClaimsPrincipal();
            var result =await _authService.Valid(context);
            Assert.IsFalse(result);
            Assert.AreEqual(await _authService.ErrorMessage(), "Email not exists in the context");
        }

        [TestMethod()]
        public async Task Valid_NotFoundCoustomer_ReturnFalse()
        {
            var httpContext = new Mock<HttpContext>();
            var context = new TokenValidatedContext(httpContext.Object, new AuthenticationScheme("", "", typeof(AuthSchemaMock)), new JwtBearerOptions());
            IList<Claim> claims = new List<Claim>
            {
                 new Claim("Email", "johny@gmail.com")
            };
            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims,""));
            _customerService.Setup(c => c.GetCustomerByEmail(It.IsAny<string>())).Returns(() => Task.FromResult<Customer>(null));
            var result = await _authService.Valid(context);
            Assert.IsFalse(result);
            Assert.AreEqual(await _authService.ErrorMessage(), "Email not exists/or not active in the customer table");
        }

        [TestMethod()]
        public async Task Valid_NotActiveCustomer_ReturnFalse()
        {
            var httpContext = new Mock<HttpContext>();
            var context = new TokenValidatedContext(httpContext.Object, new AuthenticationScheme("", "", typeof(AuthSchemaMock)), new JwtBearerOptions());
            IList<Claim> claims = new List<Claim>
            {
                 new Claim("Email", "johny@gmail.com")
            };
            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ""));
            _customerService.Setup(c => c.GetCustomerByEmail(It.IsAny<string>())).Returns(() => Task.FromResult(new Customer() { Active=false}));
            var result = await _authService.Valid(context);
            Assert.IsFalse(result);
            Assert.AreEqual(await _authService.ErrorMessage(), "Email not exists/or not active in the customer table");
        }

        [TestMethod()]
        public async Task Valid_NotActiveUserApi_ReturnFalse()
        {
            var httpContext = new Mock<HttpContext>();
            var context = new TokenValidatedContext(httpContext.Object, new AuthenticationScheme("", "", typeof(AuthSchemaMock)), new JwtBearerOptions());
            IList<Claim> claims = new List<Claim>
            {
                 new Claim("Email", "johny@gmail.com")
            };
            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ""));
            _customerService.Setup(c => c.GetCustomerByEmail(It.IsAny<string>())).Returns(() => Task.FromResult(new Customer() { Active = true }));
            _userApiServiceMock.Setup(c => c.GetUserByEmail(It.IsAny<string>())).Returns(() => Task.FromResult(new UserApi() { IsActive = false }));
            var result = await _authService.Valid(context);
            Assert.IsFalse(result);
            Assert.AreEqual("User api not exists/or not active in the user api table", await _authService.ErrorMessage());
        }

        [TestMethod()]
        public async Task Valid_ActiveUserApi_ReturnTrue()
        {
            var httpContext = new Mock<HttpContext>();
            var context = new TokenValidatedContext(httpContext.Object, new AuthenticationScheme("", "", typeof(AuthSchemaMock)), new JwtBearerOptions());
            IList<Claim> claims = new List<Claim>
            {
                 new Claim("Email", "johny@gmail.com")
            };
            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ""));
            _customerService.Setup(c => c.GetCustomerByEmail(It.IsAny<string>())).Returns(() => Task.FromResult(new Customer() { Active = true }));
            _userApiServiceMock.Setup(c => c.GetUserByEmail(It.IsAny<string>())).Returns(() => Task.FromResult(new UserApi() {IsActive=true }));
            var result = await _authService.Valid(context);
            Assert.IsTrue(result);
        }

        

        [TestMethod()]
        public async Task GetAuthenticatedCustomer_NullAuthHeader_GetNull()
        {
            var customer = new Customer() { Email = "johny@gmail.com" };
            var httpContext = new Mock<HttpContext>();
            var req = new Mock<HttpRequest>();
            req.Setup(c => c.Path).Returns(new PathString("/api/..."));
            req.Setup(c => c.Headers).Returns(new HeaderDictionary());
            httpContext.Setup(c => c.Request).Returns(req.Object);
            _httpContextAccessorMoc.Setup(c => c.HttpContext).Returns(httpContext.Object);
            _customerService.Setup(c => c.GetCustomerBySystemName(It.IsAny<string>())).Returns(() => Task.FromResult(customer));
            var result = await _authService.GetAuthenticatedCustomer();
            Assert.IsNull(result);
        }

    }
}
