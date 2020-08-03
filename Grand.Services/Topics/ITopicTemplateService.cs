using Grand.Domain.Topics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Topics
{
    /// <summary>
    /// Topic template service interface
    /// </summary>
    public partial interface ITopicTemplateService
    {
        /// <summary>
        /// Delete topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        Task DeleteTopicTemplate(TopicTemplate topicTemplate);

        /// <summary>
        /// Gets all topic templates
        /// </summary>
        /// <returns>Topic templates</returns>
        Task<IList<TopicTemplate>> GetAllTopicTemplates();

        /// <summary>
        /// Gets a topic template
        /// </summary>
        /// <param name="topicTemplateId">Topic template identifier</param>
        /// <returns>Topic template</returns>
        Task<TopicTemplate> GetTopicTemplateById(string topicTemplateId);

        /// <summary>
        /// Inserts topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        Task InsertTopicTemplate(TopicTemplate topicTemplate);

        /// <summary>
        /// Updates the topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        Task UpdateTopicTemplate(TopicTemplate topicTemplate);
    }
}
