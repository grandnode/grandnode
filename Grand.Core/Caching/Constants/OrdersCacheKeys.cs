namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        #region Checkout attributes

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : >A value indicating whether we should exlude shippable attributes
        /// {2} : ignore ACL?
        /// </remarks>
        public static string CHECKOUTATTRIBUTES_ALL_KEY => "Grand.checkoutattribute.all-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : checkout attribute ID
        /// </remarks>
        public static string CHECKOUTATTRIBUTES_BY_ID_KEY => "Grand.checkoutattribute.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string CHECKOUTATTRIBUTES_PATTERN_KEY => "Grand.checkoutattribute.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string CHECKOUTATTRIBUTEVALUES_PATTERN_KEY => "Grand.checkoutattributevalue.";

        #endregion

        #region Order tags

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string ORDERTAG_COUNT_KEY => "Grand.ordertag.count-{0}";

        /// <summary>
        /// Key for all tags
        /// </summary>
        public static string ORDERTAG_ALL_KEY => "Grand.ordertag.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string ORDERTAG_PATTERN_KEY => "Grand.ordertag.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : order ID
        /// </remarks>
        public static string ORDERS_BY_ID_KEY => "Grand.order.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        public static string ORDERS_PATTERN_KEY => "Grand.order.";

        #endregion

    }
}
