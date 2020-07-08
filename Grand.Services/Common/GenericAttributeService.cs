using Grand.Core;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        #endregion

        #region Ctor
        public GenericAttributeService(
            IRepository<BaseEntity> baseRepository,
            IRepository<GenericAttributeBaseEntity> genericattributeBaseEntitRepository)
        {
            _baseRepository = baseRepository;
            _genericattributeBaseEntitRepository = genericattributeBaseEntitRepository;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity name (collection name)</param>
        /// <param name="entityId">EntityId</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        public virtual async Task SaveAttribute<TPropType>(string entity, string entityId, string key, TPropType value, string storeId = "")
        {
            if (string.IsNullOrEmpty(entity))
                throw new ArgumentNullException("entity");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            var collection = _baseRepository.Database.GetCollection<GenericAttributeBaseEntity>(entity);
            var query = _baseRepository.Database.GetCollection<GenericAttributeBaseEntity>(entity).Find(new BsonDocument("_id", entityId)).FirstOrDefault();

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
                    var updatefilter = builder.PullFilter(x => x.GenericAttributes, y => y.Key == prop.Key && y.StoreId == storeId);
                    await collection.UpdateManyAsync(new BsonDocument("_id", entityId), updatefilter);
                }
                else
                {
                    //update
                    prop.Value = valueStr;
                    var builder = Builders<GenericAttributeBaseEntity>.Filter;
                    var filter = builder.Eq(x => x.Id, entityId);
                    filter = filter & builder.Where(x => x.GenericAttributes.Any(y => y.Key == prop.Key && y.StoreId == storeId));
                    var update = Builders<GenericAttributeBaseEntity>.Update
                        .Set(x => x.GenericAttributes.ElementAt(-1).Value, prop.Value);
                    await collection.UpdateManyAsync(filter, update);
                }
            }
            else
            {
                prop = new GenericAttribute {
                    Key = key,
                    Value = valueStr,
                    StoreId = storeId,
                };
                var updatebuilder = Builders<GenericAttributeBaseEntity>.Update;
                var update = updatebuilder.AddToSet(p => p.GenericAttributes, prop);
                await collection.UpdateOneAsync(new BsonDocument("_id", entityId), update);
            }
        }

        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        public virtual async Task SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "")
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
                    var updatefilter = builder.PullFilter(x => x.GenericAttributes, y => y.Key == prop.Key && y.StoreId == storeId);
                    await collection.UpdateManyAsync(new BsonDocument("_id", entity.Id), updatefilter);
                    var entityProp = entity.GenericAttributes.FirstOrDefault(x => x.Key == prop.Key && x.StoreId == storeId);
                    if (entityProp != null)
                        entity.GenericAttributes.Remove(entityProp);
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

                    await collection.UpdateManyAsync(filter, update);

                    var entityProp = entity.GenericAttributes.FirstOrDefault(x => x.Key == prop.Key && x.StoreId == storeId);
                    if (entityProp != null)
                        entityProp.Value = valueStr;

                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(valueStr))
                {
                    prop = new GenericAttribute {
                        Key = key,
                        Value = valueStr,
                        StoreId = storeId,
                    };
                    var updatebuilder = Builders<GenericAttributeBaseEntity>.Update;
                    var update = updatebuilder.AddToSet(p => p.GenericAttributes, prop);
                    await collection.UpdateOneAsync(new BsonDocument("_id", entity.Id), update);
                    entity.GenericAttributes.Add(prop);
                }
            }
        }

        public virtual async Task<TPropType> GetAttributesForEntity<TPropType>(BaseEntity entity, string key, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var collection = await _genericattributeBaseEntitRepository.Database.GetCollection<GenericAttributeBaseEntity>(entity.GetType().Name)
                .FindAsync(new BsonDocument("_id", entity.Id));

            var props = collection.FirstOrDefault().GenericAttributes;
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