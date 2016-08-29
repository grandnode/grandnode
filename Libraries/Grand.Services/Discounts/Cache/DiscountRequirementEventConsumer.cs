﻿using Grand.Core.Caching;
using Grand.Core.Domain.Discounts;
using Grand.Core.Events;
using Grand.Core.Infrastructure;
using Grand.Services.Events;

namespace Grand.Services.Discounts.Cache
{
    /// <summary>
    /// Cache event consumer (used for caching of discount requirements)
    /// </summary>
    public partial class DiscountRequirementEventConsumer :
        //discounts
        IConsumer<EntityUpdated<Discount>>,
        IConsumer<EntityDeleted<Discount>>

        //discount requirements
        //IConsumer<EntityInserted<DiscountRequirement>>,
        //IConsumer<EntityUpdated<DiscountRequirement>>,
        //IConsumer<EntityDeleted<DiscountRequirement>>
    {
        /// <summary>
        /// Key for discount requirement of a certain discount
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// </remarks>
        public const string DISCOUNT_REQUIREMENT_MODEL_KEY = "Nop.discountrequirements.all-{0}";
        public const string DISCOUNT_REQUIREMENT_PATTERN_KEY = "Nop.discountrequirements";

        private readonly ICacheManager _cacheManager;

        public DiscountRequirementEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>();
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

        //discount requirements
        /*
        public void HandleEvent(EntityInserted<DiscountRequirement> eventMessage)
        {
            _cacheManager.RemoveByPattern(DISCOUNT_REQUIREMENT_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<DiscountRequirement> eventMessage)
        {
            _cacheManager.RemoveByPattern(DISCOUNT_REQUIREMENT_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<DiscountRequirement> eventMessage)
        {
            _cacheManager.RemoveByPattern(DISCOUNT_REQUIREMENT_PATTERN_KEY);
        }
        */
    }
}
