using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Models.News
{
    public partial class AddNewsCommentModel : BaseModel
    {
        [GrandResourceDisplayName("News.Comments.CommentTitle")]
        public string CommentTitle { get; set; }

        [GrandResourceDisplayName("News.Comments.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}