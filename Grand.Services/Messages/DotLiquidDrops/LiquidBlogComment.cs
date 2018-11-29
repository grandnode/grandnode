using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Blogs;
using Grand.Core.Infrastructure;
using Grand.Services.Blogs;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBlogComment : Drop
    {
        private BlogComment _blogComment;
        private string _storeId;

        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public LiquidBlogComment(IStoreContext storeContext,
            IStoreService storeService)
        {
            this._storeContext = storeContext;
            this._storeService = storeService;
        }

        public void SetProperties(BlogComment blogComment, string storeId)
        {
            this._blogComment = blogComment;
            this._storeId = storeId;
        }

        public string BlogPostTitle
        {
            get { return EngineContext.Current.Resolve<IBlogService>().GetBlogPostById(_blogComment.BlogPostId).Title; }
        }

        public string BlogPostURL
        {
            get
            {
                var blogPost = EngineContext.Current.Resolve<IBlogService>().GetBlogPostById(_blogComment.BlogPostId);
                return $"{GetStoreUrl(_storeId)}{blogPost.GetSeName()}";
            }
        }

        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="useSsl">Use SSL</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(string storeId = "", bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return useSsl ? store.SecureUrl : store.Url;
        }
    }
}