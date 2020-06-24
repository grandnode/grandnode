using System;
using System.Linq;
using System.Threading.Tasks;
using Grand.Core.Domain.Logging;
using Grand.Services.Localization;

namespace Grand.Services.Logging.ActivityLogComment
{
    /// <summary>
    /// Activity log comment formatter
    /// </summary>
    public class LinkedCommentFormatter : ILinkedCommentFormatter
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IActivityEntityKeywordsProvider _activityEntityKeywords;

        public LinkedCommentFormatter(ICustomerActivityService customerActivityService, 
            ILocalizationService localizationService, 
            IActivityEntityKeywordsProvider activityEntityKeywords)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _activityEntityKeywords = activityEntityKeywords;
        }

        /// <summary>
        /// Parse activity log comment's plain text and returns comment's text, containing hyperlink to the changed object instead of the object name
        /// </summary>
        /// <param name="activityLog"></param>
        /// <returns></returns>
        public virtual async Task<string> AddLinkToPlainComment(ActivityLog activityLog)
        {
            if (CommentAlreadyFormattedWithLinks(activityLog.Comment)) 
                return activityLog.Comment;

            var activityKeyword = await GetActivityKeyword(activityLog.ActivityLogTypeId);
            var activityLogEntity = _activityEntityKeywords.GetLogEntity(activityKeyword);
            if (activityLogEntity == null) 
                return activityLog.Comment;

            return TryChangeEntityNameToLink(activityLog.EntityKeyId, activityKeyword, activityLog.Comment,
                activityLogEntity, out string formattedComment) 
                ? formattedComment 
                : activityLog.Comment;
        }

        private bool TryChangeEntityNameToLink(
            string entityKeyId, 
            string activityKeyword, 
            string comment, 
            ActivityLogEntity activityLogEntity, 
            out string linkedComment)
        {
            var commentBase = _localizationService.GetResource("ActivityLog." + activityKeyword);

            if (!TryParseSingleCommentParameter(comment, commentBase, out string commentParameter))
            {
                linkedComment = string.Empty;
                return false;
            }
            
            linkedComment = LinkedCommentHelper.GenerateLinkedComment(entityKeyId, activityLogEntity.LinkPattern, commentParameter, commentBase);

            return true;
        }


        private async Task<string> GetActivityKeyword(string activityLogTypeId)
        {
            var activityType = await _customerActivityService.GetActivityTypeById(activityLogTypeId);
            return activityType?.SystemKeyword;
        }

        private static bool CommentAlreadyFormattedWithLinks(string comment)
        {
            return comment.Contains("<a", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseSingleCommentParameter(string comment, string commentBase, out string commentParameter)
        {
            string[] logParameters = comment.ParseExact(commentBase);

            //We can change only 1 entity name to the link
            if (logParameters.Length == 1)
            {
                commentParameter = logParameters.First();
                return true;
            }

            commentParameter = string.Empty;
            return false;
        }
    }
}
