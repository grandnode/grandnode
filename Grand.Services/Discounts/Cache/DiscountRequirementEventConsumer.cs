using Grand.Core.Caching;
using Grand.Core.Domain.Discounts;
using Grand.Core.Events;
using Grand.Services.Events;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grand.Services.Discounts.Cache
{
    /// <summary>
    /// Cache event consumer (used for caching of discount requirements)
    /// </summary>
    public partial class DiscountRequirementEventConsumer :
        //discounts
        IConsumer<EntityUpdated<Discount>>,
        IConsumer<EntityDeleted<Discount>>

    {
        /// <summary>
        /// Key for discount requirement of a certain discount
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// </remarks>
        public const string DISCOUNT_REQUIREMENT_MODEL_KEY = "Grand.discountrequirements.all-{0}";
        public const string DISCOUNT_REQUIREMENT_PATTERN_KEY = "Grand.discountrequirements";

        private readonly ICacheManager _cacheManager;

        public DiscountRequirementEventConsumer(IServiceProvider serviceProvider)
        {
            this._cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        }

        //discounts
        public void HandleEvent(EntityUpdated<Discount> eventMessage)
        {
            _cacheManager.RemoveByPattern(DISCOUNT_REQUIREMENT_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Discount> eventMessage)
        {
            _cacheManager.RemoveByPattern(DISCOUNT_REQUIREMENT_PATTERN_KEY);
        }

    }
}
