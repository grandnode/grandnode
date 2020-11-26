namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address ID
        /// </remarks>
        public static string ADDRESSES_BY_ID_KEY => "Grand.address.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string ADDRESSES_PATTERN_KEY => "Grand.address.";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string ADDRESSATTRIBUTES_ALL_KEY => "Grand.addressattribute.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address attribute ID
        /// </remarks>
        public static string ADDRESSATTRIBUTES_BY_ID_KEY => "Grand.addressattribute.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string ADDRESSATTRIBUTES_PATTERN_KEY => "Grand.addressattribute.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string ADDRESSATTRIBUTEVALUES_PATTERN_KEY => "Grand.addressattributevalue.";

    }
}
