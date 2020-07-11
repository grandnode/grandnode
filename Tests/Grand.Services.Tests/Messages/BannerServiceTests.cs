using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Core.Events;
using Grand.Services.Messages;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Messages
{
    [TestClass()]
    public class BannerServiceTests
    {
        private Mock<IRepository<Banner>> _repositoryMock;
        private Mock<IMediator> _mediatorMock;
        private BannerService _bannerService;

        [TestInitialize()]
        public void Init()
        {
            _repositoryMock = new Mock<IRepository<Banner>>();
            _mediatorMock = new Mock<IMediator>();
            _bannerService = new BannerService(_repositoryMock.Object, _mediatorMock.Object);
        }

        [TestMethod()]
        public void InsertBanner_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _bannerService.InsertBanner(null), "blogPost");
        }

        [TestMethod()]
        public async Task InsertBanner_ValidArgument_InvokeRepository()
        {
            await _bannerService.InsertBanner(new Banner());
            _repositoryMock.Verify(c => c.InsertAsync(It.IsAny<Banner>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<Banner>>(), default(CancellationToken)), Times.Once);
        }


        [TestMethod()]
        public async Task DeleteBaner_ValidArgument_InvokeRepository()
        {
            await _bannerService.DeleteBanner(new Banner());
            _repositoryMock.Verify(c => c.DeleteAsync(It.IsAny<Banner>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<Banner>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public async Task GetBannerById_InvokeRepository()
        {
            await _bannerService.GetBannerById("id");
            _repositoryMock.Verify(r => r.GetByIdAsync("id"), Times.Once);
        }
        [TestMethod()]
        public async Task UpdateBanner_ValiArgument_InvokeRepository()
        {
            await _bannerService.UpdateBanner(new Banner());
            _repositoryMock.Verify(c => c.UpdateAsync(It.IsAny<Banner>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<Banner>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void UpdateBlogPost_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _bannerService.UpdateBanner(null), "blogPost");
        }
    }
}
