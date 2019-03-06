using DotLiquid;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Infrastructure;
using Grand.Services.Knowledgebase;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidKnowledgebase : Drop
    {
        private KnowledgebaseArticleComment _articleComment;
        private string _storeId;

        private readonly IStoreService _storeService;

        public LiquidKnowledgebase(KnowledgebaseArticleComment articleComment, string storeId)
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._articleComment = articleComment;
            this._storeId = storeId;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ArticleCommentTitle
        {
            get { return EngineContext.Current.Resolve<IKnowledgebaseService>().GetPublicKnowledgebaseArticle(_articleComment.ArticleId).Name; }
        }

        public string ArticleCommentUrl
        {
            get
            {
                var article = EngineContext.Current.Resolve<IKnowledgebaseService>().GetPublicKnowledgebaseArticle(_articleComment.ArticleId);
                return $"{_storeService.GetStoreUrl(_storeId)}{article.GetSeName()}";
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}