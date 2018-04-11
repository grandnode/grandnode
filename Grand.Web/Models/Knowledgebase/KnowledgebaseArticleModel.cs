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
    }
}
