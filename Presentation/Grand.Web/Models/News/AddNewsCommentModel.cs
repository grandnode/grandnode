using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.News
{
    public partial class AddNewsCommentModel : BaseGrandModel
    {
        [GrandResourceDisplayName("News.Comments.CommentTitle")]
        [AllowHtml]
        public string CommentTitle { get; set; }

        [GrandResourceDisplayName("News.Comments.CommentText")]
        [AllowHtml]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}