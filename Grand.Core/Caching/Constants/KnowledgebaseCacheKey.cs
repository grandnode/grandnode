namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string KNOWLEDGEBASE_CATEGORIES_PATTERN_KEY => "Knowledgebase.category.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string ARTICLES_PATTERN_KEY => "Knowledgebase.article.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        public static string KNOWLEDGEBASE_CATEGORY_BY_ID => "Knowledgebase.category.id-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        public static string KNOWLEDGEBASE_CATEGORIES => "Knowledgebase.category.all-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        public static string ARTICLES => "Knowledgebase.article.all-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : article ID
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        public static string ARTICLE_BY_ID => "Knowledgebase.article.id-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        public static string ARTICLES_BY_CATEGORY_ID => "Knowledgebase.article.categoryid-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : keyword
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        public static string ARTICLES_BY_KEYWORD => "Knowledgebase.article.keyword-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : keyword
        /// {1} : customer roles
        /// {2} : store id
        /// </remarks>
        public static string KNOWLEDGEBASE_CATEGORIES_BY_KEYWORD => "Knowledgebase.category.keyword-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// {0} : customer roles
        /// {1} : store id
        /// </summary>
        public static string HOMEPAGE_ARTICLES => "Knowledgebase.article.homepage-{0}-{1}";

    }
}
