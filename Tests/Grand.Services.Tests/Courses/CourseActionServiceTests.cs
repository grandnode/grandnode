using Grand.Domain.Data;
using Grand.Domain.Courses;
using Grand.Core.Events;
using Grand.Services.Courses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Courses
{
    [TestClass()]
    public class CourseActionServiceTests
    {
        private Mock<IRepository<CourseAction>> _courseActionRepository;
        private Mock<IMediator> _mediatorMock;
        private CourseActionService _courseActionService;

        [TestInitialize()]
        public void Init()
        {
            _courseActionRepository = new Mock<IRepository<CourseAction>>();
            _mediatorMock = new Mock<IMediator>();
            _courseActionService = new CourseActionService(_courseActionRepository.Object, _mediatorMock.Object);
        }

        [TestMethod()]
        public void InsertAsync_NullArgument_ThrowExcepiton()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _courseActionService.InsertAsync(null), "courseAction");
        }

        [TestMethod()]
        public async Task  InsertAsync_NullArgument_InvokeRepository()
        {
            await _courseActionService.InsertAsync(new CourseAction());
            _courseActionRepository.Verify(c => c.InsertAsync(It.IsAny<CourseAction>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<CourseAction>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void Update_NullArgument_ThrowExcepiton()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _courseActionService.Update(null), "courseAction");
        }


        [TestMethod()]
        public async Task Update_ValidArgument_InvokeRepository()
        {
            await _courseActionService.Update(new CourseAction());
            _courseActionRepository.Verify(c => c.UpdateAsync(It.IsAny<CourseAction>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<CourseAction>>(), default(CancellationToken)), Times.Once);
        }
    }
}
