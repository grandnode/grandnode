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

        public LiquidForums(Forum forum,
            ForumTopic forumTopic,
            ForumPost forumPost,
            int? friendlyForumTopicPageIndex = null,
            string appendedPostIdentifierAnchor = "")
        {
            this._storeService = EngineContext.Current.Resolve<IStoreService>();

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
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", _storeService.GetStoreUrl(), _forumTopic.Id, _forumTopic.GetSeName(), _friendlyForumTopicPageIndex.Value);
                else
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}", _storeService.GetStoreUrl(), _forumTopic.Id, _forumTopic.GetSeName());
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
            get { return string.Format("{0}boards/forum/{1}/{2}", _storeService.GetStoreUrl(), _forum.Id, _forum.GetSeName()); }
        }

        public string ForumName
        {
            get { return _forum.Name; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}