using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseItemModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public bool IsArticle { get; set; }
        public string FormattedBreadcrumbs { get; set; }
    }
}
