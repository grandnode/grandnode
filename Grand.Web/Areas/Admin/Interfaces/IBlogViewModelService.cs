using Grand.Core.Domain.Blogs;
using Grand.Web.Areas.Admin.Models.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IBlogViewModelService
    {
        Task<BlogPostModel> PrepareBlogPostModel();
        Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel);
        Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost);
        Task<BlogPostModel> PrepareBlogPostModel(BlogPost blogPost);
        Task<(IEnumerable<BlogPostModel> blogPosts, int totalCount)> PrepareBlogPostsModel(int pageIndex, int pageSize);
        Task<BlogPost> InsertBlogPostModel(BlogPostModel model);
        Task<BlogPost> UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost);
        Task<(IEnumerable<BlogCommentModel> blogComments, int totalCount)> PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize);
    }
}
