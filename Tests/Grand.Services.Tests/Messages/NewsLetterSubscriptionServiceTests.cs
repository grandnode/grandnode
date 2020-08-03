using Grand.Core;
using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Core.Events;
using Grand.Services.Common;
using Grand.Services.Notifications.Messages;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Domain;

namespace Grand.Services.Messages.Tests
{
    [TestClass()]
    public class NewsLetterSubscriptionServiceTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IRepository<NewsLetterSubscription>> _subscriptionRepository;
        private Mock<IHistoryService> _historyServiceMock;
        private INewsLetterSubscriptionService _newsLetterSubscriptionService;

        [TestInitialize()]
        public void TestInitialize()
        {
            _mediatorMock = new Mock<IMediator>();
            _subscriptionRepository = new Mock<IRepository<NewsLetterSubscription>>();
            _historyServiceMock = new Mock<IHistoryService>();
            _newsLetterSubscriptionService = new NewsLetterSubscriptionService(_subscriptionRepository.Object, _mediatorMock.Object, _historyServiceMock.Object);
        }


        [TestMethod]
        public void InsertNewsLetterSubscription_InvalidEmail_ThrowException()
        {
            var email = "NotValidEmail";
            var newsLetterSubscription = new NewsLetterSubscription();
            newsLetterSubscription.Email = email;
            Assert.ThrowsExceptionAsync<GrandException>(async () => await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription));
        }

        [TestMethod]
        public async Task InsertNewsLetterSubscription_ActiveSubcription_InvokeRepositoryAndPublishSubscriptionEvent()
        {
            var email = "johny@gmail.com";
            var newsLetterSubscription = new NewsLetterSubscription() { Email = email, Active = true };
            await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription);
            _subscriptionRepository.Verify(r => r.InsertAsync(newsLetterSubscription), Times.Once);
            _historyServiceMock.Verify(h => h.SaveObject(It.IsAny<BaseEntity>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish<EmailSubscribedEvent>(It.IsAny<EmailSubscribedEvent>(), default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<NewsLetterSubscription>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public async Task InsertNewsLetterSubscription_InactiveSubcription_InvokeRepository()
        {
            var email = "johny@gmail.com";
            var newsLetterSubscription = new NewsLetterSubscription() { Email = email, Active = false };
            await _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsLetterSubscription);
            _subscriptionRepository.Verify(r => r.InsertAsync(newsLetterSubscription), Times.Once);
            _historyServiceMock.Verify(h => h.SaveObject(It.IsAny<BaseEntity>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish<EmailSubscribedEvent>(It.IsAny<EmailSubscribedEvent>(), default(CancellationToken)), Times.Never);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<NewsLetterSubscription>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateNewsLetterSubscription_InvokeRepository()
        {
            var email = "johny@gmail.com";
            var newsLetterSubscription = new NewsLetterSubscription() { Email = email, Active = false };
            await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsLetterSubscription);
            _subscriptionRepository.Verify(r => r.UpdateAsync(newsLetterSubscription), Times.Once);
            _historyServiceMock.Verify(h => h.SaveObject(It.IsAny<BaseEntity>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<NewsLetterSubscription>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public async Task DeleteNewsLetterSubscription_InvokeRepositoryAndEmailUnsubscribedEvent()
        {
            var email = "johny@gmail.com";
            var newsLetterSubscription = new NewsLetterSubscription() { Email = email, Active = false };
            await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsLetterSubscription);
            _subscriptionRepository.Verify(r => r.DeleteAsync(newsLetterSubscription), Times.Once);
            _mediatorMock.Verify(m => m.Publish<EmailUnsubscribedEvent>(It.IsAny<EmailUnsubscribedEvent>(), default(CancellationToken)), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<NewsLetterSubscription>>(), default(CancellationToken)), Times.Once);
        }
    }
}