using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    public class KnowledgebaseArticleGridModel : BaseEntityModel
    {
        public string Name { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public string ArticleId { get; set; }
    }
}
