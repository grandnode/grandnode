using System.Collections.Generic;

namespace Grand.Services.Logging
{
    public class ActivityKeywordsProvider: IActivityKeywordsProvider
    {
        public virtual IList<string> GetCategorySystemKeywords()
        {
            var tokens = new List<string>
            {
                "PublicStore.ViewCategory",
                "EditCategory",
                "AddNewCategory",
            };
            return tokens;
        }
        public virtual IList<string> GetProductSystemKeywords()
        {
            var tokens = new List<string>
            {
                "PublicStore.ViewProduct",
                "EditProduct",
                "AddNewProduct",
            };
            return tokens;
        }
        public virtual IList<string> GetManufacturerSystemKeywords()
        {
            var tokens = new List<string>
            {
                "PublicStore.ViewManufacturer",
                "EditManufacturer",
                "AddNewManufacturer"
            };
            return tokens;
        }

        public IList<string> GetKnowledgebaseCategorySystemKeywords()
        {
            var tokens = new List<string>
            {
                "CreateKnowledgebaseCategory",
                "UpdateKnowledgebaseCategory",
                "DeleteKnowledgebaseCategory"
            };
            return tokens;
        }


        public IList<string> GetKnowledgebaseArticleSystemKeywords()
        {
            var tokens = new List<string>
            {
                "CreateKnowledgebaseArticle",
                "UpdateKnowledgebaseArticle",
                "DeleteKnowledgebaseArticle",
            };
            return tokens;
        }
    }
}
