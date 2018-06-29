using Grand.Core.Data;
using Grand.Core.Domain.PushNotifications;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.PushNotifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grand.Services.Logging;
using System;
using System.Linq;

namespace Grand.Services.Tests.PushNotifications
{
    [TestClass()]
    public class PushNotificationsTests
    {
        private IRepository<PushRegistration> _registrationRepository;
        private IRepository<PushMessage> _messageRepository;
        private IPushNotificationsService _pushNotificationsService;
        private IEventPublisher _eventPublisher;
        private PushNotificationsSettings _pushNotificationsSettings;
        private ILocalizationService _localizationService;
        private ILogger _logger;

        [TestInitialize()]
        public void TestInitialize()
        {
            _registrationRepository = new MongoDBRepositoryTest<PushRegistration>();
            _messageRepository = new MongoDBRepositoryTest<PushMessage>();

            var eventPublisher = new Mock<IEventPublisher>();
            eventPublisher.Setup(x => x.Publish(new object()));
            _eventPublisher = eventPublisher.Object;

            var pushNotificationsSettings = new Mock<PushNotificationsSettings>();
            _pushNotificationsSettings = pushNotificationsSettings.Object;

            var localizationService = new Mock<ILocalizationService>();
            _localizationService = localizationService.Object;

            var logger = new Mock<ILogger>();
            _logger = logger.Object;

            _pushNotificationsService = new PushNotificationsService(_registrationRepository, _messageRepository, _eventPublisher,
                _pushNotificationsSettings, _localizationService, _logger);
        }

        public void ClearPushRegistrations()
        {
            foreach (var item in _registrationRepository.Table)
            {
                _registrationRepository.Delete(item);
            }
        }

        public void ClearPushMessages()
        {
            foreach (var item in _messageRepository.Table)
            {
                _messageRepository.Delete(item);
            }
        }

        [TestMethod()]
        public void CanInsertRegistration()
        {
            ClearPushRegistrations();

            DateTime date = DateTime.UtcNow;

            _pushNotificationsService.InsertPushReceiver(new PushRegistration
            {
                Allowed = true,
                CustomerId = "CanInsertRegistrationCustomerId",
                Token = "CanInsertRegistrationToken",
                RegisteredOn = date
            });

            var found = _registrationRepository.Table.Where(x => x.Token == "CanInsertRegistrationToken").FirstOrDefault();

            Assert.AreNotEqual(null, found);
            Assert.AreEqual(true, found.Allowed);
            Assert.AreEqual("CanInsertRegistrationCustomerId", found.CustomerId);
            Assert.AreEqual("CanInsertRegistrationToken", found.Token);
            Assert.AreEqual(date.Date, found.RegisteredOn.Date);
        }

        [TestMethod()]
        public void CanInsertMessage()
        {
            ClearPushMessages();

            DateTime date = DateTime.UtcNow;

            _pushNotificationsService.InsertPushMessage(new PushMessage
            {
                NumberOfReceivers = 1,
                SentOn = date,
                Text = "CanInsertMessageText",
                Title = "CanInsertMessageTitle"
            });

            var found = _messageRepository.Table.Where(x => x.Text == "CanInsertMessageText").FirstOrDefault();

            Assert.AreNotEqual(null, found);
            Assert.AreEqual("CanInsertMessageText", found.Text);
            Assert.AreEqual("CanInsertMessageTitle", found.Title);
            Assert.AreEqual(date.Date, found.SentOn.Date);
        }

        [TestMethod()]
        public void CanDeleteRegistration()
        {
            ClearPushRegistrations();

            var toDelete = new PushRegistration();
            _pushNotificationsService.InsertPushReceiver(toDelete);

            Assert.AreEqual(1, _registrationRepository.Table.Count());
            _pushNotificationsService.DeletePushReceiver(toDelete);
            Assert.AreEqual(0, _registrationRepository.Table.Count());
        }

