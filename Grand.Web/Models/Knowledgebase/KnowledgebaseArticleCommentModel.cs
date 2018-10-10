using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Models.Knowledgebase
{
    public partial class KnowledgebaseArticleCommentModel : BaseGrandEntityModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CommentText { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool AllowViewingProfiles { get; set; }
    }
}
