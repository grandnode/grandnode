using Grand.Services.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grand.Services.Commands.Handlers.Catalog;
using System;
using System.Collections.Generic;
using System.Text;
using Grand.Services.Commands.Models.Catalog;
using System.Threading.Tasks;
using Grand.Domain.Catalog;
using FluentValidation.Resources;

namespace Grand.Services.Tests.Commands.Handlers.Catalog
{
    [TestClass()]
    public class SendOutBidCustomerNotificationCommandHandlerTest
    {
        private Mock<IWorkflowMessageService> _workflowMessageServiceMock;
        private SendOutBidCustomerNotificationCommandHandler _sendOutBidCustomerNotificationCommandHandler;
        private SendOutBidCustomerNotificationCommand _sendOutBidCustomerNotificationCommand;

        [TestInitialize()]
        public void Init()
        {
            _workflowMessageServiceMock = new Mock<IWorkflowMessageService>();
            _workflowMessageServiceMock.Setup(x => x.SendOutBidCustomerNotification(It.IsAny<Product>(), It.IsAny<string>(), It.IsAny<Bid>())).Returns(Task.FromResult(1));
            _sendOutBidCustomerNotificationCommandHandler = new SendOutBidCustomerNotificationCommandHandler(_workflowMessageServiceMock.Object);
            _sendOutBidCustomerNotificationCommand = new SendOutBidCustomerNotificationCommand { 
                Product = new Product(), 
                Language = new Domain.Localization.Language(), 
                Bid = new Bid()};
        }
        [TestMethod()]
        public async Task CallSendOutBidCustomerNotification()
        {
            await _sendOutBidCustomerNotificationCommandHandler.Handle(_sendOutBidCustomerNotificationCommand, default);
            _workflowMessageServiceMock.Verify(x => x.SendOutBidCustomerNotification(It.IsAny<Product>(), It.IsAny<string>(), It.IsAny<Bid>()), Times.Once());
        }
    }
}
