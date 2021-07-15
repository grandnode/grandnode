namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string PRODUCTS_PATTERN_KEY => "Grand.product.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static string PRODUCTS_BY_ID_KEY => "Grand.product.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// {1} : store ID
        /// </remarks>
        public static string PRODUCTS_CUSTOMER_ROLE => "Grand.product.cr-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string PRODUCTS_CUSTOMER_ROLE_PATTERN => "Grand.product.cr";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// </remarks>
        public static string PRODUCTS_CUSTOMER_TAG => "Grand.product.ct-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string PRODUCTS_CUSTOMER_TAG_PATTERN => "Grand.product.ct";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer tag Id?
        /// </remarks>
        public static string CUSTOMERTAGPRODUCTS_ROLE_KEY => "Grand.customertagproducts.tag-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// {0} customer id
        /// </summary>
        public static string PRODUCTS_CUSTOMER_PERSONAL_KEY => "Grand.product.personal-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        public static string PRODUCTS_CUSTOMER_PERSONAL_PATTERN => "Grand.product.personal";

        /// <summary>
        /// Key for cache 
        /// {0} - customer id
        /// {1} - product id
        /// </summary>
        public static string CUSTOMER_PRODUCT_PRICE_KEY_ID => "Grand.product.price-{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string PRODUCTS_SHOWONHOMEPAGE => "Grand.product.showonhomepage";

        /// <summary>
        /// Compare products cookie name
        /// </summary>
        public static string PRODUCTS_COMPARE_COOKIE_NAME => "Grand.CompareProducts";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string PRODUCTTAG_COUNT_KEY => "Grand.producttag.count-{0}";

        /// <summary>
        /// Key for all tags
        /// </summary>
        public static string PRODUCTTAG_ALL_KEY => "Grand.producttag.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string PRODUCTTAG_PATTERN_KEY => "Grand.producttag.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer id
        /// {1} : number
        /// </remarks>
        public static string RECENTLY_VIEW_PRODUCTS_KEY => "Grand.recentlyviewedproducts-{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// {0} customer id
        /// </summary>
        public static string RECENTLY_VIEW_PRODUCTS_PATTERN_KEY => "Grand.recentlyviewedproducts-{0}";
    }
}
