using System.Linq;

namespace Grand.Services.Logging.ActivityLogComment
{
    /// <summary>
    /// Activity log linked comment creator
    /// </summary>
    public class LinkedCommentCreator : ILinkedCommentCreator
    {

        private readonly IActivityEntityKeywordsProvider _activityEntityKeywords;

        public LinkedCommentCreator(IActivityEntityKeywordsProvider activityEntityKeywords)
        {
            _activityEntityKeywords = activityEntityKeywords;
        }

        /// <summary>
        /// Create activity log comment's text, containing hyperlink to the changed object
        /// </summary>
        /// <param name="systemKeyword"></param>
        /// <param name="entityKeyId"></param>
        /// <param name="commentBase"></param>
        /// <param name="commentParams"></param>
        /// <returns></returns>
        public virtual string CreateLinkedComment(string systemKeyword, string entityKeyId,
            string commentBase, params object[] commentParams)
        {
            string plainComment = string.Format(commentBase, commentParams);

            if (commentParams.Length != 1)
                return plainComment;

            var activityLogEntity = _activityEntityKeywords.GetLogEntity(systemKeyword);
            if (activityLogEntity == null)
                return plainComment;

            return LinkedCommentHelper.GenerateLinkedComment(entityKeyId, activityLogEntity.LinkPattern, commentParams.First().ToString(), commentBase);
        }
    }
}
