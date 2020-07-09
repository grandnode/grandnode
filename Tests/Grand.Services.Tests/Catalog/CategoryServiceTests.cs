using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Events;
using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class CategoryServiceTests
    {
        private Mock<ICacheManager> _casheManagerMock;
        private Mock<IRepository<Category>> _categoryRepositoryMock;
        private Mock<IRepository<Product>> _productRepositoryMock;
        private Mock<IWorkContext> _workContextMock;
        private Mock<IStoreContext> _storeContextMock;
        private Mock<IMediator> _mediatorMock;
        private Mock<IStoreMappingService> _storyMappingMock;
        private Mock<IAclService> _aclServiceMock;
        private CatalogSettings _settings;
        private CategoryService _categoryService;

        [TestInitialize()]
        public void Init()
        {
            _casheManagerMock = new Mock<ICacheManager>();
            _categoryRepositoryMock = new Mock<IRepository<Category>>();
            _productRepositoryMock = new Mock<IRepository<Product>>();
            _workContextMock = new Mock<IWorkContext>();
            _storeContextMock = new Mock<IStoreContext>();
            _mediatorMock = new Mock<IMediator>();
            _storyMappingMock = new Mock<IStoreMappingService>();
            _aclServiceMock = new Mock<IAclService>();
            _settings = new CatalogSettings();
            _categoryService = new CategoryService(_casheManagerMock.Object, _categoryRepositoryMock.Object, _productRepositoryMock.Object, _workContextMock.Object,
                _storeContextMock.Object, _mediatorMock.Object, _storyMappingMock.Object, _aclServiceMock.Object, _settings);
        }

        [TestMethod()]
        public void InsertCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _categoryService.InsertCategory(null), "category");
        }

        [TestMethod()]
        public async Task InsertCategory_ValidArgument_InvokeRepositoryAndCache()
        {
            await _categoryService.InsertCategory(new Category());
            _categoryRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<Category>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<Category>>(), default(CancellationToken)), Times.Once);
            _casheManagerMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), true), Times.Exactly(3));
        }


        [TestMethod()]
        public void UpdateCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _categoryService.UpdateCategory(null), "category");
        }

        [TestMethod()]
        public async Task UpdateCategory_ValidArgument_InvokeRepositoryAndCache()
        {
            await _categoryService.UpdateCategory(new Category());
            _categoryRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<Category>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<Category>>(), default(CancellationToken)), Times.Once);
            _casheManagerMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), true), Times.Exactly(3));
        }

        [TestMethod()]
        public void GetCategoryBreadCrumb_ShouldReturnEmptyList()
        {
            var allCategory = GetMockCategoryList();
            var category = new Category() { ParentCategoryId = "3" };
            _aclServiceMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            _storyMappingMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            var result = _categoryService.GetCategoryBreadCrumb(category, allCategory);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod()]
        public void GetCategoryBreadCrumb_ShouldReturnTwoElement()
        {
            var allCategory = GetMockCategoryList();
            var category = new Category() { Id = "6", ParentCategoryId = "3", Published = true };
            _aclServiceMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            _storyMappingMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            var result = _categoryService.GetCategoryBreadCrumb(category, allCategory);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result.Any(c => c.Id.Equals("6")));
            Assert.IsTrue(result.Any(c => c.Id.Equals("3")));
        }

        [TestMethod()]
        public void GetCategoryBreadCrumb_ShouldReturnThreeElement()
        {
            var allCategory = GetMockCategoryList();
            var category = new Category() { Id = "6", ParentCategoryId = "1", Published = true };
            _aclServiceMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            _storyMappingMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            var result = _categoryService.GetCategoryBreadCrumb(category, allCategory);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.Any(c => c.Id.Equals("6")));
            Assert.IsTrue(result.Any(c => c.Id.Equals("1")));
            Assert.IsTrue(result.Any(c => c.Id.Equals("5")));
        }

        [TestMethod()]
        public void GetFormattedBreadCrumb_ReturnExprectedString()
        {
            var exprectedString = "cat5 >> cat1 >> cat6";
            var allCategory = GetMockCategoryList();
            var category = new Category() { Id = "6", Name = "cat6", ParentCategoryId = "1", Published = true };
            _aclServiceMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            _storyMappingMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            var result = _categoryService.GetFormattedBreadCrumb(category, allCategory);
            Assert.IsTrue(exprectedString.Equals(result));
        }

        [TestMethod()]
        public void GetFormattedBreadCrumb_ReturnEmptyString()
        {
            var allCategory = GetMockCategoryList();
            _aclServiceMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            _storyMappingMock.Setup(a => a.Authorize<Category>(It.IsAny<Category>())).Returns(() => true);
            var result = _categoryService.GetFormattedBreadCrumb(null, allCategory);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod()]
        public async Task DeleteProductCategory_InvokreRepositoryAndClearCache()
        {
            var collectonMock = new Mock<IMongoCollection<Product>>();
            _productRepositoryMock.Setup(p => p.Collection).Returns(collectonMock.Object);
            await _categoryService.DeleteProductCategory(new ProductCategory() { ProductId = "1" });
            collectonMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<Product>>(), It.IsAny<UpdateDefinition<Product>>(), null, default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<ProductCategory>>(), default(CancellationToken)), Times.Once);
            _casheManagerMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), true), Times.Exactly(2));
            _casheManagerMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), true), Times.Once);
        }

        [TestMethod()]
        public void DeleteProductCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _categoryService.DeleteProductCategory(null), "productCategory");
        }

        [TestMethod()]
        public async Task InsertProductCategory_InvokreRepositoryAndClearCache()
        {
            var collectonMock = new Mock<IMongoCollection<Product>>();
            _productRepositoryMock.Setup(p => p.Collection).Returns(collectonMock.Object);
            await _categoryService.InsertProductCategory(new ProductCategory() { ProductId = "1" });
            collectonMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<Product>>(), It.IsAny<UpdateDefinition<Product>>(), null, default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<ProductCategory>>(), default(CancellationToken)), Times.Once);
            _casheManagerMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), true), Times.Exactly(2));
            _casheManagerMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), true), Times.Once);
        }


        [TestMethod()]
        public void InsertProductCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _categoryService.InsertProductCategory(null), "productCategory");
        }

        private IList<Category> GetMockCategoryList()
        {
            return new List<Category>()
            {
                new Category(){ Id="1" ,Name="cat1",Published=true,ParentCategoryId="5"},
                new Category(){ Id="2" ,Name="cat2",Published=true},
                new Category(){ Id="3" ,Name="cat3",Published=true},
                new Category(){ Id="4" ,Name="cat4",Published=true},
                new Category(){ Id="5" ,Name="cat5",Published=true},
            };
        }
    }
}
