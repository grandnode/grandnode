using Grand.Core;
using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Forums;
using Grand.Domain.Localization;
using Grand.Domain.Seo;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Seo
{
    public static class SeoExtensions
    {
        #region Fields

        private static Dictionary<string, string> _seoCharacterTable;
        private static readonly object s_lock = new object();

        #endregion

        #region Product tag

        /// <summary>
        /// Gets product tag SE (search engine) name
        /// </summary>
        /// <param name="productTag">Product tag</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Product tag SE (search engine) name</returns>
        public static string GetSeName(this ProductTag productTag, string languageId)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");
            string seName = GetSeName(productTag.GetLocalized(x => x.Name, languageId), false, false);
            return seName;
        }

        #endregion

        #region Forum

        /// <summary>
        /// Gets ForumGroup SE (search engine) name
        /// </summary>
        /// <param name="forumGroup">ForumGroup</param>
        /// <returns>ForumGroup SE (search engine) name</returns>
        public static string GetSeName(this ForumGroup forumGroup)
        {
            if (forumGroup == null)
                throw new ArgumentNullException("forumGroup");
            string seName = GetSeName(forumGroup.Name, false, false);
            return seName;
        }

        /// <summary>
        /// Gets Forum SE (search engine) name
        /// </summary>
        /// <param name="forum">Forum</param>
        /// <returns>Forum SE (search engine) name</returns>
        public static string GetSeName(this Forum forum)
        {
            if (forum == null)
                throw new ArgumentNullException("forum");
            string seName = GetSeName(forum.Name, false, false);
            return seName;
        }

        /// <summary>
        /// Gets ForumTopic SE (search engine) name
        /// </summary>
        /// <param name="forumTopic">ForumTopic</param>
        /// <returns>ForumTopic SE (search engine) name</returns>
        public static string GetSeName(this ForumTopic forumTopic)
        {
            if (forumTopic == null)
                throw new ArgumentNullException("forumTopic");
            string seName = GetSeName(forumTopic.Subject, false, false);

            // Trim SE name to avoid URLs that are too long
            var maxLength = 100;
            if (seName.Length > maxLength)
            {
                seName = seName.Substring(0, maxLength);
            }

            return seName;
        }

        #endregion

        #region General

        /// <summary>
        ///  Get search engine friendly name (slug)
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if language specified one is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>Search engine  name (slug)</returns>
        public static string GetSeName<T>(this T entity, string languageId, bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity, ISlugSupported, ILocalizedEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            string seName = string.Empty;
            if (!String.IsNullOrEmpty(languageId))
            {
                var value = entity.Locales.Where(x => x.LanguageId == languageId && x.LocaleKey == "SeName").FirstOrDefault();
                if (value != null)
                    if (!String.IsNullOrEmpty(value.LocaleValue))
                        seName = value.LocaleValue;
            }

            //set default value if required
            if (string.IsNullOrEmpty(seName) && returnDefaultValue)
            {
                seName = entity.SeName;
            }

            return seName;
        }

        /// <summary>
        /// Validate search engine name
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="seName">Search engine name to validate</param>
        /// <param name="name">User-friendly name used to generate sename</param>
        /// <param name="ensureNotEmpty">Ensreu that sename is not empty</param>
        /// <returns>Valid sename</returns>
        public static async Task<string> ValidateSeName<T>(this T entity, string seName, string name, bool ensureNotEmpty,
            SeoSettings seoSettings, IUrlRecordService urlRecordService, ILanguageService languageService)
             where T : BaseEntity, ISlugSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            //use name if sename is not specified
            if (String.IsNullOrWhiteSpace(seName) && !String.IsNullOrWhiteSpace(name))
                seName = name;

            //validation
            seName = GetSeName(seName, seoSettings);

            //max length
            //For long URLs we can get the following error:
            //"the specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters"
            //that's why we limit it to 200 here (consider a store URL + probably added {0}-{1} below)
            seName = CommonHelper.EnsureMaximumLength(seName, 200);

            if (String.IsNullOrWhiteSpace(seName))
            {
                if (ensureNotEmpty)
                {
                    //use entity identifier as sename if empty
                    seName = entity.Id.ToString();
                }
                else
                {
                    //return. no need for further processing
                    return seName;
                }
            }

            //ensure this sename is not reserved yet
            string entityName = typeof(T).Name;
            int i = 2;
            var tempSeName = seName;
            while (true)
            {
                //check whether such slug already exists (and that is not the current entity)
                var urlRecord = await urlRecordService.GetBySlug(tempSeName);
                var reserved1 = urlRecord != null && !(urlRecord.EntityId == entity.Id && urlRecord.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
                //and it's not in the list of reserved slugs
                var reserved2 = seoSettings.ReservedUrlRecordSlugs.Contains(tempSeName, StringComparer.OrdinalIgnoreCase);
                var reserved3 = (await languageService.GetAllLanguages(true)).Any(language => language.UniqueSeoCode.Equals(tempSeName, StringComparison.OrdinalIgnoreCase));
                if (!reserved1 && !reserved2 && !reserved3)
                    break;

                tempSeName = string.Format("{0}-{1}", seName, i);
                i++;
            }
            seName = tempSeName;

            return seName;
        }


        /// <summary>
        /// Get SE name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Result</returns>
        public static string GetSeName(string name, SeoSettings seoSettings)
        {
            return GetSeName(name, seoSettings.ConvertNonWesternChars, seoSettings.AllowUnicodeCharsInUrls, seoSettings.SeoCharConversion);
        }

        /// <summary>
        /// Get SE name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="convertNonWesternChars">A value indicating whether non western chars should be converted</param>
        /// <param name="allowUnicodeCharsInUrls">A value indicating whether Unicode chars are allowed</param>
        /// <returns>Result</returns>
        public static string GetSeName(string name, bool convertNonWesternChars, bool allowUnicodeCharsInUrls, string charConversions = null)
        {
            if (String.IsNullOrEmpty(name))
                return name;
            string okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
            name = name.Trim().ToLowerInvariant();

            if (convertNonWesternChars)
            {
                if (!string.IsNullOrEmpty(charConversions) && _seoCharacterTable == null)
                    InitializeSeoCharacterTable(charConversions);
            }

            var sb = new StringBuilder();
            foreach (char c in name.ToCharArray())
            {
                string c2 = c.ToString();
                if (convertNonWesternChars && _seoCharacterTable != null)
                {
                    if (_seoCharacterTable.ContainsKey(c2))
                        c2 = _seoCharacterTable[c2];
                }

                if (allowUnicodeCharsInUrls)
                {
                    if (char.IsLetterOrDigit(c) || okChars.Contains(c2))
                        sb.Append(c2);
                }
                else if (okChars.Contains(c2))
                {
                    sb.Append(c2);
                }
            }
            string name2 = sb.ToString();
            name2 = name2.Replace(" ", "-");
            while (name2.Contains("--"))
                name2 = name2.Replace("--", "-");
            while (name2.Contains("__"))
                name2 = name2.Replace("__", "_");
            return name2;
        }

        private static void InitializeSeoCharacterTable(string charConversions)
        {
            lock (s_lock)
            {
                if (_seoCharacterTable == null)
                {
                    _seoCharacterTable = new Dictionary<string, string>();

                    foreach (var conversion in charConversions.Split(";"))
                    {
                        var strLeft = conversion.Split(":").FirstOrDefault();
                        var strRight = conversion.Split(":").LastOrDefault();
                        if (!string.IsNullOrEmpty(strLeft) &&  !_seoCharacterTable.ContainsKey(strLeft))
                        {
                            _seoCharacterTable.Add(strLeft.Trim(), strRight.Trim());
                        }
                    }
                }
            }

        }

        #endregion
    }
}
