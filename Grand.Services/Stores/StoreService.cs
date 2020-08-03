using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Stores;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Stores
{
    /// <summary>
    /// Store service
    /// </summary>
    public partial class StoreService : IStoreService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string STORES_ALL_KEY = "Grand.stores.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string STORES_BY_ID_KEY = "Grand.stores.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Store> _storeRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        private List<Store> _allStores;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="storeRepository">Store repository</param>
        /// <param name="mediator">Mediator</param>
        public StoreService(ICacheManager cacheManager,
            IRepository<Store> storeRepository,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _storeRepository = storeRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual async Task DeleteStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            var allStores = await GetAllStores();
            if (allStores.Count == 1)
                throw new Exception("You cannot delete the only configured store");

            await _storeRepository.DeleteAsync(store);

            //clear cache
            await _cacheManager.Clear();

            //event notification
            await _mediator.EntityDeleted(store);
        }

        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>Stores</returns>
        public virtual async Task<IList<Store>> GetAllStores()
        {
            if (_allStores == null)
            {
                _allStores = await _cacheManager.GetAsync(STORES_ALL_KEY, () => {
                    return _storeRepository.Collection.Find(new BsonDocument()).SortBy(x => x.DisplayOrder).ToListAsync();
                });
            }
            return _allStores;
        }

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        public virtual Task<Store> GetStoreById(string storeId)
        {
            string key = string.Format(STORES_BY_ID_KEY, storeId);
            return _cacheManager.GetAsync(key, () => _storeRepository.GetByIdAsync(storeId));
        }

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual async Task InsertStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            await _storeRepository.InsertAsync(store);

            //clear cache
            await _cacheManager.Clear();

            //event notification
            await _mediator.EntityInserted(store);
        }

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual async Task UpdateStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            await _storeRepository.UpdateAsync(store);

            //clear cache
            await _cacheManager.Clear();

            //event notification
            await _mediator.EntityUpdated(store);
        }
        #endregion
    }
}