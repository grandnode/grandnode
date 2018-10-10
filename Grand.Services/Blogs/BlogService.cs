using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Blogs
{
    /// <summary>
    /// Blog service
    /// </summary>
    public partial class BlogService : IBlogService
    {
        #region Fields

        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public BlogService(IRepository<BlogPost> blogPostRepository,
            IRepository<BlogComment> blogCommentRepository,
            CatalogSettings catalogSettings, 
            IEventPublisher eventPublisher)
        {
            this._blogPostRepository = blogPostRepository;
            this._blogCommentRepository = blogCommentRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void DeleteBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Delete(blogPost);

            //event notification
            _eventPublisher.EntityDeleted(blogPost);
        }

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostId">Blog post identifier</param>
        /// <returns>Blog post</returns>
        public virtual BlogPost GetBlogPostById(string blogPostId)
        {
            return _blogPostRepository.GetById(blogPostId);
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="blogPostName">Blog post name</param>
        /// <returns>Blog posts</returns>
        public virtual IPagedList<BlogPost> GetAllBlogPosts(string storeId = "", 
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string tag = null, string blogPostName = "")
        {

            var query = _blogPostRepository.Table;

            if (!String.IsNullOrWhiteSpace(blogPostName))
            {
                query = query.Where
                    (b => (b.Title != null && b.Title.ToLower().Contains(blogPostName.ToLower())) || 
                    (b.BodyOverview != null && b.BodyOverview.ToLower().Contains(blogPostName.ToLower())));
            }
            if (dateFrom.HasValue)
                query = query.Where(b => dateFrom.Value <= b.CreatedOnUtc);
            if (dateTo.HasValue)
                query = query.Where(b => dateTo.Value >= b.CreatedOnUtc);

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(b => !b.StartDateUtc.HasValue || b.StartDateUtc <= utcNow);
                query = query.Where(b => !b.EndDateUtc.HasValue || b.EndDateUtc >= utcNow);
            }

            if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = query.Where(b => b.Stores.Contains(storeId) || !b.LimitedToStores);
            }
            if(!(String.IsNullOrEmpty(tag)))
            {
                query = query.Where(x => x.Tags.Contains(tag));
            }
            query = query.OrderByDescending(b => b.CreatedOnUtc);

            var blogPosts = new PagedList<BlogPost>(query, pageIndex, pageSize);
            return blogPosts;

        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all news</param>
        /// <param name="tag">Tag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog posts</returns>
        public virtual IPagedList<BlogPost> GetAllBlogPostsByTag(string storeId = "",
            string tag = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            tag = tag.Trim();

            //we load all records and only then filter them by tag
            var blogPostsAll = GetAllBlogPosts(storeId: storeId, showHidden: showHidden, tag:tag);
            var taggedBlogPosts = new List<BlogPost>();
            foreach (var blogPost in blogPostsAll)
            {
                var tags = blogPost.ParseTags();
                if (!String.IsNullOrEmpty(tags.FirstOrDefault(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase))))
                    taggedBlogPosts.Add(blogPost);
            }

            //server-side paging
            var result = new PagedList<BlogPost>(taggedBlogPosts, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// Gets all blog post tags
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all news</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog post tags</returns>
        public virtual IList<BlogPostTag> GetAllBlogPostTags(string storeId, bool showHidden = false)
        {
            var blogPostTags = new List<BlogPostTag>();

            var blogPosts = GetAllBlogPosts(storeId: storeId, showHidden: showHidden);
            foreach (var blogPost in blogPosts)
            {
                var tags = blogPost.ParseTags();
                foreach (string tag in tags)
                {
                    var foundBlogPostTag = blogPostTags.Find(bpt => bpt.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                    if (foundBlogPostTag == null)
                    {
                        foundBlogPostTag = new BlogPostTag
                        {
                            Name = tag,
                            BlogPostCount = 1
                        };
                        blogPostTags.Add(foundBlogPostTag);
                    }
                    else
                        foundBlogPostTag.BlogPostCount++;
                }
            }

            return blogPostTags;
        }

        /// <summary>
        /// Inserts an blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void InsertBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Insert(blogPost);

            //event notification
            _eventPublisher.EntityInserted(blogPost);
        }

        /// <summary>
        /// Inserts a blog post comment
        /// </summary>
        /// <param name="blogPost">Blog post comment</param>
        public virtual void InsertBlogComment(BlogComment blogComment)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogPost");

            _blogCommentRepository.Insert(blogComment);

            //event notification
            _eventPublisher.EntityInserted(blogComment);
        }

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void UpdateBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Update(blogPost);

            //event notification
            _eventPublisher.EntityUpdated(blogPost);
        }
        
        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <returns>Comments</returns>
        public virtual IList<BlogComment> GetAllComments(string customerId)
        {
            var query = from c in _blogCommentRepository.Table
                        orderby c.CreatedOnUtc
                        where (customerId == "" || c.CustomerId == customerId)
                        select c;
            var content = query.ToList();
            return content;
        }

        /// <summary>
        /// Gets a blog comment
        /// </summary>
        /// <param name="blogCommentId">Blog comment identifier</param>
        /// <returns>Blog comment</returns>
        public virtual BlogComment GetBlogCommentById(string blogCommentId)
        {
            return _blogCommentRepository.GetById(blogCommentId);
        }

        public virtual IList<BlogComment> GetBlogCommentsByBlogPostId(string blogPostId)
        {
            var query = from c in _blogCommentRepository.Table
                        where c.BlogPostId == blogPostId
                        orderby c.CreatedOnUtc
                        select c;
            var content = query.ToList();
            return content;
        }

        /// Get blog comments by identifiers
        /// </summary>
        /// <param name="commentIds">Blog comment identifiers</param>
        /// <returns>Blog comments</returns>
        public virtual IList<BlogComment> GetBlogCommentsByIds(string[] commentIds)
        {
            if (commentIds == null || commentIds.Length == 0)
                return new List<BlogComment>();

            var query = from bc in _blogCommentRepository.Table
                        where commentIds.Contains(bc.Id)
                        select bc;
            var comments = query.ToList();
            //sort by passed identifiers
            var sortedComments = new List<BlogComment>();
            foreach (string id in commentIds)
            {
                var comment = comments.Find(x => x.Id == id);
                if (comment != null)
                    sortedComments.Add(comment);
            }
            return sortedComments;
        }

        public virtual void DeleteBlogComment(BlogComment blogComment)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogComment");

            _blogCommentRepository.Delete(blogComment);
        }
        #endregion
    }
}
