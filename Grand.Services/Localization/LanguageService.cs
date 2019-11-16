using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Localization
{
    /// <summary>
    /// Language service
    /// </summary>
    public partial class LanguageService : ILanguageService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LANGUAGES_BY_ID_KEY = "Grand.language.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string LANGUAGES_ALL_KEY = "Grand.language.all-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string LANGUAGES_PATTERN_KEY = "Grand.language.";

        #endregion

        #region Fields

        private readonly IRepository<Language> _languageRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="languageRepository">Language repository</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="mediator">Mediator</param>
        public LanguageService(ICacheManager cacheManager,
            IRepository<Language> languageRepository,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            LocalizationSettings localizationSettings,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _languageRepository = languageRepository;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
            _localizationSettings = localizationSettings;
            _mediator = mediator;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual async Task DeleteLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            
            //update default admin area language (if required)
            if (_localizationSettings.DefaultAdminLanguageId == language.Id)
            {
                foreach (var activeLanguage in await GetAllLanguages())
                {
                    if (activeLanguage.Id != language.Id)
                    {
                        _localizationSettings.DefaultAdminLanguageId = activeLanguage.Id;
                        await _settingService.SaveSetting(_localizationSettings);
                        break;
                    }
                }
            }
            
            await _languageRepository.DeleteAsync(language);

            //cache
            await _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(language);
        }

        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Languages</returns>
        public virtual async Task<IList<Language>> GetAllLanguages(bool showHidden = false, string storeId = "")
        {
            string key = string.Format(LANGUAGES_ALL_KEY, showHidden);
            var languages = await _cacheManager.GetAsync(key, () =>
            {
                var query = _languageRepository.Table;

                if (!showHidden)
                    query = query.Where(l => l.Published);
                query = query.OrderBy(l => l.DisplayOrder);
                return query.ToListAsync();
            });

            //store mapping
            if (!String.IsNullOrWhiteSpace(storeId))
            {
                languages = languages
                    .Where(l => _storeMappingService.Authorize(l, storeId))
                    .ToList();
            }
            return languages;
        }

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Language</returns>
        public virtual Task<Language> GetLanguageById(string languageId)
        {
            string key = string.Format(LANGUAGES_BY_ID_KEY, languageId);
            return _cacheManager.GetAsync(key, () => _languageRepository.GetByIdAsync(languageId));
        }

        /// <summary>
        /// Inserts a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual async Task InsertLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            await _languageRepository.InsertAsync(language);

            //cache
            await _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(language);
        }

        /// <summary>
        /// Updates a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual async Task UpdateLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            
            //update language
            await _languageRepository.UpdateAsync(language);

            //cache
            await _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(language);
        }

        #endregion
    }
}
