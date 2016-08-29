﻿using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Events;
using Grand.Core.Infrastructure;
using Grand.Services.Events;

namespace Grand.Plugin.Misc.FacebookShop.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer: 
        //categories
        IConsumer<EntityInserted<Category>>,
        IConsumer<EntityUpdated<Category>>,
        IConsumer<EntityDeleted<Category>>
    {
        /// <summary>
        /// Key for CategoryNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_NAVIGATION_MODEL_KEY = "Nop.plugins.misc.facebookshop.category.navigation-{0}-{1}-{2}";
        public const string CATEGORY_NAVIGATION_PATTERN_KEY = "Nop.plugins.misc.facebookshop";

        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>();
        }

        //categories
         public void HandleEvent(EntityInserted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
        }
    }
}
