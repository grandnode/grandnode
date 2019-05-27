using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Configuration;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Orders;
using Grand.Core.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Catalog.Cache
{
    /// <summary>
    /// Price cache event consumer (used for caching of prices)
    /// </summary>
    public abstract class PriceCacheEventConsumer :
        //settings
        INotificationHandler<EntityUpdated<Setting>>,
        //categories
        INotificationHandler<EntityInserted<Category>>,
        INotificationHandler<EntityUpdated<Category>>,
        INotificationHandler<EntityDeleted<Category>>,
        //manufacturers
        INotificationHandler<EntityInserted<Manufacturer>>,
        INotificationHandler<EntityUpdated<Manufacturer>>,
        INotificationHandler<EntityDeleted<Manufacturer>>,
        //discounts
        INotificationHandler<EntityInserted<Discount>>,
        INotificationHandler<EntityUpdated<Discount>>,
        INotificationHandler<EntityDeleted<Discount>>,
        //product categories
        INotificationHandler<EntityInserted<ProductCategory>>,
        INotificationHandler<EntityUpdated<ProductCategory>>,
        INotificationHandler<EntityDeleted<ProductCategory>>,
        //product manufacturers
        INotificationHandler<EntityInserted<ProductManufacturer>>,
        INotificationHandler<EntityUpdated<ProductManufacturer>>,
        INotificationHandler<EntityDeleted<ProductManufacturer>>,

        //products
        INotificationHandler<EntityInserted<Product>>,
        INotificationHandler<EntityUpdated<Product>>,
        INotificationHandler<EntityDeleted<Product>>,
        //tier prices
        INotificationHandler<EntityInserted<TierPrice>>,
        INotificationHandler<EntityUpdated<TierPrice>>,
        INotificationHandler<EntityDeleted<TierPrice>>,
        //orders
        INotificationHandler<EntityInserted<Order>>,
        INotificationHandler<EntityUpdated<Order>>,
        INotificationHandler<EntityDeleted<Order>>
    {
        /// <summary>
        /// Key for product prices
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : additional charge
        /// {2} : include discounts (true, false)
        /// {3} : quantity
        /// {4} : roles of the current user
        /// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_PRICE_MODEL_KEY = "Grand.totals.productprice-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_PRICE_PATTERN_KEY = "Grand.totals.productprice";

        /// <summary>
        /// Key for category IDs of a discount
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string DISCOUNT_CATEGORY_IDS_MODEL_KEY = "Grand.totals.discount.categoryids-{0}-{1}-{2}";
        public const string DISCOUNT_CATEGORY_IDS_PATTERN_KEY = "Grand.totals.discount.categoryids";

        /// <summary>
        /// Key for manufacturer IDs of a discount
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string DISCOUNT_MANUFACTURER_IDS_MODEL_KEY = "Grand.totals.discount.manufacturerids-{0}-{1}-{2}";
        public const string DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY = "Grand.totals.discount.manufacturerids";

        /// <summary>
        /// Key for category IDs of a product
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string DISCOUNT_PRODUCT_CATEGORY_IDS_MODEL_KEY = "Grand.totals.product.categoryids-{0}-{1}-{2}";
        public const string DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY = "Grand.totals.product.categoryids";

        /// <summary>
        /// Key for manufacturer IDs of a product
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string DISCOUNT_PRODUCT_MANUFACTURER_IDS_MODEL_KEY = "Grand.totals.product.manufacturerids-{0}-{1}-{2}";
        public const string DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY = "Grand.totals.product.manufacturerids";

        private readonly ICacheManager _cacheManager;

        public PriceCacheEventConsumer(IServiceProvider serviceProvider)
        {
            _cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        }

        public PriceCacheEventConsumer()
        {
        }

        //settings
        public async Task Handle(EntityUpdated<Setting> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }

        //categories
        public async Task Handle(EntityInserted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }

        public async Task Handle(EntityUpdated<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }

        public async Task Handle(EntityDeleted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }

        //manufacturers
        public async Task Handle(EntityInserted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }

        //discounts
        public async Task Handle(EntityInserted<Discount> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Discount> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Discount> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(DISCOUNT_CATEGORY_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_MANUFACTURER_IDS_PATTERN_KEY);
        }

        //product categories
        public async Task Handle(EntityInserted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_CATEGORY_IDS_PATTERN_KEY);
        }

        //product manufacturers
        public async Task Handle(EntityInserted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(DISCOUNT_PRODUCT_MANUFACTURER_IDS_PATTERN_KEY);
        }

        //products
        public async Task Handle(EntityInserted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }

        //tier prices
        public async Task Handle(EntityInserted<TierPrice> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TierPrice> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TierPrice> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }

        //orders
        public async Task Handle(EntityInserted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_PRICE_PATTERN_KEY);
        }

    }
}
