using DotLiquid;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Stores;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Seo;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidForums : Drop
    {
        private ForumTopic _forumTopic;
        private ForumPost _forumPost;
        private Forum _forum;
        private Customer _customer;
        private Store _store;

        private int? _friendlyForumTopicPageIndex;
        private string _appendedPostIdentifierAnchor;

        public LiquidForums(Forum forum,
            ForumTopic forumTopic,
            ForumPost forumPost,
            Customer customer,
            Store store,
            int? friendlyForumTopicPageIndex = null,
            string appendedPostIdentifierAnchor = "")
        {
            _forumTopic = forumTopic;
            _forumPost = forumPost;
            _forum = forum;
            _friendlyForumTopicPageIndex = friendlyForumTopicPageIndex;
            _appendedPostIdentifierAnchor = appendedPostIdentifierAnchor;
            _customer = customer;
            _store = store;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string TopicURL
        {
            get
            {
                string topicUrl;
                if (_friendlyForumTopicPageIndex.HasValue && _friendlyForumTopicPageIndex.Value > 1)
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _forumTopic.Id, _forumTopic.GetSeName(), _friendlyForumTopicPageIndex.Value);
                else
                    topicUrl = string.Format("{0}boards/topic/{1}/{2}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _forumTopic.Id, _forumTopic.GetSeName());
                if (!String.IsNullOrEmpty(_appendedPostIdentifierAnchor))
                    topicUrl = string.Format("{0}#{1}", topicUrl, _appendedPostIdentifierAnchor);

                return topicUrl;
            }
        }

        public string PostAuthor
        {
            get
            {
                return _customer.FormatUserName(CustomerNameFormat.ShowFullNames);
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
            get { return string.Format("{0}boards/forum/{1}/{2}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _forum.Id, _forum.GetSeName()); }
        }

        public string ForumName
        {
            get { return _forum.Name; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}