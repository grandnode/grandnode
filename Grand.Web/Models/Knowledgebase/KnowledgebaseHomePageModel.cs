using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseHomePageModel : BaseGrandEntityModel
    {
        public List<KnowledgebaseArticleModel> Articles { get; set; }
        public List<KnowledgebaseCategoryModel> Categories { get; set; }
        public string CurrentCategoryId { get; set; }
        public string CurrentCategoryDescription { get; set; }
        public string CurrentCategoryMetaTitle { get; set; }
        public string CurrentCategoryMetaDescription { get; set; }
        public string CurrentCategoryMetaKeywords { get; set; }
        public string CurrentCategoryName { get; set; }
        public string CurrentCategorySeName { get; set; }

        public KnowledgebaseHomePageModel()
        {
            Articles = new List<KnowledgebaseArticleModel>();
            Categories = new List<KnowledgebaseCategoryModel>();
        }
    }
}
