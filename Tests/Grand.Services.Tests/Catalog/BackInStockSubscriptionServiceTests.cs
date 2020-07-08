using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Catalog;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class BackInStockSubscriptionServiceTests
    {
        private Mock<IRepository<BackInStockSubscription>> _repositoryMock;
        private Mock<IMediator> _mediatorMock;
        private BackInStockSubscriptionService _backInStockSubscriptionService;

        [TestInitialize()]
        public void Init()
        {
            _repositoryMock = new Mock<IRepository<BackInStockSubscription>>();
            _mediatorMock = new Mock<IMediator>();
            _backInStockSubscriptionService = new BackInStockSubscriptionService(_repositoryMock.Object, _mediatorMock.Object);
        }

        [TestMethod()]
        public void SendNotificationsToSubscribers_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async ()=> await _backInStockSubscriptionService.SendNotificationsToSubscribers(null, ""));
        }

        [TestMethod()]
        public async Task SendNotificationsToSubscribers_ValidArgument_InvokeSendCommandAndDeleteSubscription()
        {
            var subscriptions = new List<BackInStockSubscription>()
            {
                new BackInStockSubscription(),
                new BackInStockSubscription(),
                new BackInStockSubscription(),
                new BackInStockSubscription()
            };
            _mediatorMock.Setup(c => c.Send(It.IsAny<SendNotificationsToSubscribersCommand>(), default(CancellationToken)))
                .Returns(async () => await Task.FromResult(subscriptions));
            await _backInStockSubscriptionService.SendNotificationsToSubscribers(new Product(), "");
            _mediatorMock.Verify(c => c.Send(It.IsAny<SendNotificationsToSubscribersCommand>(), default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BackInStockSubscription>>(), default(CancellationToken)), Times.Exactly(subscriptions.Count));
            _repositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BackInStockSubscription>()), Times.Exactly(subscriptions.Count));
        }

        [TestMethod()]
        public async Task SendNotificationsToSubscribers_NotFoundSubscribers()
        {
            var subscriptions = new List<BackInStockSubscription>();
            _mediatorMock.Setup(c => c.Send(It.IsAny<SendNotificationsToSubscribersCommand>(), default(CancellationToken)))
                .Returns(async () => await Task.FromResult(subscriptions));
            await _backInStockSubscriptionService.SendNotificationsToSubscribers(new Product(), "");
            _mediatorMock.Verify(c => c.Send(It.IsAny<SendNotificationsToSubscribersCommand>(), default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BackInStockSubscription>>(), default(CancellationToken)), Times.Never);
            _repositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BackInStockSubscription>()), Times.Never);
        }
    }
}
