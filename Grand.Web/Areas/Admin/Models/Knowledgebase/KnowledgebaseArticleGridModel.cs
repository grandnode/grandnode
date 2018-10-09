using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    public class KnowledgebaseArticleGridModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public string ArticleId { get; set; }
    }
}
