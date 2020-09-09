using System;
using Grand.Core.Events;
using Grand.Services.Documents;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Grand.Domain.Documents;
using Grand.Domain.Data;

namespace Grand.Services.Tests.Documents
{
    [TestClass()]
    public class DocumentServiceTests
    {
        private Mock<IRepository<Document>> _documentRepositoryMock;
        private DocumentService _documentService;
        private Mock<IMediator> _mediatorMock;

        [TestInitialize()]
        public void Init()
        {
            _mediatorMock = new Mock<IMediator>();
            _documentRepositoryMock = new Mock<IRepository<Document>>();
            _documentService = new DocumentService(_documentRepositoryMock.Object, _mediatorMock.Object);
        }

        [TestMethod()]
        public async Task DeleteDocument_NullArgument_ThrowException()
        {
            await  Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _documentService.Delete(null), "document");
        }

        [TestMethod()]
        public async Task DeleteDocument_ValidArgument()
        {
            await _documentService.Delete(new Document());
            _documentRepositoryMock.Verify(c => c.DeleteAsync(It.IsAny<Document>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<Document>>(), default), Times.Once);
        }

        [TestMethod]
        public async Task GetDocumentById()
        {
            await _documentService.GetById("id");
            _documentRepositoryMock.Verify(c => c.GetByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task InsertDocument_NullArgument_ThrowException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _documentService.Insert(null), "document");
        }

        [TestMethod()]
        public async Task InsertDocument_ValidArgument()
        {
            await _documentService.Insert(new Document());
            _documentRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<Document>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<Document>>(), default), Times.Once);
        }

        [TestMethod()]
        public async Task UpdateDocument_ValidArgument()
        {
            await _documentService.Update(new Document());
            _documentRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<Document>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<Document>>(), default), Times.Once);
        }

        [TestMethod()]
        public void UpdateDocument_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _documentService.Update(null), "document");
        }

         
    }
}
