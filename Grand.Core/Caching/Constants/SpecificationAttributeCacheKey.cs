namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : sename
        /// </remarks>
        public static string SPECIFICATION_BY_SENAME => "Grand.specification.sename-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : specification ID
        /// </remarks>
        public static string SPECIFICATION_BY_ID_KEY => "Grand.specification.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : specification option ID
        /// </remarks>
        public static string SPECIFICATION_BY_OPTIONID_KEY => "Grand.specification.optionid-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string SPECIFICATION_PATTERN_KEY => "Grand.specification.";

    }
}
