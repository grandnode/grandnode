using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        #region Currency

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : currency ID
        /// </remarks>
        public static string CURRENCIES_BY_ID_KEY => "Grand.currency.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : currency code
        /// </remarks>
        public static string CURRENCIES_BY_CODE => "Grand.currency.code-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public static string CURRENCIES_ALL_KEY => "Grand.currency.all-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string CURRENCIES_PATTERN_KEY => "Grand.currency.";

        #endregion

        #region Measure

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string MEASUREDIMENSIONS_ALL_KEY => "Grand.measuredimension.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : dimension ID
        /// </remarks>
        public static string MEASUREDIMENSIONS_BY_ID_KEY => "Grand.measuredimension.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string MEASUREWEIGHTS_ALL_KEY => "Grand.measureweight.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : weight ID
        /// </remarks>
        public static string MEASUREWEIGHTS_BY_ID_KEY => "Grand.measureweight.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string MEASUREDIMENSIONS_PATTERN_KEY => "Grand.measuredimension.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string MEASUREWEIGHTS_PATTERN_KEY => "Grand.measureweight.";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string MEASUREUNITS_ALL_KEY => "Grand.measureunit.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : dimension ID
        /// </remarks>
        public static string MEASUREUNITS_BY_ID_KEY => "Grand.measureunit.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string MEASUREUNITS_PATTERN_KEY => "Grand.measureunit.";

        #endregion

        #region Country
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : show hidden records?
        /// </remarks>
        public static string COUNTRIES_ALL_KEY => "Grand.country.all-{0}-{1}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// </remarks>
        public static string COUNTRIES_BY_KEY => "Grand.country.id-{0}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : twoletter
        /// </remarks>
        public static string COUNTRIES_BY_TWOLETTER => "Grand.country.twoletter-{0}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : threeletter
        /// </remarks>
        public static string COUNTRIES_BY_THREELETTER => "Grand.country.threeletter-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string COUNTRIES_PATTERN_KEY => "Grand.country.";

        #endregion

        #region StateProvinces

        /// {0} : state ID
        /// </remarks>
        public static string STATEPROVINCES_BY_KEY => "Grand.stateprovince.{0}";

        /// {0} : country ID
        /// {1} : language ID
        /// {2} : show hidden records?
        /// </remarks>
        public static string STATEPROVINCES_ALL_KEY => "Grand.stateprovince.all-{0}-{1}-{2}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string STATEPROVINCES_PATTERN_KEY => "Grand.stateprovince.";

        #endregion
    }
}
