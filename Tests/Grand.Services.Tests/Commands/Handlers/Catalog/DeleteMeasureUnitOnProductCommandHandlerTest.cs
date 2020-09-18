using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grand.Services.Commands.Handlers.Catalog;
using System.Threading.Tasks;
using Grand.Services.Commands.Models.Catalog;
using MongoDB.Driver;

namespace Grand.Services.Tests.Commands.Handlers.Catalog
{
    [TestClass()]
    public class DeleteMeasureUnitOnProductCommandHandlerTest
    {
        private Mock<IRepository<Product>> _mockRepositoryProduct;
        private DeleteMeasureUnitOnProductCommandHandler _mockDeleteMeasureUnitOnProductCommandHandler;
        private Mock<IMongoCollection<Product>> _mockCollection;

        [TestInitialize()]
        public void Init()
        {
            _mockCollection = new Mock<IMongoCollection<Product>>();
            _mockRepositoryProduct = new Mock<IRepository<Product>>();
            _mockRepositoryProduct.Setup(x => x.Collection).Returns(_mockCollection.Object);
            _mockDeleteMeasureUnitOnProductCommandHandler = new DeleteMeasureUnitOnProductCommandHandler(_mockRepositoryProduct.Object);
        }

        [TestMethod]
        public async Task HandleTest()
        {
            await _mockDeleteMeasureUnitOnProductCommandHandler.Handle(new DeleteMeasureUnitOnProductCommand(), default);
            _mockRepositoryProduct.Verify(c => c.Collection.UpdateManyAsync(It.IsAny<FilterDefinition<Product>>(), It.IsAny<UpdateDefinition<Product>>(), null, default), Times.Once);
        }
    }
}
