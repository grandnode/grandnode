using Grand.Core.Domain.Topics;
using Grand.Web.Areas.Admin.Models.Topics;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ITopicViewModelService
    {
        TopicListModel PrepareTopicListModel();
        void PrepareTemplatesModel(TopicModel model);
        Topic InsertTopicModel(TopicModel model);
        Topic UpdateTopicModel(Topic topic, TopicModel model);
        void DeleteTopic(Topic topic);
    }
}
