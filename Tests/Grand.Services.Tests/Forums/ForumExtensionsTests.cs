using Grand.Core;
using Grand.Domain;
using Grand.Domain.Forums;
using Grand.Services.Forums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Forums
{
    [TestClass()]
    public class ForumExtensionsTests
    {
        [TestMethod()]
        public void StripTopicSubject()
        {
            var forumTopic = new ForumTopic();
            var settings = new ForumSettings() { StrippedTopicMaxLength = 8 };
            forumTopic.Subject = "Sample topic name";
            var expected = "Sample topic...";
            Assert.IsTrue(forumTopic.StripTopicSubject(settings).Equals(expected));
        }

        [TestMethod()]
        public async Task GetFirstPost_InvokeRepository()
        {
            var forumTopic = new ForumTopic();
            forumTopic.Id = "1";
            var forumServiceMock = new Mock<IForumService>();
            forumServiceMock.Setup(c => c.GetAllPosts(forumTopic.Id, "", string.Empty, 0, 1)).Returns(() => Task.FromResult(new Mock<IPagedList<ForumPost>>().Object));
            await forumTopic.GetFirstPost(forumServiceMock.Object);
            forumServiceMock.Verify(f => f.GetAllPosts(forumTopic.Id, "", string.Empty, 0, 1),Times.Once);
        }

        [TestMethod()]
        public void GetFirstPost_NullTopic_ThrowException()
        {
            ForumTopic forumTopic = null;
            var forumServiceMock = new Mock<IForumService>();
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async ()=>await forumTopic.GetFirstPost(forumServiceMock.Object), "forumTopic");
        }
    }
}
