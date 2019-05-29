using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Vendors;
using Grand.Core.Events;
using Grand.Services.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public class ModelCacheEventConsumer :
        //specification attributes
        INotificationHandler<EntityInserted<SpecificationAttribute>>,
        INotificationHandler<EntityUpdated<SpecificationAttribute>>,
        INotificationHandler<EntityDeleted<SpecificationAttribute>>,
        //categories
        INotificationHandler<EntityInserted<Category>>,
        INotificationHandler<EntityUpdated<Category>>,
        INotificationHandler<EntityDeleted<Category>>,
        //manufacturers
        INotificationHandler<EntityInserted<Manufacturer>>,
        INotificationHandler<EntityUpdated<Manufacturer>>,
        INotificationHandler<EntityDeleted<Manufacturer>>,
        //vendors
        INotificationHandler<EntityInserted<Vendor>>,
        INotificationHandler<EntityUpdated<Vendor>>,
        INotificationHandler<EntityDeleted<Vendor>>
    {

        /// <summary>
        /// Key for specification attributes caching (product details page)
        /// </summary>
        public const string SPEC_ATTRIBUTES_MODEL_KEY = "Grand.pres.admin.product.specs";
        public const string SPEC_ATTRIBUTES_PATTERN_KEY = "Grand.pres.admin.product.specs";

        /// <summary>
        /// Key for categories caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string CATEGORIES_LIST_KEY = "Grand.pres.admin.categories.list-{0}";
        public const string CATEGORIES_LIST_PATTERN_KEY = "Grand.pres.admin.categories.list";

        /// <summary>
        /// Key for manufacturers caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string MANUFACTURERS_LIST_KEY = "Grand.pres.admin.manufacturers.list-{0}";
        public const string MANUFACTURERS_LIST_PATTERN_KEY = "Grand.pres.admin.manufacturers.list";

        /// <summary>
        /// Key for vendors caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string VENDORS_LIST_KEY = "Grand.pres.admin.vendors.list-{0}";
        public const string VENDORS_LIST_PATTERN_KEY = "Grand.pres.admin.vendors.list";


        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer(ICacheManager cacheManager)
        {
            this._cacheManager = cacheManager;
        }

        //specification attributes
        public async Task Handle(EntityInserted<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SPEC_ATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SPEC_ATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SPEC_ATTRIBUTES_PATTERN_KEY);
        }

        //categories
        public async Task Handle(EntityInserted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }

        //manufacturers
        public async Task Handle(EntityInserted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURERS_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURERS_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURERS_LIST_PATTERN_KEY);
        }

        //vendors
        public async Task Handle(EntityInserted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
    }
}
