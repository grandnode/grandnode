using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Knowledgebase
{
    /// <summary>
    /// Extensions 
    /// </summary>
    public static class KnowledgebaseExtensions
    {
        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this KnowledgebaseCategory category,
            IList<KnowledgebaseCategory> allCategories,
            string separator = ">>", string languageId = "")
        {
            string result = string.Empty;

            var breadcrumb = GetCategoryBreadCrumb(category, allCategories);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var categoryName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? categoryName
                    : string.Format("{0} {1} {2}", result, separator, categoryName);
            }

            return result;
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public static IList<KnowledgebaseCategory> GetCategoryBreadCrumb(this KnowledgebaseCategory category, IList<KnowledgebaseCategory> allCategories)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            var result = new List<KnowledgebaseCategory>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<string>();

            while (category != null && !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = (from c in allCategories
                            where c.Id == category.ParentCategoryId
                            select c).FirstOrDefault();
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public static IList<KnowledgebaseCategory> GetCategoryBreadCrumb(this KnowledgebaseCategory category,
            IKnowledgebaseService knowledgebaseService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            var result = new List<KnowledgebaseCategory>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<string>();

            while (category != null && //not null                
                (showHidden || category.Published) && //published
                (showHidden || aclService.Authorize(category)) && //ACL
                (showHidden || storeMappingService.Authorize(category)) && //Store mapping
                !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = knowledgebaseService.GetKnowledgebaseCategory(category.ParentCategoryId);
            }

            result.Reverse();
            return result;
        }

        public static List<KnowledgebaseCategory> SortCategoriesForTree(this IList<KnowledgebaseCategory> source, string parentId = null,
            bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<KnowledgebaseCategory>();

            foreach (var cat in source.Where(c => c.ParentCategoryId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCategoriesForTree(source, cat.Id, true));
            }
            if (!ignoreCategoriesWithoutExistingParent && result.Count != source.Count)
            {
                //find categories without parent in provided category source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }
    }
}
