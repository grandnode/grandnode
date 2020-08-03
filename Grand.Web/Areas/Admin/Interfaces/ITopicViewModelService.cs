using Grand.Domain.Topics;
using Grand.Web.Areas.Admin.Models.Topics;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ITopicViewModelService
    {
        Task<TopicListModel> PrepareTopicListModel();
        Task PrepareTemplatesModel(TopicModel model);
        Task<Topic> InsertTopicModel(TopicModel model);
        Task<Topic> UpdateTopicModel(Topic topic, TopicModel model);
        Task DeleteTopic(Topic topic);
    }
}
