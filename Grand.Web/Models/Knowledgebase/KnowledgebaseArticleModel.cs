using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseArticleModel : BaseGrandEntityModel
    {
        public KnowledgebaseArticleModel()
        {
            RelatedArticles = new List<KnowledgebaseArticleModel>();
            CategoryBreadcrumb = new List<KnowledgebaseCategoryModel>();
            Comments = new List<KnowledgebaseArticleCommentModel>();
            AddNewComment = new AddKnowledgebaseArticleCommentModel();
        }

        public string Name { get; set; }

        public string Content { get; set; }

        public string ParentCategoryId { get; set; }

        public string SeName { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public bool AllowComments { get; set; }

        public IList<KnowledgebaseArticleModel> RelatedArticles { get; set; }

        public List<KnowledgebaseCategoryModel> CategoryBreadcrumb { get; set; }

        public AddKnowledgebaseArticleCommentModel AddNewComment { get; set; }

        public IList<KnowledgebaseArticleCommentModel> Comments { get; set; }
    }
}
