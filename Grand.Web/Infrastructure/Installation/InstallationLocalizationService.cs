using Grand.Core;
using Grand.Core.Extensions;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Grand.Web.Infrastructure.Installation
{
    /// <summary>
    /// Localization service for installation process
    /// </summary>
    public partial class InstallationLocalizationService : IInstallationLocalizationService
    {
        /// <summary>
        /// Cookie name to language for the installation page
        /// </summary>
        private const string LANGUAGE_COOKIE_NAME = ".Grand.installation.lang";

        /// <summary>
        /// Available languages
        /// </summary>
        private IList<InstallationLanguage> _availableLanguages;

        /// <summary>
        /// Available collation
        /// </summary>
        private IList<InstallationCollation> _availableCollation;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstallationLocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Get locale resource value
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>Resource value</returns>
        public string GetResource(string resourceName)
        {
            var language = GetCurrentLanguage();
            if (language == null)
                return resourceName;
            var resourceValue = language.Resources
                .Where(r => r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Value)
                .FirstOrDefault();
            if (String.IsNullOrEmpty(resourceValue))
                //return name
                return resourceName;

            return resourceValue;
        }

        /// <summary>
        /// Get current language for the installation page
        /// </summary>
        /// <returns>Current language</returns>
        public virtual InstallationLanguage GetCurrentLanguage()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            //try to get cookie
            httpContext.Request.Cookies.TryGetValue(LANGUAGE_COOKIE_NAME, out string cookieLanguageCode);

            //ensure it's available (it could be delete since the previous installation)
            var availableLanguages = GetAvailableLanguages();

            var language = availableLanguages
                .FirstOrDefault(l => l.Code.Equals(cookieLanguageCode, StringComparison.OrdinalIgnoreCase));
            if (language != null)
                return language;

            //let's find by current browser culture
            if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var userLanguages))
            {
                var userLanguage = userLanguages.FirstOrDefault().Return(l => l.Split(',')[0], string.Empty);
                if (!string.IsNullOrEmpty(userLanguage))
                {
                    //right. we do "StartsWith" (not "Equals") because we have shorten codes (not full culture names)
                    language = availableLanguages.FirstOrDefault(l => userLanguage.StartsWith(l.Code, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (language != null)
                return language;

            //let's return the default one
            language = availableLanguages.FirstOrDefault(l => l.IsDefault);
            if (language != null)
                return language;

            //return any available language
            language = availableLanguages.FirstOrDefault();
            return language;
        }

        /// <summary>
        /// Save a language for the installation page
        /// </summary>
        /// <param name="languageCode">Language code</param>
        public virtual void SaveCurrentLanguage(string languageCode)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(24),
                HttpOnly = true
            };
            httpContext.Response.Cookies.Delete(LANGUAGE_COOKIE_NAME);
            httpContext.Response.Cookies.Append(LANGUAGE_COOKIE_NAME, languageCode, cookieOptions);
        }

        /// <summary>
        /// Get a list of available languages
        /// </summary>
        /// <returns>Available installation languages</returns>
        public virtual IList<InstallationLanguage> GetAvailableLanguages()
        {
            if (_availableLanguages != null)
                return _availableLanguages;

            _availableLanguages = new List<InstallationLanguage>();
            foreach (var filePath in Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/Installation/"), "*.xml"))
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(File.OpenRead(filePath));

                //get language code
                var languageCode = "";
                //we file name format: installation.{languagecode}.xml
                var r = new Regex(Regex.Escape("installation.") + "(.*?)" + Regex.Escape(".xml"));
                var matches = r.Matches(Path.GetFileName(filePath));
                foreach (Match match in matches)
                    languageCode = match.Groups[1].Value;

                var languageNode = xmlDocument.SelectSingleNode(@"//Language");

                if (languageNode == null || languageNode.Attributes == null)
                    continue;

                //get language friendly name
                var languageName = languageNode.Attributes["Name"].InnerText.Trim();

                //is default
                var isDefaultAttribute = languageNode.Attributes["IsDefault"];
                var isDefault = isDefaultAttribute != null && Convert.ToBoolean(isDefaultAttribute.InnerText.Trim());

                //is default
                var isRightToLeftAttribute = languageNode.Attributes["IsRightToLeft"];
                var isRightToLeft = isRightToLeftAttribute != null && Convert.ToBoolean(isRightToLeftAttribute.InnerText.Trim());

                //create language
                var language = new InstallationLanguage
                {
                    Code = languageCode,
                    Name = languageName,
                    IsDefault = isDefault,
                    IsRightToLeft = isRightToLeft,
                };

                //load resources
                var resources = xmlDocument.SelectNodes(@"//Language/LocaleResource");
                if (resources == null)
                    continue;
                foreach (XmlNode resNode in resources)
                {
                    if (resNode.Attributes == null)
                        continue;

                    var resNameAttribute = resNode.Attributes["Name"];
                    var resValueNode = resNode.SelectSingleNode("Value");

                    if (resNameAttribute == null)
                        throw new GrandException("All installation resources must have an attribute Name=\"Value\".");
                    var resourceName = resNameAttribute.Value.Trim();
                    if (string.IsNullOrEmpty(resourceName))
                        throw new GrandException("All installation resource attributes 'Name' must have a value.'");

                    if (resValueNode == null)
                        throw new GrandException("All installation resources must have an element \"Value\".");
                    var resourceValue = resValueNode.InnerText.Trim();

                    language.Resources.Add(new InstallationLocaleResource
                    {
                        Name = resourceName,
                        Value = resourceValue
                    });
                }

                _availableLanguages.Add(language);
                _availableLanguages = _availableLanguages.OrderBy(l => l.Name).ToList();

            }
            return _availableLanguages;
        }

        /// <summary>
        /// Get a list of available collactions
        /// </summary>
        /// <returns>Available collations mongodb</returns>
        public virtual IList<InstallationCollation> GetAvailableCollations()
        {
            if (_availableCollation != null)
                return _availableCollation;

            _availableCollation = new List<InstallationCollation>();
            var filePath = CommonHelper.MapPath("~/App_Data/Localization/supportedcollation.xml");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(File.OpenRead(filePath));

            var collation = xmlDocument.SelectNodes(@"//Collations/Collation");

            foreach (XmlNode resNode in collation)
            {
                var resNameAttribute = resNode.Attributes["Name"];
                var resValueNode = resNode.SelectSingleNode("Value");

                var resourceName = resNameAttribute.Value.Trim();
                var resourceValue = resValueNode.InnerText.Trim();

                _availableCollation.Add(new InstallationCollation()
                {
                    Name = resourceName,
                    Value = resourceValue,
                });
            }

            return _availableCollation;
        }
    }
}
