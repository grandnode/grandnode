using Grand.Web.Models.Blogs;
using MediatR;

namespace Grand.Web.Features.Models.Blogs
{
    public class GetBlogPostList: IRequest<BlogPostListModel>
    {
        public GetBlogPostList()
        {
            Command = new BlogPagingFilteringModel();
        }
        public BlogPagingFilteringModel Command { get; set; }
    }
}
