using System;
using System.Collections.Generic;
using Grand.Core.Domain.Catalog;
using Grand.Core.Data;
using Grand.Services.Events;
using MongoDB.Driver;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Auction service
    /// </summary>
    public partial class AuctionService : IAuctionService
    {
        private readonly IRepository<Bid> _bidRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProductService _productService;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;

        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        public AuctionService(IRepository<Bid> bidRepository,
            IEventPublisher eventPublisher,
            IProductService productService,
            IRepository<Product> productRepository,
            ICacheManager cacheManager)
        {
            this._bidRepository = bidRepository;
            this._eventPublisher = eventPublisher;
            this._productService = productService;
            this._productRepository = productRepository;
            this._cacheManager = cacheManager;
        }

        public virtual void DeleteBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            _bidRepository.Delete(bid);
            _eventPublisher.EntityDeleted(bid);

            var productToUpdate = _productService.GetProductById(bid.ProductId);
            var highestBid = GetBidsByProductId(bid.ProductId).OrderByDescending(x => x.Amount).FirstOrDefault();
            if (productToUpdate != null)
            {
                UpdateHighestBid(productToUpdate, highestBid != null ? highestBid.Amount: 0, highestBid != null ? highestBid.CustomerId : "");
            }
        }

        public virtual Bid GetBid(string Id)
        {
            return _bidRepository.GetById(Id);
        }

        public virtual Bid GetLatestBid(string productId)
        {
            var builder = Builders<Bid>.Filter;
            var filter = builder.Eq(x => x.ProductId, productId);
            var bid = _bidRepository.Collection.Find(filter).SortByDescending(x => x.Date).FirstOrDefault();
            return bid;
        }

        public virtual IPagedList<Bid> GetBidsByProductId(string productId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _bidRepository.Table.Where(x => x.ProductId == productId).OrderByDescending(x => x.Date);
            return new PagedList<Bid>(query, pageIndex, pageSize);
        }

        public virtual IPagedList<Bid> GetBidsByCustomerId(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _bidRepository.Table.Where(x => x.CustomerId == customerId);
            return new PagedList<Bid>(query, pageIndex, pageSize);
        }

        public virtual void InsertBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            _bidRepository.Insert(bid);
            _eventPublisher.EntityInserted(bid);
        }

        public virtual void UpdateBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");

            _bidRepository.Update(bid);
            _eventPublisher.EntityUpdated(bid);
        }

        public virtual void UpdateHighestBid(Product product, decimal bid, string highestBidder)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update.Set(x => x.HighestBid, bid).Set(x => x.HighestBidder, highestBidder);

            var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

            _eventPublisher.EntityUpdated(product);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
        }

        public virtual IList<Product> GetAuctionsToEnd()
        {
            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;
            filter = filter & builder.Where(x => x.ProductTypeId == (int)ProductType.Auction &&
            !x.AuctionEnded && x.AvailableEndDateTimeUtc < DateTime.UtcNow);
            var products = _productRepository.Collection.Find(filter).ToList();
            return products;
        }

        public virtual void UpdateAuctionEnded(Product product, bool ended)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update.Set(x => x.AuctionEnded, ended);

            var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

            _eventPublisher.EntityUpdated(product);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
        }
    }
}