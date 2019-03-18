using Grand.Core.Domain.Blogs;
using Grand.Web.Areas.Admin.Models.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IBlogViewModelService
    {
        BlogPostModel PrepareBlogPostModel();
        BlogPostModel PrepareBlogPostModel(BlogPostModel blogPostmodel);
        BlogPostModel PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost);
        BlogPostModel PrepareBlogPostModel(BlogPost blogPost);
        Task<(IEnumerable<BlogPostModel> blogPosts, int totalCount)> PrepareBlogPostsModel(int pageIndex, int pageSize);
        BlogPost InsertBlogPostModel(BlogPostModel model);
        BlogPost UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost);
        Task<(IEnumerable<BlogCommentModel> blogComments, int totalCount)> PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize);
    }
}
