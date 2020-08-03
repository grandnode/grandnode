using DotLiquid;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Services.Seo;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidKnowledgebase : Drop
    {
        private KnowledgebaseArticle _article;
        private KnowledgebaseArticleComment _articleComment;
        private Store _store;
        private Language _language;

        public LiquidKnowledgebase(KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, Store store, Language language)
        {
            _article = article;
            _articleComment = articleComment;
            _store = store;
            _language = language;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ArticleCommentTitle
        {
            get { return _article.Name; }
        }

        public string ArticleCommentUrl
        {
            get
            {
                return $"{(_store.SslEnabled ? _store.SecureUrl : _store.Url)}{_article.GetSeName(_language.Id)}";
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}