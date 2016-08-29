using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Common;
using Grand.Data;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generic attribute service
    /// </summary>
    public partial class GenericAttributeService : IGenericAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : key group
        /// </remarks>
        private const string GENERICATTRIBUTE_KEY = "Nop.genericattribute.{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string GENERICATTRIBUTE_PATTERN_KEY = "Nop.genericattribute.";
        #endregion

        #region Fields

        private readonly IRepository<BaseEntity> _baseRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="genericAttributeRepository">Generic attribute repository</param>
        /// <param name="eventPublisher">Event published</param>
        public GenericAttributeService(ICacheManager cacheManager,
            IRepository<BaseEntity> baseRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._baseRepository = baseRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        public virtual void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (key == null)
                throw new ArgumentNullException("key");

            string keyGroup = entity.GetType().Name;

            var collection = _baseRepository.Database.GetCollection<BaseEntity>(keyGroup);

            var props = entity.GenericAttributes != null ? entity.GenericAttributes.Where(x => x.StoreId == storeId) : new List<GenericAttribute>();
            
            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            var valueStr = CommonHelper.To<string>(value);

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    //delete
                    var builder = Builders<BaseEntity>.Update;
                    var updatefilter = builder.PullFilter(x => x.GenericAttributes, y => y.Key == prop.Key &&  y.StoreId == storeId);
                    var result = collection.UpdateManyAsync(new BsonDocument("_id", entity.Id), updatefilter).Result;
                }
                else
                {
                    //update
                    prop.Value = valueStr;
                    var builder = Builders<BaseEntity>.Filter;
                    var filter = builder.Eq(x => x.Id, entity.Id);
                    filter = filter & builder.Where(x => x.GenericAttributes.Any(y => y.Key == prop.Key));
                    var update = Builders<BaseEntity>.Update
                        .Set(x => x.GenericAttributes.ElementAt(-1).Value, prop.Value);

                    var result = collection.UpdateManyAsync(filter, update).Result;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(valueStr))
                {
                    prop = new GenericAttribute
                    {
                        Key = key,
                        Value = valueStr,
                        StoreId = storeId,
                    };
                    var updatebuilder = Builders<BaseEntity>.Update;
                    var update = updatebuilder.AddToSet(p => p.GenericAttributes, prop);
                    var result = collection.UpdateOneAsync(new BsonDocument("_id", entity.Id), update).Result;
                }
            }
        }

        #endregion
    }
}