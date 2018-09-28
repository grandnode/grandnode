using Grand.Core.Domain.Topics;
using Grand.Web.Models.Topics;

namespace Grand.Web.Services
{
    public partial interface ITopicViewModelService
    {
        TopicModel PrepareTopicModel(Topic topic);
        TopicModel TopicDetails(string topicId);
        TopicModel TopicDetailsPopup(string systemName);
        TopicModel TopicBlock(string systemName);
        string PrepareTopicTemplateViewPath(string templateId);
    }
}