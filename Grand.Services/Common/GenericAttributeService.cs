using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Common;
using Grand.Services.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generic attribute service
    /// </summary>
    public partial class GenericAttributeService : IGenericAttributeService
    {

        #region Fields

        private readonly IRepository<BaseEntity> _baseRepository;
        private readonly IRepository<GenericAttributeBaseEntity> _genericattributeBaseEntitRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="genericAttributeRepository">Generic attribute repository</param>
        /// <param name="GenericAttributeBaseEntity">Generic attribute base repository</param>
        /// <param name="eventPublisher">Event published</param>
        public GenericAttributeService(
            IRepository<BaseEntity> baseRepository,
            IRepository<GenericAttributeBaseEntity> genericattributeBaseEntitRepository,
            IEventPublisher eventPublisher)
        {
            this._baseRepository = baseRepository;
            this._genericattributeBaseEntitRepository = genericattributeBaseEntitRepository;
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

            var collection = _baseRepository.Database.GetCollection<GenericAttributeBaseEntity>(keyGroup);
            var query = _baseRepository.Database.GetCollection<GenericAttributeBaseEntity>(keyGroup).Find(new BsonDocument("_id", entity.Id)).FirstOrDefault();

            var props = query.GenericAttributes.Where(x => string.IsNullOrEmpty(storeId) || x.StoreId == storeId);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.OrdinalIgnoreCase)); //should be culture invariant

            var valueStr = CommonHelper.To<string>(value);

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    //delete
                    var builder = Builders<GenericAttributeBaseEntity>.Update;
                    var updatefilter = builder.PullFilter(x => x.GenericAttributes, y => y.Key == prop.Key &&  y.StoreId == storeId);
                    var result = collection.UpdateManyAsync(new BsonDocument("_id", entity.Id), updatefilter).Result;
                }
                else
                {
                    //update
                    prop.Value = valueStr;
                    var builder = Builders<GenericAttributeBaseEntity>.Filter;
                    var filter = builder.Eq(x => x.Id, entity.Id);
                    filter = filter & builder.Where(x => x.GenericAttributes.Any(y => y.Key == prop.Key && y.StoreId == storeId));
                    var update = Builders<GenericAttributeBaseEntity>.Update
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
                    var updatebuilder = Builders<GenericAttributeBaseEntity>.Update;
                    var update = updatebuilder.AddToSet(p => p.GenericAttributes, prop);
                    var result = collection.UpdateOneAsync(new BsonDocument("_id", entity.Id), update).Result;
                }
            }
        }

        public virtual TPropType GetAttributesForEntity<TPropType>(BaseEntity entity, string key, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var collection = _genericattributeBaseEntitRepository.Database.GetCollection<GenericAttributeBaseEntity>(entity.GetType().Name).Find(new BsonDocument("_id", entity.Id)).FirstOrDefault();

            var props = collection.GenericAttributes;
            if (props == null)
                return default(TPropType);
            props = props.Where(x => x.StoreId == storeId).ToList();
            if (!props.Any())
                return default(TPropType);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.OrdinalIgnoreCase)); 

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return default(TPropType);

            return CommonHelper.To<TPropType>(prop.Value);
        }

        #endregion
    }
}