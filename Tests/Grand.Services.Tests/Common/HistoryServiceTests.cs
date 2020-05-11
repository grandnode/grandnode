using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Common
{
    [TestClass()]
    public class HistoryServiceTests
    {
        private Mock<IRepository<HistoryObject>> _mockHistoryRepository;
        private IHistoryService _historyService;

        [TestInitialize()]
        public void Init()
        {
            _mockHistoryRepository = new Mock<IRepository<HistoryObject>>();
            _historyService = new HistoryService(_mockHistoryRepository.Object);
        }

        [TestMethod()]
        public void SaveObject_NullObject_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _historyService.SaveObject<Product>(null), "entity");
        }

        [TestMethod()]
        public async Task SaveObject_InvokeRepositoryWithCorrectObject()
        {
            var product = new Product() { Id = "1" };
            await _historyService.SaveObject<Product>(product);
            _mockHistoryRepository.Verify(c => c.InsertAsync(It.Is<HistoryObject>(h=>h.Object.Id.Equals(product.Id))), Times.Once);
        }

    }
}
