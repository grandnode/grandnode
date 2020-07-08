using Grand.Domain.Blogs;
using Grand.Services.Blogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Services.Tests.Blogs
{
    [TestClass()]
    public class BlogExtensionsTests
    {
        private List<BlogPost> _blogPosts;

        [TestInitialize()]
        public void Init()
        {
            _blogPosts = new List<BlogPost>()
            {
                new BlogPost(){StartDateUtc=DateTime.Now.AddDays(4),Id="5"},
                new BlogPost(){StartDateUtc=DateTime.Now.AddDays(3),Id="4"},
                new BlogPost(){StartDateUtc=DateTime.Now.AddDays(2),Id="3"},
                new BlogPost(){StartDateUtc=DateTime.Now.AddDays(1),Id="2"},
                new BlogPost(){StartDateUtc=DateTime.Now,Id="1"}
            };
        }

        [TestMethod()]
        public void GetPostsByDate_BlogPostContainsInDateRange()
        {
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(3);
            var result = _blogPosts.GetPostsByDate(from, to);
            Assert.AreEqual(4, result.Count());
            Assert.IsTrue(result.Any(b => b.Id.Equals("1")));
            Assert.IsTrue(result.Any(b => b.Id.Equals("2")));
            Assert.IsTrue(result.Any(b => b.Id.Equals("3")));
            Assert.IsTrue(result.Any(b => b.Id.Equals("4")));
        }

        [TestMethod()]
        public void GetPostsByDate_BlogPostsNotContainsInDateRange_ReturnEmptyList()
        {
            var from = DateTime.Now.AddDays(-3);
            var to = DateTime.Now.AddDays(-1);
            var result = _blogPosts.GetPostsByDate(from, to);
            Assert.AreEqual(0, result.Count());
        }
    }
}
