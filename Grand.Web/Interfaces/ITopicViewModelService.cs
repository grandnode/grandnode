using Grand.Core.Domain.Topics;
using Grand.Web.Models.Topics;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ITopicViewModelService
    {
        TopicModel PrepareTopicModel(Topic topic);
        Task<TopicModel> TopicDetails(string topicId);
        Task<TopicModel> TopicDetailsPopup(string systemName);
        Task<TopicModel> TopicBlock(string systemName);
        Task<string> PrepareTopicTemplateViewPath(string templateId);
    }
}