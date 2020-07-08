using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Services.Events;
using Grand.Services.Localization;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Directory
{
    /// <summary>
    /// Country service
    /// </summary>
    public partial class CountryService : ICountryService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : show hidden records?
        /// </remarks>
        private const string COUNTRIES_ALL_KEY = "Grand.country.all-{0}-{1}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// </remarks>
        private static string COUNTRIES_BY_KEY = "Grand.country.id-{0}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : twoletter
        /// </remarks>
        private static string COUNTRIES_BY_TWOLETTER = "Grand.country.twoletter-{0}";

        /// <summary>
        /// key for caching by country id
        /// </summary>
        /// <remarks>
        /// {0} : threeletter
        /// </remarks>
        private static string COUNTRIES_BY_THREELETTER = "Grand.country.threeletter-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string COUNTRIES_PATTERN_KEY = "Grand.country.";

        #endregion

        #region Fields

        private readonly IRepository<Country> _countryRepository;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="countryRepository">Country repository</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="mediator">Mediator</param>
        public CountryService(ICacheManager cacheManager,
            IRepository<Country> countryRepository,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _countryRepository = countryRepository;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual async Task DeleteCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            await _countryRepository.DeleteAsync(country);

            await _cacheManager.RemoveByPrefix(COUNTRIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(country);
        }

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Countries</returns>
        public virtual async Task<IList<Country>> GetAllCountries(string languageId = "", bool showHidden = false)
        {
            string key = string.Format(COUNTRIES_ALL_KEY, languageId, showHidden);

            return await _cacheManager.GetAsync(key, async () =>
            {
                var builder = Builders<Country>.Filter;
                var filter = builder.Empty;

                if (!showHidden)
                    filter = filter & builder.Where(c => c.Published);

                if (!showHidden && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                    filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                }
                var countries = await _countryRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();
                if (!string.IsNullOrEmpty(languageId))
                {
                    //we should sort countries by localized names when they have the same display order
                    countries = countries
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.GetLocalized(x => x.Name, languageId))
                        .ToList();
                }
                return countries;
            });
        }

        /// <summary>
        /// Gets all countries that allow billing
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Countries</returns>
        public virtual async Task<IList<Country>> GetAllCountriesForBilling(string languageId = "", bool showHidden = false)
        {
            var countries = await GetAllCountries(languageId, showHidden);
            return countries.Where(c => c.AllowsBilling).ToList();
        }

        /// <summary>
        /// Gets all countries that allow shipping
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Countries</returns>
        public virtual async Task<IList<Country>> GetAllCountriesForShipping(string languageId = "", bool showHidden = false)
        {
            var countries = await GetAllCountries(languageId, showHidden);
            return countries.Where(c => c.AllowsShipping).ToList();
        }

        /// <summary>
        /// Gets a country 
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Country</returns>
        public virtual async Task<Country> GetCountryById(string countryId)
        {
            if (string.IsNullOrEmpty(countryId))
                return null;

            var key = string.Format(COUNTRIES_BY_KEY, countryId);
            return await _cacheManager.GetAsync(key, () => _countryRepository.GetByIdAsync(countryId));
        }

        /// <summary>
        /// Get countries by identifiers
        /// </summary>
        /// <param name="countryIds">Country identifiers</param>
        /// <returns>Countries</returns>
        public virtual async Task<IList<Country>> GetCountriesByIds(string[] countryIds)
        {
            if (countryIds == null || countryIds.Length == 0)
                return new List<Country>();

            var query = from c in _countryRepository.Table
                        where countryIds.Contains(c.Id)
                        select c;
            var countries = await query.ToListAsync();
            //sort by passed identifiers
            var sortedCountries = new List<Country>();
            foreach (string id in countryIds)
            {
                var country = countries.Find(x => x.Id == id);
                if (country != null)
                    sortedCountries.Add(country);
            }
            return sortedCountries;
        }

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        /// <param name="twoLetterIsoCode">Country two letter ISO code</param>
        /// <returns>Country</returns>
        public virtual Task<Country> GetCountryByTwoLetterIsoCode(string twoLetterIsoCode)
        {
            var key = string.Format(COUNTRIES_BY_TWOLETTER, twoLetterIsoCode);
            return _cacheManager.GetAsync(key, () =>
            {
                var filter = Builders<Country>.Filter.Eq(x => x.TwoLetterIsoCode, twoLetterIsoCode);
                return _countryRepository.Collection.Find(filter).FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        /// <param name="threeLetterIsoCode">Country three letter ISO code</param>
        /// <returns>Country</returns>
        public virtual Task<Country> GetCountryByThreeLetterIsoCode(string threeLetterIsoCode)
        {
            var key = string.Format(COUNTRIES_BY_THREELETTER, threeLetterIsoCode);
            return _cacheManager.GetAsync(key, () =>
            {
                var filter = Builders<Country>.Filter.Eq(x => x.ThreeLetterIsoCode, threeLetterIsoCode);
                return _countryRepository.Collection.Find(filter).FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Inserts a country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual async Task InsertCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            await _countryRepository.InsertAsync(country);

            await _cacheManager.RemoveByPrefix(COUNTRIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(country);
        }

        /// <summary>
        /// Updates the country
        /// </summary>
        /// <param name="country">Country</param>
        public virtual async Task UpdateCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            await _countryRepository.UpdateAsync(country);

            await _cacheManager.RemoveByPrefix(COUNTRIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(country);
        }

        #endregion
    }
}