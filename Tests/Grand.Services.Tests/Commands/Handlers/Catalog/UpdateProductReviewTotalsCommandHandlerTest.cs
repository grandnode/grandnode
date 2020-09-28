﻿using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grand.Services.Commands.Handlers.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Services.Commands.Models.Catalog;
using MongoDB.Driver;
using Grand.Services.Catalog;

namespace Grand.Services.Tests.Commands.Handlers.Catalog
{
    [TestClass()]
    public class UpdateProductReviewTotalsCommandHandlerTest
    {
        private Mock<IRepository<Product>> _productRepositoryMock;
        private Mock<IRepository<ProductReview>> _productReviewRepositoryMock;
        private Mock<ICacheManager> _cacheManagerMock;
        private UpdateProductReviewTotalsCommandHandler _updateProductReviewTotalsCommandHandler;
        private Mock<IMongoCollection<Product>> _mongoCollectionMock;
        private Mock<IProductReviewService> _productReviewServiceMock;


        [TestInitialize()]
        public void Init()
        {
            var reviews = new List<ProductReview> {
                new ProductReview { Id = "1", ReplyText = "text1"},
                new ProductReview { Id = "2", ReplyText = "text2"}};

            _mongoCollectionMock = new Mock<IMongoCollection<Product>>();
            _mongoCollectionMock.SetupAllProperties();
            _mongoCollectionMock.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Product>>(), 
                It.IsAny<UpdateDefinition<Product>>(), 
                It.IsAny<UpdateOptions>(), 
                default));
                       
            _productRepositoryMock = new Mock<IRepository<Product>>();
            _productRepositoryMock.Setup(x => x.Collection).Returns(_mongoCollectionMock.Object);

            _productReviewRepositoryMock = new Mock<IRepository<ProductReview>>();
            _cacheManagerMock = new Mock<ICacheManager>();

            _productReviewServiceMock = new Mock<IProductReviewService>();
            _productReviewServiceMock.Setup(x => x.ProductReviewsByProductAsync(It.IsAny<string>())).Returns(Task.FromResult(reviews));

            _updateProductReviewTotalsCommandHandler = new UpdateProductReviewTotalsCommandHandler(
                _productRepositoryMock.Object,
                _productReviewRepositoryMock.Object,
                _productReviewServiceMock.Object,
                _cacheManagerMock.Object);
        }

        [TestMethod()]
        public async Task InsertProduct_NullArgument_ThrowException()
        {
            var request = new UpdateProductReviewTotalsCommand();
            
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _updateProductReviewTotalsCommandHandler.Handle(request, default));
        }

        [TestMethod()]
        public async Task UpdateProductReviews_ValidArgument_InvokeRepositoryAndCache()
        {
            var request = new UpdateProductReviewTotalsCommand { Product = new Product() };
            await _updateProductReviewTotalsCommandHandler.Handle(request, default);

            _productReviewServiceMock.Verify(x => x.ProductReviewsByProductAsync(It.IsAny<string>()), Times.Once);
            _productRepositoryMock.Verify(x => x.Collection.UpdateOneAsync(It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<UpdateDefinition<Product>>(),
                It.IsAny<UpdateOptions>(),
                default), Times.Once);
        }
    }
}
