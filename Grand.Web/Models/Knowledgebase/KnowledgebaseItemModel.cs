using Grand.Core.Models;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseItemModel : BaseEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public bool IsArticle { get; set; }
        public string FormattedBreadcrumbs { get; set; }
    }
}
