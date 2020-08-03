using Grand.Core;
using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Messages;
using Grand.Services.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Common
{
    [TestClass()]
    public class HistoryExtensionsTests
    {
        private Mock<IHistoryService> _service;

        [TestInitialize()]
        public void Init()
        {
            _service = new Mock<IHistoryService>();
        }

        [TestMethod()]
        public void SaveHistory_NullEntity_ThrowExcepiton()
        {
            Product product = null;
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await product.SaveHistory<NewsLetterSubscription>(_service.Object), "entity");
        }

        [TestMethod()]
        public async  Task SaveHistory_InvokeService()
        {
            Product product = new Product();
            await product.SaveHistory<NewsLetterSubscription>(_service.Object);
            _service.Verify(s => s.SaveObject(It.IsAny<BaseEntity>()), Times.Once);
            
        }

        [TestMethod()]
        public async Task GetHistoryObject_InvokeService()
        {
            Product product = new Product();
            await product.GetHistoryObject(_service.Object);
            _service.Verify(s => s.GetHistoryObjectForEntity(It.IsAny<BaseEntity>()), Times.Once);
        }
    }
}
