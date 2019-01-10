using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Forums;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Seo;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidForums : Drop
    {
        private ForumTopic _forumTopic;
        private ForumPost _forumPost;
        private Forum _forum;
        private int? _friendlyForumTopicPageIndex;
        private string _appendedPostIdentifierAnchor;

        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;

        public LiquidForums(Forum forum,
            ForumTopic forumTopic,
            ForumPost forumPost,
            int? friendlyForumTopicPageIndex = null,
            string appendedPostIdentifierAnchor = "")
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._storeContext = EngineContext.Current.Resolve<IStoreContext>();

            this._forumTopic = forumTopic;
            this._forumPost = forumPost;
            this._forum = forum;
            this._friendlyForumTopicPageIndex = friendlyForumTopicPageIndex;
            this._appendedPostIdentifierAnchor = appendedPostIdentifierAnchor;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string TopicURL
        {
            get
            {
                string topicUrl;
                if (_friendlyForumTopicPageIndex.HasValue && _friendlyForumTopicPageIndex.Value > 1)
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", GetStoreUrl(), _forumTopic.Id, _forumTopic.GetSeName(), _friendlyForumTopicPageIndex.Value);
                else
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}", GetStoreUrl(), _forumTopic.Id, _forumTopic.GetSeName());
                if (!String.IsNullOrEmpty(_appendedPostIdentifierAnchor))
                    topicUrl = string.Format("{0}#{1}", topicUrl, _appendedPostIdentifierAnchor);

                return topicUrl;
            }
        }

        public string PostAuthor
        {
            get
            {
                var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(_forumPost.CustomerId);
                return customer.FormatUserName();
            }
        }

        public string PostBody
        {
            get
            {
                return _forumPost.FormatPostText();
            }
        }

        public string TopicName
        {
            get { return _forumTopic.Subject; }
        }

        public string ForumURL
        {
            get { return string.Format("{0}boards/forum/{1}/{2}", GetStoreUrl(), _forum.Id, _forum.GetSeName()); }
        }

        public string ForumName
        {
            get { return _forum.Name; }
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}