        [TestMethod()]
        public void CanUpdateRegistration()
        {
            ClearPushRegistrations();

            var registration = new PushRegistration() { Token = "CanUpdateRegistrationToken" };
            _pushNotificationsService.InsertPushReceiver(registration);

            registration.Token = "CanUpdateRegistrationToken1";
            _pushNotificationsService.UpdatePushReceiver(registration);

            var found = _registrationRepository.Table.Where(x => x.Token == "CanUpdateRegistrationToken1");

            Assert.AreEqual(1, found.Count());
        }

        [TestMethod()]
        public void CanGetRegistration()
        {
            ClearPushRegistrations();

            var registration = new PushRegistration { Token = "CanGetRegistration" };
            _pushNotificationsService.InsertPushReceiver(registration);

            var actual = _registrationRepository.Table.Where(x => x.Token == "CanGetRegistration").First();
            var found = _pushNotificationsService.GetPushReceiver(actual.Id);

            Assert.AreEqual(actual.Token, found.Token);
            Assert.AreEqual(actual.Id, found.Id);
        }

        [TestMethod()]
        public void CanGetMessages()
        {
            ClearPushMessages();

            var message1 = new PushMessage { Text = "CanGetMessages1" };
            var message2 = new PushMessage { Text = "CanGetMessages2" };
            _pushNotificationsService.InsertPushMessage(message1);
            _pushNotificationsService.InsertPushMessage(message2);

            var found = _pushNotificationsService.GetPushMessages();

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Text == "CanGetMessages1"));
            Assert.AreEqual(true, found.Any(x => x.Text == "CanGetMessages2"));
        }

        [TestMethod()]
        public void CanGetReceivers()
        {
            ClearPushRegistrations();

            var receiver1 = new PushRegistration { Token = "CanGetReceivers1", Allowed = true };
            var receiver2 = new PushRegistration { Token = "CanGetReceivers2", Allowed = true };
            _pushNotificationsService.InsertPushReceiver(receiver1);
            _pushNotificationsService.InsertPushReceiver(receiver2);

            var found = _pushNotificationsService.GetPushReceivers();

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(true, found.Any(x => x.Token == "CanGetReceivers1"));
            Assert.AreEqual(true, found.Any(x => x.Token == "CanGetReceivers2"));
        }

        [TestMethod()]
        public void CanGetAllowedAndDeniedReceivers()
        {
            ClearPushRegistrations();

            var receiver1 = new PushRegistration { Allowed = true };
            var receiver2 = new PushRegistration { Allowed = true };
            var receiver3 = new PushRegistration { Allowed = false };

            _pushNotificationsService.InsertPushReceiver(receiver1);
            _pushNotificationsService.InsertPushReceiver(receiver2);
            _pushNotificationsService.InsertPushReceiver(receiver3);

            var allowed = _pushNotificationsService.GetAllowedReceivers();
            var denied = _pushNotificationsService.GetDeniedReceivers();

            Assert.AreEqual(2, allowed);
            Assert.AreEqual(1, denied);
        }

        [TestMethod()]
        public void CanGetPushReceiverByCustomerId()
        {
            ClearPushRegistrations();

            var receiver1 = new PushRegistration
            {
                CustomerId = "CanGetPushReceiverByCustomerId1",
                Token = "CanGetPushReceiverByCustomerIdToken1"
            };

            var receiver2 = new PushRegistration
            {
                CustomerId = "CanGetPushReceiverByCustomerId2",
                Token = "CanGetPushReceiverByCustomerIdToken2"
            };

            _pushNotificationsService.InsertPushReceiver(receiver1);
            _pushNotificationsService.InsertPushReceiver(receiver2);

            var found = _pushNotificationsService.GetPushReceiverByCustomerId("CanGetPushReceiverByCustomerId1");

            Assert.AreEqual("CanGetPushReceiverByCustomerIdToken1", found.Token);
        }
    }
}
