using DotLiquid;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Services.Seo;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidBlogComment : Drop
    {
        private BlogComment _blogComment;
        private BlogPost _blogPost;
        private Store _store;
        private readonly Language _language;
        public LiquidBlogComment(BlogComment blogComment, BlogPost blogPost, Store store, Language language)
        {
            this._blogComment = blogComment;
            this._blogPost = blogPost;
            this._store = store;
            this._language = language;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string BlogPostTitle
        {
            get { return _blogPost.Title; }
        }

        public string BlogPostURL
        {
            get
            {
                return $"{(_store.SslEnabled ? _store.SecureUrl : _store.Url)}{_blogPost.GetSeName(_language.Id)}";
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}