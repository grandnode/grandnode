using Grand.Core.Domain.Blogs;
using Grand.Web.Models.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IBlogViewModelService
    {
        Task<HomePageBlogItemsModel> PrepareHomePageBlogItems();
        Task PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments);
        Task<BlogCommentModel> PrepareBlogPostCommentModel(BlogComment blogComment);
        Task<BlogPostListModel> PrepareBlogPostListModel(BlogPagingFilteringModel command);
        Task<BlogPostTagListModel> PrepareBlogPostTagListModel();
        Task<List<BlogPostYearModel>> PrepareBlogPostYearModel();
        Task<List<BlogPostCategoryModel>> PrepareBlogPostCategoryModel();
        Task<BlogComment> InsertBlogComment(BlogPostModel model, BlogPost blogPost);
    }
}