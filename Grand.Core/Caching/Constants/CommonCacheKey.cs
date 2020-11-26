namespace Grand.Core.Caching.Constants
{
    public static partial class CacheKey
    {
        #region Settings

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string SETTINGS_ALL_KEY => "Grand.setting.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string SETTINGS_PATTERN_KEY => "Grand.setting.";

        #endregion

        #region Discounts

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : discont ID
        /// </remarks>
        public static string DISCOUNTS_BY_ID_KEY => "Grand.discount.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : store ident
        /// {2} : coupon code
        /// {3} : discount name
        /// </remarks>
        public static string DISCOUNTS_ALL_KEY => "Grand.discount.all-{0}-{1}-{2}-{3}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string DISCOUNTS_PATTERN_KEY => "Grand.discount.";

        #endregion

        #region Forum

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string FORUMGROUP_ALL_KEY => "Grand.forumgroup.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : forum group ID
        /// </remarks>
        public static string FORUM_ALLBYFORUMGROUPID_KEY => "Grand.forum.allbyforumgroupid-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string FORUMGROUP_PATTERN_KEY => "Grand.forumgroup.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string FORUM_PATTERN_KEY => "Grand.forum.";

        #endregion

        #region Languages & localization

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public static string LANGUAGES_BY_ID_KEY => "Grand.language.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public static string LANGUAGES_ALL_KEY => "Grand.language.all-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string LANGUAGES_PATTERN_KEY => "Grand.language.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public static string LOCALSTRINGRESOURCES_ALL_KEY => "Grand.lsr.all-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : resource key
        /// </remarks>
        public static string LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY => "Grand.lsr.{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string LOCALSTRINGRESOURCES_PATTERN_KEY => "Grand.lsr.";

        #endregion

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture ID
        /// {1} : store ID
        /// {2} : target size
        /// {3} : showDefaultPicture
        /// {4} : storeLocation
        /// {5} : pictureType
        /// </remarks>
        public static string PICTURE_BY_KEY => "Grand.picture-{0}-{1}-{2}-{3}-{4}-{5}";

        #region Seo

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// {2} : language ID
        /// </remarks>
        public static string URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY => "Grand.urlrecord.active.id-name-language-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string URLRECORD_ALL_KEY => "Grand.urlrecord.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : slug
        /// </remarks>
        public static string URLRECORD_BY_SLUG_KEY => "Grand.urlrecord.active.slug-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string URLRECORD_PATTERN_KEY = "Grand.urlrecord.";

        #endregion

        #region Stores

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string STORES_ALL_KEY => "Grand.stores.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string STORES_BY_ID_KEY => "Grand.stores.id-{0}";

        #endregion

        #region Tax

        /// <summary>
        /// Key for caching
        /// </summary>
        public static string TAXCATEGORIES_ALL_KEY => "Grand.taxcategory.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : tax category ID
        /// </remarks>
        public static string TAXCATEGORIES_BY_ID_KEY => "Grand.taxcategory.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string TAXCATEGORIES_PATTERN_KEY => "Grand.taxcategory.";

        #endregion

        #region Topics

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : ignore ACL?
        /// </remarks>
        public static string TOPICS_ALL_KEY => "Grand.topics.all-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic ID
        /// </remarks>
        public static string TOPICS_BY_ID_KEY => "Grand.topics.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic systemname
        /// {1} : store id
        /// </remarks>
        public static string TOPICS_BY_SYSTEMNAME => "Grand.topics.systemname-{0}-{1}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string TOPICS_PATTERN_KEY => "Grand.topics.";

        #endregion
    }
}
