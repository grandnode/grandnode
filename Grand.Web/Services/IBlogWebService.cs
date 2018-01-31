using Grand.Core.Domain.Blogs;
using Grand.Web.Models.Blogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface IBlogWebService
    {
        HomePageBlogItemsModel PrepareHomePageBlogItems();
        void PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments);
        BlogCommentModel PrepareBlogPostCommentModel(BlogComment blogComment);
        BlogPostListModel PrepareBlogPostListModel(BlogPagingFilteringModel command);
        BlogPostTagListModel PrepareBlogPostTagListModel();
        List<BlogPostYearModel> PrepareBlogPostYearModel();
        BlogComment InsertBlogComment(BlogPostModel model, BlogPost blogPost);
    }
}