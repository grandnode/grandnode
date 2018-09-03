using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Models.Knowledgebase
{
    public partial class AddKnowledgebaseArticleCommentModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Knowledgebase.Article.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
