using DotLiquid;
using Grand.Core.Domain.Blogs;
using Grand.Core.Infrastructure;
using Grand.Services.Blogs;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBlogComment : Drop
    {
        private BlogComment _blogComment;
        private string _storeId;
        private readonly IStoreService _storeService;

        public LiquidBlogComment(BlogComment blogComment, string storeId)
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._blogComment = blogComment;
            this._storeId = storeId;

            AdditionalTokens = new Dictionary<string, string>();
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
                return $"{_storeService.GetStoreUrl(_storeId)}{blogPost.GetSeName()}";
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}