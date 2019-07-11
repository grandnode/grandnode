using Grand.Core.Domain.Localization;
using Grand.Core.Extensions;
using Grand.Core.Infrastructure;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Framework.Localization
{
    /// <summary>
    /// Represents extensions for localized URLs
    /// </summary>
    public static class LocalizedUrlExtenstions
    {
        /// <summary>
        /// Get a value indicating whether URL is localized (contains SEO code)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="pathBase">Application path base</param>
        /// <param name="isRawPath">A value indicating whether passed URL is raw URL</param>
        /// <param name="language">Language whose SEO code is in the URL if URL is localized</param>
        /// <returns>True if passed URL contains SEO code; otherwise false</returns>
        public static bool IsLocalizedUrl(this string url, PathString pathBase, bool isRawPath, IList<Language> languages, out Language language)
        {
            language = null;
            if (string.IsNullOrEmpty(url))
                return false;

            //remove application path from raw url
            if (isRawPath)
                url = url.RemoveApplicationPathFromRawUrl(pathBase);

            //get first segment of passed URL
            var firstSegment = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(firstSegment))
                return false;

            //suppose that the first segment is the language code and try to get language
            language = languages.FirstOrDefault(urlLanguage => urlLanguage.UniqueSeoCode.Equals(firstSegment, StringComparison.OrdinalIgnoreCase));

            //if language exists and published passed URL is localized
            return language.Return(urlLanguage => urlLanguage.Published, false);
        }

        /// <summary>
        /// Get a value indicating whether URL is localized (contains SEO code)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="pathBase">Application path base</param>
        /// <param name="isRawPath">A value indicating whether passed URL is raw URL</param>
        /// <param name="language">Language whose SEO code is in the URL if URL is localized</param>
        /// <returns>True if passed URL contains SEO code; otherwise false</returns>
        public static async Task<bool> IsLocalizedUrlAsync(this string url, ILanguageService languageService, PathString pathBase, bool isRawPath)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            
            //remove application path from raw url
            if (isRawPath)
                url = url.RemoveApplicationPathFromRawUrl(pathBase);

            //get first segment of passed URL
            var firstSegment = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(firstSegment))
                return false;

            //suppose that the first segment is the language code and try to get language
            var language = (await languageService.GetAllLanguages())
                .FirstOrDefault(urlLanguage => urlLanguage.UniqueSeoCode.Equals(firstSegment, StringComparison.OrdinalIgnoreCase));

            //if language exists and published passed URL is localized
            return language.Return(urlLanguage => urlLanguage.Published, false);
        }

        /// <summary>
        /// Remove application path from raw URL
        /// </summary>
        /// <param name="rawUrl">Raw URL</param>
        /// <param name="pathBase">Application path base</param>
        /// <returns>Result</returns>
        public static string RemoveApplicationPathFromRawUrl(this string rawUrl, PathString pathBase)
        {
            new PathString(rawUrl).StartsWithSegments(pathBase, out PathString result);
            return WebUtility.UrlDecode(result);
        }

        /// <summary>
        /// Remove language SEO code from URL
        /// </summary>
        /// <param name="url">Raw URL</param>
        /// <param name="pathBase">Application path base</param>
        /// <param name="isRawPath">A value indicating whether passed URL is raw URL</param>
        /// <returns>URL without language SEO code</returns>
        public static string RemoveLanguageSeoCodeFromUrl(this string url, PathString pathBase, bool isRawPath)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            //remove application path from raw URL
            if (isRawPath)
                url = url.RemoveApplicationPathFromRawUrl(pathBase);

            //get result URL
            url = url.TrimStart('/');
            var result = url.Contains('/') ? url.Substring(url.IndexOf('/')) : string.Empty;

            //and add back application path to raw URL
            if (isRawPath)
                result = pathBase + result;

            return result;
        }

        /// <summary>
        /// Add language SEO code to URL
        /// </summary>
        /// <param name="url">Raw URL</param>
        /// <param name="pathBase">Application path base</param>
        /// <param name="isRawPath">A value indicating whether passed URL is raw URL</param>
        /// <param name="language">Language</param>
        /// <returns>Result</returns>
        public static string AddLanguageSeoCodeToUrl(this string url, PathString pathBase, bool isRawPath, Language language)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            //remove application path from raw URL
            if (isRawPath && !string.IsNullOrEmpty(url))
                url = url.RemoveApplicationPathFromRawUrl(pathBase);

            //add language code
            url = $"/{language.UniqueSeoCode}{url}";

            //and add back application path to raw URL
            if (isRawPath)
                url = pathBase + url;

            return url;
        }
    }
}