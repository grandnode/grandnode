using DotLiquid;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Stores;
using Grand.Services.Seo;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidNewsComment : Drop
    {
        private NewsComment _newsComment;
        private NewsItem _newsItem;
        private Store _store;
        private Language _language;

        public LiquidNewsComment(NewsItem newsItem, NewsComment newsComment, Store store, Language language)
        {
            this._newsComment = newsComment;
            this._newsItem = newsItem;
            this._store = store;
            this._language = language;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string NewsTitle
        {
            get { return _newsItem.Title; }
        }

        public string CommentText
        {
            get { return _newsComment.CommentText; }
        }

        public string CommentTitle
        {
            get { return _newsComment.CommentTitle; }
        }

        public string NewsURL
        {
            get { return $"{(_store.SslEnabled ? _store.SecureUrl : _store.Url)}{_newsItem.GetSeName(_language.Id)}"; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}