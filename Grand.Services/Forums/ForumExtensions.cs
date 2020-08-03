using Grand.Domain.Forums;
using Grand.Core.Html;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Forums
{
    public static class ForumExtensions
    {
        /// <summary>
        /// Formats the forum post text
        /// </summary>
        /// <param name="forumPost">Forum post</param>
        /// <returns>Formatted text</returns>
        public static string FormatPostText(this ForumPost forumPost)
        {
            string text = forumPost.Text;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, true, false, true);

            return text;
        }

        /// <summary>
        /// Strips the topic subject
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        /// <returns>Formatted subject</returns>
        public static string StripTopicSubject(this ForumTopic forumTopic, ForumSettings forumSettings)
        {
            string subject = forumTopic.Subject;
            if (String.IsNullOrEmpty(subject))
            {
                return subject;
            }

            int strippedTopicMaxLength = forumSettings.StrippedTopicMaxLength;
            if (strippedTopicMaxLength > 0)
            {
                if (subject.Length > strippedTopicMaxLength)
                {
                    int index = subject.IndexOf(" ", strippedTopicMaxLength);
                    if (index > 0)
                    {
                        subject = subject.Substring(0, index);
                        subject += "...";
                    }
                }
            }

            return subject;
        }

        /// <summary>
        /// Formats the forum signature text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public static string FormatForumSignatureText(this string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);
            return text;
        }

        /// <summary>
        /// Formats the private message text
        /// </summary>
        /// <param name="pm">Private message</param>
        /// <returns>Formatted text</returns>
        public static string FormatPrivateMessageText(this PrivateMessage pm)
        {
            string text = pm.Text;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, true, false, false);

            return text;
        }
        
        /// <summary>
        /// Get first post
        /// </summary>
        /// <param name="forumTopic">Forum topic</param>
        /// <param name="forumService">Forum service</param>
        /// <returns>Forum post</returns>
        public static async Task<ForumPost> GetFirstPost(this ForumTopic forumTopic, IForumService forumService)
        {
            if (forumTopic == null)
                throw new ArgumentNullException("forumTopic");

            var forumPosts = await forumService.GetAllPosts(forumTopic.Id, "", string.Empty, 0, 1);
            if (forumPosts.Count > 0)
                return forumPosts[0];

            return null;
        }
    }
}
