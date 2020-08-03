using DotLiquid.Util;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class CompareProductsServiceTests
    {
        private Mock<IProductService> _productServiceMock;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private CatalogSettings _settings;
        private CompareProductsService _compareProductsService;
        private List<Product> _mockProducts;

        [TestInitialize()]
        public void Init()
        {
            _productServiceMock= new Mock<IProductService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _settings = new CatalogSettings();
            _compareProductsService = new CompareProductsService(_httpContextAccessor.Object, _productServiceMock.Object, _settings);
            _mockProducts = new List<Product>
            {
                new Product(){Id="1",Published=true},
                new Product(){Id="2",Published=true},
                new Product(){Id="3",Published=true},
                new Product(){Id="4",Published=true},
                new Product(){Id="5",Published=true}
            };
        }

        [TestMethod()]
        public async  Task GetComparedProducts_NullHttpContext_ReturnEmptyList()
        {
            var result =await _compareProductsService.GetComparedProducts();
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod()]
        public async Task GetComparedProducts_NotConstainsCookie_ReturnEmptyList()
        {
            var value = "";
            var request = new Mock<HttpRequest>();
            var cookie = new Mock<IRequestCookieCollection>();
            cookie.Setup(c => c.TryGetValue(It.IsAny<string>(), out value)).Returns(false);
            request.Setup(r => r.Cookies).Returns(cookie.Object);
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns(request.Object);
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);

            var result = await _compareProductsService.GetComparedProducts();
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod()]
        public async Task GetComparedProducts_ConstainsCookie_ReturnProductList()
        {
            var ids = "1,2,3,4,5";
            var request = new Mock<HttpRequest>();
            var cookie = new Mock<IRequestCookieCollection>();
            cookie.Setup(c => c.TryGetValue(It.IsAny<string>(), out ids)).Returns(true);
            request.Setup(r => r.Cookies).Returns(cookie.Object);
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns(request.Object);
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            _productServiceMock.Setup(c => c.GetProductById(It.IsAny<string>(), default(bool)))
                .Returns((string w,bool f) => Task.FromResult(_mockProducts.FirstOrDefault(i => i.Id.Equals(w))));

            var result = await _compareProductsService.GetComparedProducts();
            Assert.IsTrue(result.Count == 5);
        }

        [TestMethod()]
        public void RemoveProductFromCompareList_SetNewCompareProductCookie()
        {
            var ids = "1,2,3,4,5";
            var request = new Mock<HttpRequest>();
            var cookie = new Mock<IRequestCookieCollection>();
            cookie.Setup(c => c.TryGetValue(It.IsAny<string>(), out ids)).Returns(true);
            request.Setup(r => r.Cookies).Returns(cookie.Object);
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns(request.Object);
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            _productServiceMock.Setup(c => c.GetProductById(It.IsAny<string>(), default(bool)))
                .Returns((string w, bool f) => Task.FromResult(_mockProducts.FirstOrDefault(i => i.Id.Equals(w))));
            var responseCookie = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookie.Object);
            httpContext.Setup(c => c.Response).Returns(response.Object);
            _compareProductsService.RemoveProductFromCompareList("5");

            //delete current cookie if exists
            responseCookie.Verify(rc => rc.Delete(It.IsAny<string>()),Times.Once);
            //add cookie 
            responseCookie.Verify(rc=>rc.Append(It.IsAny<string>(),"1,2,3,4",It.IsAny<CookieOptions>()), Times.Once);

        }

        [TestMethod()]
        public void ClearCompareProducts_NullHttpContext_NotInvokeDeleteCookie()
        {
            var responseCookie = new Mock<IResponseCookies>();
            _compareProductsService.ClearCompareProducts();
            responseCookie.Verify(rc => rc.Delete(It.IsAny<string>()), Times.Never);
        }


        [TestMethod()]
        public void ClearCompareProducts_InvokeDeleteCookie()
        {
            var httpContext = new Mock<HttpContext>();
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            var responseCookie = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookie.Object);
            httpContext.Setup(c => c.Response).Returns(response.Object);

            _compareProductsService.ClearCompareProducts();
            responseCookie.Verify(rc => rc.Delete(It.IsAny<string>()),Times.Once);
        }

        [TestMethod()]
        public void AddProductToCompareList_SetNewCompareProductCookie()
        {
            var ids = "1,2,3,4,5";
            var exprectedNewCookie = "6,1,2,3,4,5";
            var request = new Mock<HttpRequest>();
            var cookie = new Mock<IRequestCookieCollection>();
            cookie.Setup(c => c.TryGetValue(It.IsAny<string>(), out ids)).Returns(true);
            request.Setup(r => r.Cookies).Returns(cookie.Object);
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns(request.Object);
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            _productServiceMock.Setup(c => c.GetProductById(It.IsAny<string>(), default(bool)))
                .Returns((string w, bool f) => Task.FromResult(_mockProducts.FirstOrDefault(i => i.Id.Equals(w))));
            var responseCookie = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookie.Object);
            httpContext.Setup(c => c.Response).Returns(response.Object);
            _settings.CompareProductsNumber = 10;
            _compareProductsService.AddProductToCompareList("6");

            //delete current cookie if exists
            responseCookie.Verify(rc => rc.Delete(It.IsAny<string>()), Times.Once);
            //add cookie 
            responseCookie.Verify(rc => rc.Append(It.IsAny<string>(), exprectedNewCookie, It.IsAny<CookieOptions>()), Times.Once);

        }
    }
}
