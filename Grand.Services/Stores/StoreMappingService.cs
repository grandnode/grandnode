using Grand.Core;
using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Stores;
using System;

namespace Grand.Services.Stores
{
    /// <summary>
    /// Store mapping service
    /// </summary>
    public partial class StoreMappingService : IStoreMappingService
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storeContext">Store context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public StoreMappingService(IStoreContext storeContext,
            CatalogSettings catalogSettings)
        {
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Authorize whether entity could be accessed in the current store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            return Authorize(entity, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, string storeId) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
                return false;

            if (String.IsNullOrEmpty(storeId))
                //return true if no store specified/found
                return true;

            if (_catalogSettings.IgnoreStoreLimitations)
                return true;

            if (!entity.LimitedToStores)
                return true;

            foreach (var storeIdWithAccess in entity.Stores)
                if (storeId == storeIdWithAccess)
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}