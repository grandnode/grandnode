namespace Grand.Services.Logging.ActivityLogComment
{
    class LinkedCommentHelper
    {
        internal static string GenerateLinkedComment(string entityKeyId, string linkPattern, string commentParameter, string commentBase)
        {
            var linkUrl = string.Format(linkPattern, entityKeyId); // "/Admin/Category/Edit/5ea45e9c8258183ea0dbcb90"
            var link = $"<a href=\"{linkUrl}\">{commentParameter}</a>";
            return string.Format(commentBase, link);
        }
    }
}
