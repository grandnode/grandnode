using Grand.Core.Caching;
using Grand.Core.Caching.Constants;
using Grand.Domain.Data;
using Grand.Domain.Localization;
using Grand.Services.Events;
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
        #region Fields

        private readonly IRepository<Language> _languageRepository;
        private readonly ICacheBase _cacheBase;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="languageRepository">Language repository</param>
        /// <param name="mediator">Mediator</param>
        public LanguageService(ICacheBase cacheManager,
            IRepository<Language> languageRepository,
            IMediator mediator)
        {
            _cacheBase = cacheManager;
            _languageRepository = languageRepository;
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

            await _languageRepository.DeleteAsync(language);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.LANGUAGES_PATTERN_KEY);

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
            string key = string.Format(CacheKey.LANGUAGES_ALL_KEY, showHidden);
            var languages = await _cacheBase.GetAsync(key, () =>
            {
                var query = _languageRepository.Table;

                if (!showHidden)
                    query = query.Where(l => l.Published);
                query = query.OrderBy(l => l.DisplayOrder);
                return query.ToListAsync();
            });

            //store mapping
            if (!string.IsNullOrWhiteSpace(storeId))
            {
                languages = languages
                    .Where(l => l.Stores.Contains(storeId) || !l.LimitedToStores)
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
            string key = string.Format(CacheKey.LANGUAGES_BY_ID_KEY, languageId);
            return _cacheBase.GetAsync(key, () => _languageRepository.GetByIdAsync(languageId));
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
            await _cacheBase.RemoveByPrefix(CacheKey.LANGUAGES_PATTERN_KEY);

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
            await _cacheBase.RemoveByPrefix(CacheKey.LANGUAGES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(language);
        }

        #endregion
    }
}
