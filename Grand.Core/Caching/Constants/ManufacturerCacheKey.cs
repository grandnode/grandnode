namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string MANUFACTURERS_PATTERN_KEY => "Grand.manufacturer.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer ID
        /// </remarks>
        public static string MANUFACTURERS_BY_ID_KEY => "Grand.manufacturer.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : manufacturer ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        public static string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY => "Grand.productmanufacturer.allbymanufacturerid-{0}-{1}-{2}-{3}-{4}-{5}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string PRODUCTMANUFACTURERS_PATTERN_KEY => "Grand.productmanufacturer.";

    }
}
