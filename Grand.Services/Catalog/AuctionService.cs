using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Auction service
    /// </summary>
    public partial class AuctionService : IAuctionService
    {
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        private readonly IRepository<Bid> _bidRepository;
        private readonly IProductService _productService;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        public AuctionService(IRepository<Bid> bidRepository,
            IProductService productService,
            IRepository<Product> productRepository,
            ICacheManager cacheManager,
            IMediator mediator)
        {
            _bidRepository = bidRepository;
            _productService = productService;
            _productRepository = productRepository;
            _cacheManager = cacheManager;
            _mediator = mediator;
        }

        public virtual async Task DeleteBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            await _bidRepository.DeleteAsync(bid);
            await _mediator.EntityDeleted(bid);

            var productToUpdate = await _productService.GetProductById(bid.ProductId, true);
            var _bid = await GetBidsByProductId(bid.ProductId);
            var highestBid = _bid.OrderByDescending(x => x.Amount).FirstOrDefault();
            if (productToUpdate != null)
            {
                await UpdateHighestBid(productToUpdate, highestBid != null ? highestBid.Amount : 0, highestBid != null ? highestBid.CustomerId : "");
            }
        }

        public virtual Task<Bid> GetBid(string Id)
        {
            return _bidRepository.GetByIdAsync(Id);
        }

        public virtual Task<Bid> GetLatestBid(string productId)
        {
            var builder = Builders<Bid>.Filter;
            var filter = builder.Eq(x => x.ProductId, productId);
            var bid = _bidRepository.Collection.Find(filter).SortByDescending(x => x.Date).FirstOrDefaultAsync();
            return bid;
        }

        public virtual async Task<IPagedList<Bid>> GetBidsByProductId(string productId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _bidRepository.Table.Where(x => x.ProductId == productId).OrderByDescending(x => x.Date);
            return await Task.FromResult(new PagedList<Bid>(query, pageIndex, pageSize));
        }

        public virtual async Task<IPagedList<Bid>> GetBidsByCustomerId(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _bidRepository.Table.Where(x => x.CustomerId == customerId);
            return await Task.FromResult(new PagedList<Bid>(query, pageIndex, pageSize));
        }

        public virtual async Task InsertBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            await _bidRepository.InsertAsync(bid);
            await _mediator.EntityInserted(bid);
        }

        public virtual async Task UpdateBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            await _bidRepository.UpdateAsync(bid);
            await _mediator.EntityUpdated(bid);
        }

        public virtual async Task UpdateHighestBid(Product product, decimal bid, string highestBidder)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update.Set(x => x.HighestBid, bid).Set(x => x.HighestBidder, highestBidder);

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            await _mediator.EntityUpdated(product);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
        }

        public virtual async Task<IList<Product>> GetAuctionsToEnd()
        {
            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;
            filter &= builder.Where(x => x.ProductTypeId == (int)ProductType.Auction && !x.AuctionEnded && x.AvailableEndDateTimeUtc < DateTime.UtcNow);
            return await _productRepository.Collection.Find(filter).ToListAsync();
        }

        public virtual async Task UpdateAuctionEnded(Product product, bool ended, bool enddate = false)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var updateDefinition = Builders<Product>.Update;
            var update = updateDefinition.Set(x => x.AuctionEnded, ended).CurrentDate("UpdatedOnUtc");
            if (enddate)
                update = update.Set(x => x.AvailableEndDateTimeUtc, DateTime.UtcNow);

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            await _mediator.EntityUpdated(product);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
        }


        /// <summary>
        /// New bid
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="product"></param>
        /// <param name="store"></param>
        /// <param name="warehouseId"></param>
        /// <param name="language"></param>
        /// <param name="amount"></param>
        public virtual async Task NewBid(Customer customer, Product product, Store store, Language language, string warehouseId, decimal amount)
        {
            var latestbid = await GetLatestBid(product.Id);
            await InsertBid(new Bid {
                Date = DateTime.UtcNow,
                Amount = amount,
                CustomerId = customer.Id,
                ProductId = product.Id,
                StoreId = store.Id,
                WarehouseId = warehouseId,
            });

            if (latestbid != null)
            {
                if (latestbid.CustomerId != customer.Id)
                {
                    await _mediator.Send(new SendOutBidCustomerNotificationCommand() {
                        Product = product,
                        Bid = latestbid,
                        Language = language
                    });
                }
            }
            product.HighestBid = amount;
            await UpdateHighestBid(product, amount, customer.Id);
        }

        /// <summary>
        /// Cancel bid
        /// </summary>
        /// <param name="OrderId">OrderId</param>
        public virtual async Task CancelBidByOrder(string orderId)
        {
            var builder = Builders<Bid>.Filter;
            var filter = builder.Eq(x => x.OrderId, orderId);
            var bid = _bidRepository.Collection.Find(filter).FirstOrDefault();
            if (bid != null)
            {
                var filterDelete = builder.Eq(x => x.ProductId, bid.ProductId);
                await _bidRepository.Collection.DeleteManyAsync(filterDelete);
                var product = await _productService.GetProductById(bid.ProductId);
                if (product != null)
                {
                    await UpdateHighestBid(product, 0, "");
                    await UpdateAuctionEnded(product, false);
                }
            }
        }
    }
}