namespace Grand.Services.Logging.ActivityLogComment
{
    /// <summary>
    /// Activity log linked comment creator interface
    /// </summary>
    public interface ILinkedCommentCreator
    {
        /// <summary>
        /// Create activity log comment's text, containing hyperlink to the changed object
        /// </summary>
        /// <param name="systemKeyword"></param>
        /// <param name="entityKeyId"></param>
        /// <param name="commentBase"></param>
        /// <param name="commentParams"></param>
        /// <returns></returns>
        string CreateLinkedComment(string systemKeyword, string entityKeyId,
            string commentBase, params object[] commentParams);
    }
}