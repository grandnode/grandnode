using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Models.Knowledgebase
{
    public partial class AddKnowledgebaseArticleCommentModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Knowledgebase.Article.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
