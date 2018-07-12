using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Knowledgebase
{
    public class KnowledgebaseHomePageModel : BaseGrandEntityModel
    {
        public List<KnowledgebaseItemModel> Items { get; set; }
        public List<KnowledgebaseCategoryModel> Categories { get; set; }
        public string CurrentCategoryId { get; set; }
        public string CurrentCategoryDescription { get; set; }
        public string CurrentCategoryMetaTitle { get; set; }
        public string CurrentCategoryMetaDescription { get; set; }
        public string CurrentCategoryMetaKeywords { get; set; }
        public string CurrentCategoryName { get; set; }
        public string CurrentCategorySeName { get; set; }
        public List<KnowledgebaseCategoryModel> CategoryBreadcrumb { get; set; }
        public string SearchKeyword { get; set; }

        public KnowledgebaseHomePageModel()
        {
            Items = new List<KnowledgebaseItemModel>();
            Categories = new List<KnowledgebaseCategoryModel>();
            CategoryBreadcrumb = new List<KnowledgebaseCategoryModel>();
        }
    }
}
