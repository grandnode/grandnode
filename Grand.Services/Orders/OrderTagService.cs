using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Orders;
using System.Threading.Tasks;
using MediatR;
using System.Linq;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Grand.Services.Events;
using Grand.Core.Caching.Constants;

namespace Grand.Services.Orders
{
    public partial class OrderTagService : IOrderTagService
    {
        
        #region Fields

        private readonly IRepository<OrderTag> _orderTagRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICacheBase _cacheBase;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public OrderTagService(IRepository<OrderTag> orderTagRepository,
            IRepository<Order> orderRepository,
            ICacheBase cacheManager,
            IMediator mediator
            )
        {
            _orderTagRepository = orderTagRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _cacheBase = cacheManager;
        }

        #endregion
                
        #region Utilities

        /// <summary>
        /// Get order's  count for each of existing order tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Dictionary of "order's tag ID : order's count"</returns>
        private async Task<Dictionary<string, int>> GetOrderCount(string orderTagId)
        {
            string key = string.Format(CacheKey.ORDERTAG_COUNT_KEY, orderTagId);
            return await _cacheBase.GetAsync(key, async () => 
            {
                var query = from ot in _orderTagRepository.Table
                            select ot;

                var dictionary = new Dictionary<string, int>();
                foreach (var tag in await query.ToListAsync())
                {
                    dictionary.Add(tag.Id, tag.Count);
                }
                return dictionary;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete an order's tag
        /// </summary>
        /// <param name="orderTag">Order's tag</param>
        public virtual async Task DeleteOrderTag(OrderTag orderTag)
        {
            if (orderTag == null)
                throw new ArgumentNullException("orderTag");

            var builder = Builders<Order>.Update;
            var updatefilter = builder.Pull(x => x.OrderTags, orderTag.Id);
            await _orderRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _orderTagRepository.DeleteAsync(orderTag);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.ORDERTAG_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ORDERS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(orderTag);
        }

        /// <summary>
        /// Gets all order tags
        /// </summary>
        /// <returns>Order tags</returns>
        public virtual async Task<IList<OrderTag>> GetAllOrderTags()
        {
            var query = _orderTagRepository.Table;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets order's tag by id
        /// </summary>
        /// <param name="orderTagId">Order's tag identifier</param>
        /// <returns>Order's tag</returns>
        public virtual Task<OrderTag> GetOrderTagById(string orderTagId)
        {
            return _orderTagRepository.GetByIdAsync(orderTagId);
        }

        /// <summary>
        /// Gets order's tag by name
        /// </summary>
        /// <param name="name">Order's tag name</param>
        /// <returns>Order's tag</returns>
        public virtual Task<OrderTag> GetOrderTagByName(string name)
        {
            var query = from pt in _orderTagRepository.Table
                        where pt.Name == name
                        select pt;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts a order's tag
        /// </summary>
        /// <param name="orderTag">Order's tag</param>
        public virtual async Task InsertOrderTag(OrderTag orderTag)
        {
            if (orderTag == null)
                throw new ArgumentNullException("orderTag");

            await _orderTagRepository.InsertAsync(orderTag);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.ORDERTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(orderTag);
        }

        /// <summary>
        /// Updating a order's tag
        /// </summary>
        /// <param name="orderTag">Order tag</param>
        public virtual async Task UpdateOrderTag(OrderTag orderTag)
        {
            if (orderTag == null)
                throw new ArgumentNullException("orderTag");

            await _orderTagRepository.UpdateAsync(orderTag);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.ORDERTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(orderTag);
        }

        /// <summary>
        /// Attach a tag to the order
        /// </summary>
        /// <param name="orderTag">Order's identification</param>
        public virtual async Task AttachOrderTag(string orderTagId, string orderId)
        {
            var updateBuilder = Builders<Order>.Update;
            var update = updateBuilder.AddToSet(p => p.OrderTags, orderTagId);
            await _orderRepository.Collection.UpdateOneAsync(new BsonDocument("_id", orderId), update);
            
            // update ordertag with count's order and new order id
            var updateBuilderTag = Builders<OrderTag>.Update
                .Inc(x => x.Count, 1);

            await _orderTagRepository.Collection.UpdateOneAsync(new BsonDocument("_id", orderTagId), updateBuilderTag);
            var orderTag =   await _orderTagRepository.GetByIdAsync(orderTagId);

            //cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.ORDERS_BY_ID_KEY, orderId));
            await _cacheBase.RemoveAsync(string.Format(CacheKey.ORDERTAG_COUNT_KEY, orderTagId));

            //event notification
            await _mediator.EntityUpdated(orderTag);
        }

        // <summary>
        /// Detach a tag from the order
        /// </summary>
        /// <param name="orderTag">Order Tag</param>
        public virtual async Task DetachOrderTag(string orderTagId, string orderId)
        {
            var updateBuilder = Builders<Order>.Update;
            var update = updateBuilder.Pull(p => p.OrderTags, orderTagId);
            await _orderRepository.Collection.UpdateOneAsync(new BsonDocument("_id", orderId), update);
            
            var updateTag = Builders<OrderTag>.Update
                .Inc(x => x.Count, -1);
            await _orderTagRepository.Collection.UpdateManyAsync(new BsonDocument("_id", orderTagId), updateTag);

            //cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.ORDERS_BY_ID_KEY, orderId));
            await _cacheBase.RemoveAsync(string.Format(CacheKey.ORDERTAG_COUNT_KEY, orderTagId));
        }

        /// <summary>
        /// Get number of orders
        /// </summary>
        /// <param name="orderTagId">Order's tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of orders</returns>
        public virtual async Task<int> GetOrderCount(string orderTagId, string storeId)
        {
            var dictionary = await GetOrderCount(orderTagId);
            if (dictionary.ContainsKey(orderTagId))
                return dictionary[orderTagId];

            return 0;
        }

        #endregion
    }
}
