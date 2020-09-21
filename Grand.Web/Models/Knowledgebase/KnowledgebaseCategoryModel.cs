using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseCategoryModel : BaseEntityModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsCurrent { get; set; }

        public List<KnowledgebaseCategoryModel> Children { get; set; }

        public KnowledgebaseCategoryModel Parent { get; set; }

        public string SeName { get; set; }
    }
}
