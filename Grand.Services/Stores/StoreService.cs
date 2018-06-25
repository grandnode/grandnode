using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Stores;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Bson;

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
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STORES_PATTERN_KEY = "Grand.stores.";

        #endregion
        
        #region Fields
        
        private readonly IRepository<Store> _storeRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="storeRepository">Store repository</param>
        /// <param name="eventPublisher">Event published</param>
        public StoreService(ICacheManager cacheManager,
            IRepository<Store> storeRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._storeRepository = storeRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void DeleteStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            var allStores = GetAllStores();
            if (allStores.Count == 1)
                throw new Exception("You cannot delete the only configured store");

            _storeRepository.Delete(store);

            //clear cache
            _cacheManager.Clear();

            //event notification
            _eventPublisher.EntityDeleted(store);
        }

        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>Stores</returns>
        public virtual IList<Store> GetAllStores()
        {
            string key = STORES_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                return _storeRepository.Collection.Find(new BsonDocument()).SortBy(x => x.DisplayOrder).ToList();
            });
        }

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        public virtual Store GetStoreById(string storeId)
        {
            string key = string.Format(STORES_BY_ID_KEY, storeId);
            return _cacheManager.Get(key, () => _storeRepository.GetById(storeId));
        }

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void InsertStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Insert(store);

            //clear cache
            _cacheManager.Clear();

            //event notification
            _eventPublisher.EntityInserted(store);
        }

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void UpdateStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Update(store);

            //clear cache
            _cacheManager.Clear();

            //event notification
            _eventPublisher.EntityUpdated(store);
        }

        /// <summary>
        /// Gets a store mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>store mapping</returns>
        public virtual IList<Store> GetAllStoresByDiscount(string discountId)
        {
            var query = from c in _storeRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;
            var stores = query.ToList();
            return stores;
        }
        #endregion
    }
}