using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.News;
using Grand.Core.Infrastructure;
using Grand.Services.News;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidNewsComment : Drop
    {
        private NewsComment _newsComment;
        private NewsItem _newsItem;
        private string _storeId;

        private readonly IStoreService _storeService;

        public LiquidNewsComment(NewsComment newsComment, string storeId)
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._newsComment = newsComment;
            this._newsItem = EngineContext.Current.Resolve<INewsService>().GetNewsById(newsComment.NewsItemId);
            this._storeId = storeId;
                       
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
            get { return $"{_storeService.GetStoreUrl(_storeId)}{_newsItem.GetSeName()}"; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}