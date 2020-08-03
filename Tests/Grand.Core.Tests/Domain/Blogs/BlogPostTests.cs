using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Domain.Blogs.Tests
{
    [TestClass()]
    public class BlogPostTests {
        [TestMethod()]
        public void can_parse_tags() {
            BlogPost blogPost = new BlogPost { Tags = "tag1, tag2, tag 3 4,  t,a,g,23,2" };

            string[] expected = { "tag1", "tag2", "tag 3 4", "t", "a", "g", "23","2" };
            string[] actual = blogPost.ParseTags();

            for(int a = 0; a < expected.Length; ++a) {
                Assert.AreEqual(expected[a], actual[a]);
            }
        }
    }
}