using Grand.Domain.Topics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Topics
{
    /// <summary>
    /// Topic service interface
    /// </summary>
    public partial interface ITopicService
    {
        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        Task DeleteTopic(Topic topic);

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        Task<Topic> GetTopicById(string topicId);

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="storeId">Store identifier; pass 0 to ignore filtering by store and load the first one</param>
        /// <returns>Topic</returns>
        Task<Topic> GetTopicBySystemName(string systemName, string storeId = "");

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <returns>Topics</returns>
        Task<IList<Topic>> GetAllTopics(string storeId, bool ignorAcl = false);

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        Task InsertTopic(Topic topic);

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        Task UpdateTopic(Topic topic);
    }
}
