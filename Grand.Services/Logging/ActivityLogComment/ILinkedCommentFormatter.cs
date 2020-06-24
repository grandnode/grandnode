using System.Threading.Tasks;
using Grand.Core.Domain.Logging;

namespace Grand.Services.Logging.ActivityLogComment
{
    /// <summary>
    /// Activity log comment formatter interface
    /// </summary>
    public interface ILinkedCommentFormatter
    {
        /// <summary>
        /// Parse activity log comment's plain text and returns comment's text, containing hyperlink to the changed object instead of the object name
        /// </summary>
        /// <param name="activityLog"></param>
        /// <returns></returns>
        Task<string> AddLinkToPlainComment(ActivityLog activityLog);
    }
}