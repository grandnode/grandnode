using Grand.Core.Configuration;

namespace Grand.Core.Domain.Knowledgebase
{
    public class KnowledgebaseSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether knowledgebase is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not registered user can leave comments
        /// </summary>
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify about new article comments
        /// </summary>
        public bool NotifyAboutNewArticleComments { get; set; }
    }
}
