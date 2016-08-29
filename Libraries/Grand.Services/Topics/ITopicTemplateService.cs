using System.Collections.Generic;
using Grand.Core.Domain.Topics;

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
        void DeleteTopicTemplate(TopicTemplate topicTemplate);

        /// <summary>
        /// Gets all topic templates
        /// </summary>
        /// <returns>Topic templates</returns>
        IList<TopicTemplate> GetAllTopicTemplates();

        /// <summary>
        /// Gets a topic template
        /// </summary>
        /// <param name="topicTemplateId">Topic template identifier</param>
        /// <returns>Topic template</returns>
        TopicTemplate GetTopicTemplateById(string topicTemplateId);

        /// <summary>
        /// Inserts topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        void InsertTopicTemplate(TopicTemplate topicTemplate);

        /// <summary>
        /// Updates the topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        void UpdateTopicTemplate(TopicTemplate topicTemplate);
    }
}
