using System;
using System.Collections.Generic;
using Grand.Core;
using Grand.Core.Domain.Blogs;

namespace Grand.Services.Blogs
{
    /// <summary>
    /// Blog service interface
    /// </summary>
    public partial interface IBlogService
    {
        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        void DeleteBlogPost(BlogPost blogPost);

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostId">Blog post identifier</param>
        /// <returns>Blog post</returns>
        BlogPost GetBlogPostById(string blogPostId);

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog posts</returns>
        IPagedList<BlogPost> GetAllBlogPosts(string storeId = "", 
            DateTime? dateFrom = null, DateTime? dateTo = null, 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string tag = null);

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="tag">Tag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog posts</returns>
        IPagedList<BlogPost> GetAllBlogPostsByTag(string storeId = "",
            string tag = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all blog post tags
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog post tags</returns>
        IList<BlogPostTag> GetAllBlogPostTags(string storeId, bool showHidden = false);

        /// <summary>
        /// Inserts an blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        void InsertBlogPost(BlogPost blogPost);

        /// <summary>
        /// Inserts an blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        void InsertBlogComment(BlogComment blogComment);

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        void UpdateBlogPost(BlogPost blogPost);

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        IList<BlogComment> GetAllComments(string customerId);

        /// <summary>
        /// Gets a blog comment
        /// </summary>
        /// <param name="blogCommentId">Blog comment identifier</param>
        /// <returns>Blog comment</returns>
        BlogComment GetBlogCommentById(string blogCommentId);

        /// <summary>
        /// Get blog comments by identifiers
        /// </summary>
        /// <param name="commentIds">Blog comment identifiers</param>
        /// <returns>Blog comments</returns>
        IList<BlogComment> GetBlogCommentsByIds(string[] commentIds);

        IList<BlogComment> GetBlogCommentsByBlogPostId(string blogPostId);
        void DeleteBlogComment(BlogComment blogComment);

    }
}
