using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using Grand.Services.Commands.Handlers.Catalog;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Commands.Handlers.Catalog
{
    [TestClass()]
    public class SendQuantityBelowStoreOwnerNotificationCommandHandlerTest
    {
        private Mock<IWorkflowMessageService> _workflowMessageServiceMock;
        private Mock<LocalizationSettings> _localizationSettingsMock;
        private SendQuantityBelowStoreOwnerNotificationCommandHandler _sendQuantityBelowStoreOwnerNotificationCommandHandler;
        private SendQuantityBelowStoreOwnerNotificationCommand _request;
        private SendQuantityBelowStoreOwnerNotificationCommand _requestWithoutPAC;

        [TestInitialize()]
        public void Init()
        {
            _workflowMessageServiceMock = new Mock<IWorkflowMessageService>();
            _localizationSettingsMock = new Mock<LocalizationSettings>();
            _sendQuantityBelowStoreOwnerNotificationCommandHandler = new SendQuantityBelowStoreOwnerNotificationCommandHandler(_workflowMessageServiceMock.Object, _localizationSettingsMock.Object);
            _request = new SendQuantityBelowStoreOwnerNotificationCommand 
            { 
                Product = new Product(), 
                ProductAttributeCombination = new ProductAttributeCombination()
            };
            _requestWithoutPAC = new SendQuantityBelowStoreOwnerNotificationCommand {
                Product = new Product()
            };
        }

        [TestMethod()]
        public async Task ProductAttributeCombinationNotNull()
        {
            await _sendQuantityBelowStoreOwnerNotificationCommandHandler.Handle(_request, default);
            _workflowMessageServiceMock.Verify(x => x.SendQuantityBelowStoreOwnerNotification(It.IsAny<Product>(), It.IsAny<ProductAttributeCombination>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task ProductAttributeCombinationIsNull()
        {
            await _sendQuantityBelowStoreOwnerNotificationCommandHandler.Handle(_requestWithoutPAC, default);
            _workflowMessageServiceMock.Verify(x => x.SendQuantityBelowStoreOwnerNotification(It.IsAny<Product>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task HandleResultReturnsTrue()
        {
            var result = await _sendQuantityBelowStoreOwnerNotificationCommandHandler.Handle(_request, default);
            Assert.IsTrue(result);
        }
    }
}
