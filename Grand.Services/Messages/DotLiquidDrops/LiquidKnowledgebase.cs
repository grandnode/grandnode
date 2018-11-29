using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Infrastructure;
using Grand.Services.Knowledgebase;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidKnowledgebase : Drop
    {
        private KnowledgebaseArticleComment _articleComment;
        private string _storeId;

        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public LiquidKnowledgebase(IStoreContext storeContext,
            IStoreService storeService)
        {
            this._storeContext = storeContext;
            this._storeService = storeService;
        }

        public void SetProperties(KnowledgebaseArticleComment articleComment, string storeId)
        {
            this._articleComment = articleComment;
            this._storeId = storeId;
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
                return $"{GetStoreUrl(_storeId)}{article.GetSeName()}";
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