using Grand.Core.Domain.Topics;
using Grand.Web.Models.Topics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface ITopicWebService
    {
        TopicModel PrepareTopicModel(Topic topic);
        TopicModel TopicDetails(string topicId);
        TopicModel TopicDetailsPopup(string systemName);
        TopicModel TopicBlock(string systemName);
        string PrepareTopicTemplateViewPath(string templateId);
    }
}