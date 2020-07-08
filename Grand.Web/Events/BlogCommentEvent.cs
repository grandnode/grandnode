using Grand.Domain.Blogs;
using Grand.Web.Models.Blogs;
using MediatR;

namespace Grand.Web.Events
{
    public class BlogCommentEvent : INotification
    {
        public BlogPost BlogPost { get; private set; }
        public AddBlogCommentModel BlogComment { get; private set; }
        public BlogCommentEvent(BlogPost blogPost, AddBlogCommentModel blogComment)
        {
            BlogPost = blogPost;
            BlogComment = blogComment;
        }
    }
}
