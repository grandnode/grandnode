using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Stores;
using Grand.Services.Stores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Services.Tests.Stores
{
    [TestClass()]
    public class StoreMappingServiceTests
    {
        private Mock<IStoreContext> _storeContextMock;
        private CatalogSettings _settings;
        private IStoreMappingService _storeMappingsService;

        [TestInitialize()]
        public void Init()
        {
            _storeContextMock = new Mock<IStoreContext>();
            _settings = new CatalogSettings();
            _storeMappingsService = new StoreMappingService(_storeContextMock.Object, _settings);
        }

        [TestMethod()]
        public void Authorize_NullEntity_ReturnFalse()
        {
            _storeContextMock.Setup(s => s.CurrentStore).Returns(new Store() { Id = "id" });
            var result = _storeMappingsService.Authorize<Product>(null);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void Authorize_NullEntity_ReturnTrue()
        {
            _storeContextMock.Setup(s => s.CurrentStore).Returns(new Store() { Id = "" });
            var result = _storeMappingsService.Authorize<Product>(new Product());
            Assert.IsTrue(result); 
        }

        [TestMethod()]
        public void Authorize_IgnoreStoreLimitations_ReturnTrue()
        {
            _settings.IgnoreStoreLimitations=true;
            _storeContextMock.Setup(s => s.CurrentStore).Returns(new Store() { Id = "id" });
            var result = _storeMappingsService.Authorize<Product>(new Product());
            Assert.IsTrue(result); 
        }

        [TestMethod()]
        public void Authorize_NotLimitedToStores_ReturnTrue()
        {
            _storeContextMock.Setup(s => s.CurrentStore).Returns(new Store() { Id = "id2" });
            var product = new Product();
            var result = _storeMappingsService.Authorize<Product>(product);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void Authorize_StoreIdWithAccess_ReturnTrue()
        {
            
            _storeContextMock.Setup(s => s.CurrentStore).Returns(new Store() { Id = "id2" });
            var product = new Product();
            product.LimitedToStores = true;
            product.Stores = new List<string>() {
                "id1","id2"
            };
            var result = _storeMappingsService.Authorize<Product>(product);
            Assert.IsTrue(result);
        }
    }
}
