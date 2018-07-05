using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grand.Services.Messages.Tests
{
    [TestClass()]
    public class NewsLetterSubscriptionServiceTests {
        private Mock<IEventPublisher> tempEventPublisher;
        private IEventPublisher _eventPublisher;
        //private Mock<IRepository<NewsLetterSubscription>> tempNewsLetterSubscriptionRepository;
        private IRepository<NewsLetterSubscription> _newsLetterSubscriptionRepository;
        private IRepository<Customer> _customerRepository;
        private ICustomerService _customerService;

        [TestInitialize()]
        public void TestInitialize() {
            tempEventPublisher = new Mock<IEventPublisher>();
            {
                _eventPublisher = tempEventPublisher.Object;
            }
            _newsLetterSubscriptionRepository = new Mock<IRepository<NewsLetterSubscription>>().Object;
            _customerRepository = new MongoDBRepositoryTest<Customer>();
            _customerService = new Mock<ICustomerService>().Object;
        }
        /* TODO - Problem is that the method InsertNewsLetterSubscription used static class
        [TestMethod()]
        public void InsertNewsLetterSubscriptionTest() {
            var service = new NewsLetterSubscriptionService(_newsLetterSubscriptionRepository,
                _customerRepository, _eventPublisher, _customerService);

            var subscription = new NewsLetterSubscription { Active = true, Email = "test@test.com" };
            service.InsertNewsLetterSubscription(subscription, true);

            tempEventPublisher.Verify(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));
            tempEventPublisher.Object.Publish(new EmailSubscribedEvent(subscription.Email));
        }
        */
        /* TODO - Problem is that the method UpdateNewsLetterSubscription used static class
        [TestMethod()]
        public void VerifyEmailUpdateTriggersUnsubscribeAndSubscribeEvent() {
            var originalSubscription = new NewsLetterSubscription { Active = true, Email = "test@test.com" };

            tempNewsLetterSubscriptionRepository = new Mock<IRepository<NewsLetterSubscription>>();
            {
                tempNewsLetterSubscriptionRepository.SetupAllProperties(); 
                tempNewsLetterSubscriptionRepository.Setup(x => x.GetById(It.IsAny<string>())).Returns(originalSubscription);
                _newsLetterSubscriptionRepository = tempNewsLetterSubscriptionRepository.Object;
            }

            var service = new NewsLetterSubscriptionService(
                _newsLetterSubscriptionRepository, _customerRepository,
                _eventPublisher, _customerService);

            var subscription = new NewsLetterSubscription { Active = true, Email = "test@somenewdomain.com" };
            service.UpdateNewsLetterSubscription(subscription, true);

            //call method
            tempEventPublisher.Object.Publish(new EmailUnsubscribedEvent(originalSubscription.Email));
            tempEventPublisher.Object.Publish(new EmailSubscribedEvent(subscription.Email));
            //verify call
            tempEventPublisher.Verify(x => x.Publish(new EmailUnsubscribedEvent(originalSubscription.Email)));
            tempEventPublisher.Verify(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));
        }
        */

        /* TODO - Problem is that the method UpdateNewsLetterSubscription used static class
        [TestMethod()]
        public void VerifyInactiveToActiveUpdateTriggersSubscribeEvent() {
            var originalSubscription = new NewsLetterSubscription { Active = false, Email = "test@test.com" };

            tempNewsLetterSubscriptionRepository = new Mock<IRepository<NewsLetterSubscription>>();
            {
                tempNewsLetterSubscriptionRepository.SetupAllProperties();
                tempNewsLetterSubscriptionRepository.Setup(x => x.GetById(It.IsAny<string>())).Returns(originalSubscription);
                _newsLetterSubscriptionRepository = tempNewsLetterSubscriptionRepository.Object;
            }

            var service = new NewsLetterSubscriptionService(
                _newsLetterSubscriptionRepository, _customerRepository,
                _eventPublisher, _customerService);

            var subscription = new NewsLetterSubscription { Active = true, Email = "test@somenewdomain.com" };
            service.UpdateNewsLetterSubscription(subscription, true);

            //call method
            tempEventPublisher.Object.Publish(new EmailSubscribedEvent(subscription.Email));
            //verify call
            tempEventPublisher.Verify(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));           
        }        
        */
    }
}