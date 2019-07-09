using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.News
{
    public partial class AddNewsCommentModel : BaseGrandModel
    {
        [GrandResourceDisplayName("News.Comments.CommentTitle")]
        public string CommentTitle { get; set; }

        [GrandResourceDisplayName("News.Comments.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}