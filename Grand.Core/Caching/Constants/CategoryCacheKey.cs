namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string CATEGORIES_PATTERN_KEY => "Grand.category.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// </remarks>
        public static string CATEGORIES_BY_ID_KEY => "Grand.category.id-{0}";

        /// <summary>
        /// Key for caching 
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// {4} : include all levels (child)
        /// </remarks>
        public static string CATEGORIES_BY_PARENT_CATEGORY_ID_KEY => "Grand.category.byparent-{0}-{1}-{2}-{3}-{4}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string PRODUCTCATEGORIES_PATTERN_KEY => "Grand.productcategory.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : category ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        public static string PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY => "Grand.productcategory.allbycategoryid-{0}-{1}-{2}-{3}-{4}-{5}";
    }
}
