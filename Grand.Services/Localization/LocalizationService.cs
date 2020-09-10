using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Localization;
using Grand.Services.Events;
using Grand.Services.Logging;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Services.Localization
{
    /// <summary>
    /// Provides information about localization
    /// </summary>
    public partial class LocalizationService : ILocalizationService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_ALL_KEY = "Grand.lsr.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : resource key
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY = "Grand.lsr.{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string LOCALSTRINGRESOURCES_PATTERN_KEY = "Grand.lsr.";

        private Dictionary<string, LocaleStringResource> _alllocaleStringResource = null;

        #endregion

        #region Fields

        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ICacheManager _cacheManager;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="logger">Logger</param>
        /// <param name="workContext">Work context</param>
        /// <param name="lsrRepository">Locale string resource repository</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="mediator">Mediator</param>
        public LocalizationService(ICacheManager cacheManager,
            ILogger logger, IWorkContext workContext,
            IRepository<LocaleStringResource> lsrRepository,
            LocalizationSettings localizationSettings, IMediator mediator)
        {
            _cacheManager = cacheManager;
            _logger = logger;
            _workContext = workContext;
            _lsrRepository = lsrRepository;
            _localizationSettings = localizationSettings;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual async Task DeleteLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");

            await _lsrRepository.DeleteAsync(localeStringResource);

            //cache
            await _cacheManager.RemoveByPrefix(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(localeStringResource);
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeStringResourceId">Locale string resource identifier</param>
        /// <returns>Locale string resource</returns>
        public virtual Task<LocaleStringResource> GetLocaleStringResourceById(string localeStringResourceId)
        {
            return _lsrRepository.GetByIdAsync(localeStringResourceId);
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <returns>Locale string resource</returns>
        public virtual Task<LocaleStringResource> GetLocaleStringResourceByName(string resourceName, string languageId,
            bool logIfNotFound = true)
        {
            var query = from lsr in _lsrRepository.Table
                        orderby lsr.ResourceName
                        where lsr.LanguageId == languageId && lsr.ResourceName == resourceName
                        select lsr;
            var localeStringResource = query.FirstOrDefaultAsync();

            if (localeStringResource == null && logIfNotFound)
                _logger.Warning(string.Format("Resource string ({0}) not found. Language ID = {1}", resourceName, languageId));
            return localeStringResource;
        }

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        public virtual IList<LocaleStringResource> GetAllResources(string languageId)
        {
            var filter = Builders<LocaleStringResource>.Filter.Eq(x => x.LanguageId, languageId);
            return _lsrRepository.Collection.Find(filter).ToList();
        }

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual async Task InsertLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");

            localeStringResource.ResourceName = localeStringResource.ResourceName.ToLowerInvariant();
            await _lsrRepository.InsertAsync(localeStringResource);

            //cache
            await _cacheManager.RemoveByPrefix(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(localeStringResource);
        }

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual async Task UpdateLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");

            localeStringResource.ResourceName = localeStringResource.ResourceName.ToLowerInvariant();
            await _lsrRepository.UpdateAsync(localeStringResource);

            //cache
            await _cacheManager.RemoveByPrefix(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(localeStringResource);
        }

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey)
        {
            if (_workContext.WorkingLanguage != null)
                return GetResource(resourceKey, _workContext.WorkingLanguage.Id);

            return "";
        }

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey, string languageId,
            bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
        {
            string result = string.Empty;
            if (resourceKey == null)
                resourceKey = string.Empty;
            resourceKey = resourceKey.Trim().ToLowerInvariant();
            if (_localizationSettings.LoadAllLocaleRecordsOnStartup)
            {
                //load all records (cached)
                if (_alllocaleStringResource != null)
                {
                    if (_alllocaleStringResource.ContainsKey(resourceKey.ToLowerInvariant()))
                        result = _alllocaleStringResource[resourceKey.ToLowerInvariant()].ResourceValue;
                }
                else
                {
                    string key = string.Format(LOCALSTRINGRESOURCES_ALL_KEY, languageId);
                    _alllocaleStringResource = _cacheManager.Get(key, () =>
                    {
                        var dictionary = new Dictionary<string, LocaleStringResource>();
                        var locales = GetAllResources(languageId);
                        foreach (var locale in locales)
                        {
                            var resourceName = locale.ResourceName.ToLowerInvariant();
                            if (!dictionary.ContainsKey(resourceName))
                                dictionary.Add(resourceName.ToLowerInvariant(), locale);
                            else
                            {
                                _lsrRepository.Delete(locale);
                            }
                        }
                        return dictionary;
                    });
                    if (_alllocaleStringResource.ContainsKey(resourceKey.ToLowerInvariant()))
                        result = _alllocaleStringResource[resourceKey.ToLowerInvariant()].ResourceValue;
                }
            }
            else
            {
                //gradual loading
                string key = string.Format(LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY, languageId, resourceKey);
                string lsr = _cacheManager.Get(key, () =>
                {
                    var builder = Builders<LocaleStringResource>.Filter;
                    var filter = builder.Eq(x => x.LanguageId, languageId);
                    filter = filter & builder.Eq(x => x.ResourceName, resourceKey.ToLowerInvariant());
                    return _lsrRepository.Collection.Find(filter).FirstOrDefault()?.ResourceValue;
                });

                if (lsr != null)
                    result = lsr;
            }
            if (string.IsNullOrEmpty(result))
            {
                if (logIfNotFound)
                    _logger.Warning(string.Format("Resource string ({0}) is not found. Language ID = {1}", resourceKey, languageId));

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    if (!returnEmptyIfNotFound)
                        result = resourceKey;
                }
            }
            return result;
        }

        /// <summary>
        /// Export language resources to xml
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportResourcesToXml(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            var sb = new StringBuilder();

            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true
            };

            using (var stringWriter = new StringWriter(sb))
            using (var xmlWriter = XmlWriter.Create(stringWriter, xwSettings))
            {

                await xmlWriter.WriteStartDocumentAsync();
                xmlWriter.WriteStartElement("Language");
                xmlWriter.WriteAttributeString("Name", language.Name);

                var resources = GetAllResources(language.Id);
                foreach (var resource in resources)
                {
                    xmlWriter.WriteStartElement("LocaleResource");
                    xmlWriter.WriteAttributeString("Name", resource.ResourceName);
                    xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                await xmlWriter.FlushAsync();
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        public virtual async Task ImportResourcesFromXml(Language language, string xml)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            if (string.IsNullOrEmpty(xml))
                return;

            var localeStringResources = new List<LocaleStringResource>();
            //stored procedures aren't supported
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");
            foreach (XmlNode node in nodes)
            {
                string name = node.Attributes["Name"].InnerText.Trim();
                string value = "";
                var valueNode = node.SelectSingleNode("Value");
                if (valueNode != null)
                    value = valueNode.InnerText;

                if (String.IsNullOrEmpty(name))
                    continue;

                //do not use "Insert"/"Update" methods because they clear cache
                //let's bulk insert
                var resource = await (from l in _lsrRepository.Table
                                      where l.ResourceName == name.ToLowerInvariant() && l.LanguageId == language.Id
                                      select l).FirstOrDefaultAsync();

                if (resource != null)
                {
                    resource.ResourceName = resource.ResourceName.ToLowerInvariant();
                    resource.ResourceValue = value;
                    await _lsrRepository.UpdateAsync(resource);
                }
                else
                {

                    localeStringResources.Add(new LocaleStringResource {
                        LanguageId = language.Id,
                        ResourceName = name.ToLowerInvariant(),
                        ResourceValue = value
                    });
                }
            }

            if(localeStringResources.Any())
                await _lsrRepository.InsertManyAsync(localeStringResources);


            //clear cache
            await _cacheManager.RemoveByPrefix(LOCALSTRINGRESOURCES_PATTERN_KEY);
        }

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        public virtual async Task ImportResourcesFromXmlInstall(Language language, string xml)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            if (String.IsNullOrEmpty(xml))
                return;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var localeStringResources = new List<LocaleStringResource>();

            var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");
            foreach (XmlNode node in nodes)
            {
                string name = node.Attributes["Name"].InnerText.Trim();
                string value = "";
                var valueNode = node.SelectSingleNode("Value");
                if (valueNode != null)
                    value = valueNode.InnerText;

                if (string.IsNullOrEmpty(name))
                    continue;

                localeStringResources.Add(
                    new LocaleStringResource {
                        LanguageId = language.Id,
                        ResourceName = name.ToLowerInvariant(),
                        ResourceValue = value
                    });
            }

            await _lsrRepository.InsertManyAsync(localeStringResources);

            //clear cache
            await _cacheManager.RemoveByPrefix(LOCALSTRINGRESOURCES_PATTERN_KEY);
        }

        #endregion
    }
}
