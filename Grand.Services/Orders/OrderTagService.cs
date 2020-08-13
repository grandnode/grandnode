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
using System.Runtime.CompilerServices;
using System.Text;
using Grand.Services.Events;

namespace Grand.Services.Orders
{
    public partial class OrderTagService : IOrderTagService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string ORDERTAG_COUNT_KEY = "Grand.ordertag.count-{0}";

        /// <summary>
        /// Key for all tags
        /// </summary>
        private const string ORDERTAG_ALL_KEY = "Grand.ordertag.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ORDERTAG_PATTERN_KEY = "Grand.ordertag.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : order ID
        /// </remarks>
        private const string ORDERS_BY_ID_KEY = "Grand.order.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        private const string ORDERS_PATTERN_KEY = "Grand.order.";


        #endregion

        #region Fields

        private readonly IRepository<OrderTag> _orderTagRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public OrderTagService(IRepository<OrderTag> orderTagRepository,
            IRepository<Order> orderRepository,
            ICacheManager cacheManager,
            IMediator mediator
            )
        {
            _orderTagRepository = orderTagRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Nested classes

        private class OrderTagWithCount
        {
            public int OrderTagId { get; set; }
            public int OrderCount { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get order's  count for each of existing order tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Dictionary of "order's tag ID : order's count"</returns>
        private async Task<Dictionary<string, int>> GetOrderCount(string storeId)
        {
            string key = string.Format(ORDERTAG_COUNT_KEY, storeId);
            return await _cacheManager.GetAsync(key, async () => 
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
            var updatefilter = builder.PullFilter(x => x.OrderTags, y => y.OrderId == orderTag.Id );
            await _orderRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _orderTagRepository.DeleteAsync(orderTag);

            //cache
            await _cacheManager.RemoveByPrefix(ORDERTAG_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ORDERS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(orderTag);
        }

        /// <summary>
        /// Gets all order tags
        /// </summary>
        /// <returns>Order tags</returns>
        public virtual async Task<IList<OrderTag>> GetAllOrderTags()
        {
            //return await _cacheManager.GetAsync(ORDERTAG_ALL_KEY, async () =>
            //{
                var query = _orderTagRepository.Table;
                return await query.ToListAsync();
            //});
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
        /// Gets order's tags by order
        /// </summary>
        /// <param name="orderId">Order's identifier</param>
        /// <returns>Order's tag</returns>
        public virtual async Task<IList<OrderTag>> GetOrderTagsByOrder(string orderId)
        {
            var result = await _orderTagRepository.Collection.FindAsync(p => p.Orders.Contains(orderId));
            return result.ToList();
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
            await _cacheManager.RemoveByPrefix(ORDERTAG_PATTERN_KEY);

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

            var previouse = await GetOrderTagById(orderTag.Id);

            await _orderTagRepository.UpdateAsync(orderTag);

            //update name on orders
            var filter = new BsonDocument
            {
                new BsonElement("OrderTags", previouse.Name)
            };
                        
            //cache
            await _cacheManager.RemoveByPrefix(ORDERTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(orderTag);
        }

        /// <summary>
        /// Attach a tag to the order
        /// </summary>
        /// <param name="orderTag">Order's picture</param>
        public virtual async Task AttachOrderTag(OrderTag orderTag, Order order)
        {
            if (orderTag == null)
                throw new ArgumentNullException("orderTag");

            // update  order with tags
            var updatebuilder = Builders<Order>.Update;
            var update = updatebuilder.AddToSet(p => p.OrderTags, new OrderOrderTags { OrderId = order.Id, OrderTagId = orderTag.Id });
            await _orderRepository.Collection.UpdateOneAsync(new BsonDocument("_id", order.Id), update);

            // update ordertag with count's order and new order id
            var builder = Builders<OrderTag>.Filter;
            var filter = builder.Eq(x => x.Id, orderTag.Id);
            var updateTag = Builders<OrderTag>.Update
                .Inc(x => x.Count, 1);
            await _orderTagRepository.Collection.UpdateManyAsync(filter, updateTag);

            //cache
            await _cacheManager.RemoveAsync(string.Format(ORDERS_BY_ID_KEY, order.Id));

            //event notification
            await _mediator.EntityInserted(orderTag);
        }

        // <summary>
        /// Detach a tag from the order
        /// </summary>
        /// <param name="orderTag">Order Tag</param>
        public virtual async Task DetachOrderTag(OrderTag orderTag, Order order)
        {
            if (orderTag == null)
                throw new ArgumentNullException("orderTag");

            var filterOrder = Builders<Order>.Filter.Where(o => o.Id == order.Id);
            var updateOrder = Builders<Order>.Update.PullFilter(p => p.OrderTags, Builders<OrderOrderTags>.Filter.Where(y => y.OrderTagId == orderTag.Id && y.OrderId == order.Id)); //y.OrderTagId == orderTag.Id && y.OrderId == order.Id);
            await _orderRepository.Collection.UpdateManyAsync(filterOrder, updateOrder);

            var builder = Builders<OrderTag>.Filter;
            var filter = builder.Eq(x => x.Id, orderTag.Id);
            var updateTag = Builders<OrderTag>.Update
                .Inc(x => x.Count, -1).PullFilter(p => p.Orders, y => y == order.Id);
            await _orderTagRepository.Collection.UpdateManyAsync(filter, updateTag);

            //cache
            await _cacheManager.RemoveAsync(string.Format(ORDERS_BY_ID_KEY, orderTag.OrderId));

            //event notification
            await _mediator.EntityDeleted(orderTag);
        }

        /// <summary>
        /// Get number of orders
        /// </summary>
        /// <param name="orderTagId">Order's tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of orders</returns>
        public virtual async Task<int> GetOrderCount(string orderTagId, string storeId)
        {
            var dictionary = await GetOrderCount(storeId);
            if (dictionary.ContainsKey(orderTagId))
                return dictionary[orderTagId];

            return 0;
        }

        #endregion
    }
}
