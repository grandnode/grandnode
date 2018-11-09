using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.News;
using Grand.Core.Infrastructure;
using Grand.Services.News;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidNewsComment : Drop
    {
        private readonly NewsComment _newsComment;
        private readonly NewsItem _newsItem;

        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public LiquidNewsComment(NewsComment newsComment,
            NewsItem newsItem,
            IStoreContext storeContext,
            IStoreService storeService)
        {
            this._newsComment = newsComment;
            this._newsItem = EngineContext.Current.Resolve<INewsService>().GetNewsById(newsComment.NewsItemId);
            this._storeContext = storeContext;
            this._storeService = storeService;
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
            get { return $"{GetStoreUrl(_storeContext.CurrentStore.Id)}{_newsItem.GetSeName()}"; }
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