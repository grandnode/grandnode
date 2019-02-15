using Grand.Core.Domain.Blogs;
using Grand.Web.Models.Blogs;
using System.Collections.Generic;

namespace Grand.Web.Interfaces
{
    public partial interface IBlogViewModelService
    {
        HomePageBlogItemsModel PrepareHomePageBlogItems();
        void PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments);
        BlogCommentModel PrepareBlogPostCommentModel(BlogComment blogComment);
        BlogPostListModel PrepareBlogPostListModel(BlogPagingFilteringModel command);
        BlogPostTagListModel PrepareBlogPostTagListModel();
        List<BlogPostYearModel> PrepareBlogPostYearModel();
        List<BlogPostCategoryModel> PrepareBlogPostCategoryModel();
        BlogComment InsertBlogComment(BlogPostModel model, BlogPost blogPost);
    }
}