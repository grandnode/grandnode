using Grand.Domain.Blogs;
using Grand.Web.Models.Blogs;
using MediatR;

namespace Grand.Web.Commands.Models.Blogs
{
    public class InsertBlogCommentCommand : IRequest<BlogComment>
    {
        public BlogPostModel Model { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
