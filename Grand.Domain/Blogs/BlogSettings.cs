using Grand.Domain.Configuration;

namespace Grand.Domain.Blogs
{
    public class BlogSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether blog is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the page size for posts
        /// </summary>
        public int PostsPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not registered user can leave comments
        /// </summary>
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify about new blog comments
        /// </summary>
        public bool NotifyAboutNewBlogComments { get; set; }

        /// <summary>
        /// Gets or sets a number of blog tags that appear in the tag cloud
        /// </summary>
        public int NumberOfTags { get; set; }

        /// <summary>
        /// Enable the blog RSS feed link in customers browser address bar
        /// </summary>
        public bool ShowHeaderRssUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show blog on the home page
        /// </summary>
        public bool ShowBlogOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating blog count displayed on the home page
        /// </summary>
        public int HomePageBlogCount { get; set; }

        /// <summary>
        /// Gets or sets a value the maximum size of the text on home page
        /// </summary>
        public int MaxTextSizeHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show blog posts in the auto complete search
        /// </summary>
        public bool ShowBlogPostsInSearchAutoComplete { get; set; }

    }
}