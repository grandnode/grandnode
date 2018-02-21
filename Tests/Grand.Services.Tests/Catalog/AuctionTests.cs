using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class AuctionTests
    {
        private IRepository<Product> _productRepository;
        private IRepository<Bid> _bidRepository;
        private IAuctionService _auctionService;
        private IEventPublisher _eventPublisher;
        private IProductService _productService;
        private ICacheManager _cacheManager;

        [TestInitialize()]
        public void TestInitialize()
        {
            _productRepository = new MongoDBRepositoryTest<Product>();
            _bidRepository = new MongoDBRepositoryTest<Bid>();

            var eventPublisher = new Mock<IEventPublisher>();
            eventPublisher.Setup(x => x.Publish(new object()));
            _eventPublisher = eventPublisher.Object;

            _productService = new Mock<IProductService>().Object;
            _cacheManager = new Mock<ICacheManager>().Object;

            _auctionService = new AuctionService(_bidRepository, _eventPublisher, _productService, _productRepository, _cacheManager);

            _productRepository.Insert(new Product
            {
                ProductType = ProductType.Auction,
                Name = "Temp"
            });
        }

        [TestMethod()]
        public void CanInsertBid()
        {
            DateTime date = DateTime.UtcNow;

            _auctionService.InsertBid(new Bid
            {
                Amount = 1,
                Bin = true,
                CustomerId = "CustomerId",
                Date = date,
                OrderId = "OrderId",
                ProductId = "ProductId",
                StoreId = "StoreId",
                Win = true
            });

            var inserted = _bidRepository.Table.First();

            Assert.AreEqual(1, inserted.Amount);
            Assert.AreEqual(true, inserted.Bin);
            Assert.AreEqual("CustomerId", inserted.CustomerId);
            Assert.AreEqual(date.Date, inserted.Date.Date);
            Assert.AreEqual("OrderId", inserted.OrderId);
            Assert.AreEqual("ProductId", inserted.ProductId);
            Assert.AreEqual("StoreId", inserted.StoreId);
            Assert.AreEqual(true, inserted.Win);
            Assert.AreNotEqual(null, inserted.Id);
            Assert.AreNotEqual("", inserted.Id);
        }

        [TestMethod()]
        public void CanDeleteBid()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanDeleteBid"
            });

            var bid = _bidRepository.Table.Where(x => x.CustomerId == "CanDeleteBid").First();
            _auctionService.DeleteBid(bid);
            var deleted = _bidRepository.Table.Where(x => x.CustomerId == "CanDeleteBid").FirstOrDefault();

            Assert.AreEqual(null, deleted);
        }

        [TestMethod()]
        public void CanGetBid()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBid"
            });

            var bid = _bidRepository.Table.Where(x => x.CustomerId == "CanGetBid").First();
            var found = _auctionService.GetBid(bid.Id);

            Assert.AreEqual(bid.CustomerId, found.CustomerId);
            Assert.AreEqual(bid.Id, found.Id);
        }

        [TestMethod()]
        public void CanGetLatestBid()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetLatestBidC",
                ProductId = "CanGetLatestBidP"
            });

            var found = _auctionService.GetLatestBid("CanGetLatestBidP");

            Assert.AreEqual("CanGetLatestBidC", found.CustomerId);
        }

        [TestMethod()]
        public void CanGetBidsByProductId()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByProductId1",
                ProductId = "CanGetBidsByProductId"
            });

            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByProductId2",
                ProductId = "CanGetBidsByProductId"
            });

            var found = _auctionService.GetBidsByProductId("CanGetBidsByProductId");

            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByProductId1").Count());
            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByProductId2").Count());
        }

        [TestMethod()]
        public void CanGetBidsByCustomerId()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByCustomerId1",
                ProductId = "CanGetBidsByCustomerId"
            });

            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByCustomerId2",
                ProductId = "CanGetBidsByCustomerId"
            });

            var found = _auctionService.GetBidsByCustomerId("CanGetBidsByCustomerId1");

            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByCustomerId1").Count());
            Assert.AreEqual(0, found.Where(x => x.CustomerId == "CanGetBidsByCustomerId2").Count());
        }

        [TestMethod()]
        public void CanUpdateBid()
        {
            _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanUpdateBid"
            });

            var before = _auctionService.GetBidsByCustomerId("CanUpdateBid").First();
            before.CustomerId = "CanUpdateBid2";
            _auctionService.UpdateBid(before);
            var after = _auctionService.GetBid(before.Id);

            Assert.AreEqual("CanUpdateBid2", after.CustomerId);
        }

        [TestMethod()]
        public void CanUpdateHighestBid()
        {
            Product p = new Product();
            p.Name = "CanUpdateHighestBid";
            p.HighestBid = 10;
            p.HighestBidder = "a";
            _productRepository.Insert(p);

            _auctionService.UpdateHighestBid(p, 15, "b");
            var updated = _productRepository.Table.Where(x => x.Name == "CanUpdateHighestBid").First();

            Assert.AreEqual(15, updated.HighestBid);
            Assert.AreEqual("b", updated.HighestBidder);
        }

        [TestMethod()]
        public void CanGetAuctionsToEnd()
        {
            Product p = new Product();
            p.Name = "CanGetAuctionsToEnd";
            p.AvailableEndDateTimeUtc = DateTime.Now.AddDays(-5);
            p.ProductType = ProductType.Auction;
            _productRepository.Insert(p);

            var found = _auctionService.GetAuctionsToEnd();

            Assert.AreEqual(1, found.Count);
        }

        [TestMethod()]
        public void CanUpdateAuctionEnded()
        {
            Product p = new Product();
            p.Name = "CanUpdateAuctionEnded";
            p.AvailableEndDateTimeUtc = DateTime.Now.AddDays(5);
            p.ProductType = ProductType.Auction;
            _productRepository.Insert(p);

            var before = _productRepository.Table.Where(x => x.Name == "CanUpdateAuctionEnded").First();
            _auctionService.UpdateAuctionEnded(before, true, true);
            var after = _productRepository.Table.Where(x => x.Name == "CanUpdateAuctionEnded").First();

            Assert.AreEqual(true, after.AuctionEnded);
            Assert.AreEqual(DateTime.UtcNow.Date, after.AvailableEndDateTimeUtc.Value.Date);
        }

        [TestMethod()]
        public void CanCancelBidByOrder()
        {
            _productService.InsertProduct(new Product { Name = "test", Sku = "SKU" });
            var v = _productService.GetProductBySku("SKU");


            Product p = new Product();
            p.Name = "CanCancelBidByOrder";
            p.AvailableEndDateTimeUtc = DateTime.Now.AddDays(5);
            p.ProductType = ProductType.Auction;
            p.HighestBid = 10;
            p.HighestBidder = "H";
            _productRepository.Insert(p);

            Bid bid = new Bid();
            bid.Amount = 1;
            bid.OrderId = "o";
            bid.ProductId = p.Id;
            _bidRepository.Insert(bid);
            var xx = _productService.GetProductById(p.Id);
            _auctionService.CancelBidByOrder("o");

            var found = _auctionService.GetBidsByProductId("CanCancelBidByOrder");
            var product = _productRepository.Table.Where(x => x.Name == "CanCancelBidByOrder").FirstOrDefault();

            Assert.AreEqual(0, found.Count);
            //Assert.AreEqual(0, product.HighestBid);
            //Assert.AreEqual("", product.HighestBidder);
            //Assert.AreEqual(false, product.AuctionEnded);
        }
    }
}
