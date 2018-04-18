using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseArticleModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string Content { get; set; }

        public string ParentCategoryId { get; set; }

        public string SeName { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }
    }
}
