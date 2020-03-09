using Grand.Core.Domain.Blogs;
using Grand.Web.Models.Blogs;
using MediatR;

namespace Grand.Web.Commands.Models
{
    public class InsertBlogCommentCommandModel : IRequest<BlogComment>
    {
        public BlogPostModel Model { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
