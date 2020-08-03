using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Events;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class AuctionTests
    {
        private IRepository<Product> _productRepository;
        private IRepository<Bid> _bidRepository;
        private IAuctionService _auctionService;
        private IMediator _eventPublisher;
        private IProductService _productService;
        private ICacheManager _cacheManager;
        private IServiceProvider _serviceProvider;

        [TestInitialize()]
        public void TestInitialize()
        {
            _productRepository = new MongoDBRepositoryTest<Product>();
            _bidRepository = new MongoDBRepositoryTest<Bid>();

            var eventPublisher = new Mock<IMediator>();
            //eventPublisher.Setup(x => x.PublishAsync(new object()));
            _eventPublisher = eventPublisher.Object;

            var productService = new Mock<IProductService>();
            _productService = productService.Object;

            _cacheManager = new Mock<ICacheManager>().Object;

            _serviceProvider = new Mock<IServiceProvider>().Object;

            _auctionService = new AuctionService(_bidRepository, _productService, _productRepository, _cacheManager, _eventPublisher);

            _productRepository.Insert(new Product
            {
                ProductType = ProductType.Auction,
                Name = "Temp"
            });
        }

        [TestMethod()]
        public async Task CanInsertBid()
        {
            DateTime date = DateTime.UtcNow;

            await _auctionService.InsertBid(new Bid
            {
                Amount = 1,
                Bin = true,
                CustomerId = "CustomerIdTest1",
                Date = date,
                OrderId = "OrderId",
                ProductId = "ProductId",
                StoreId = "StoreId",
                Win = true
            });

            var inserted = _bidRepository.Table.Where(x=>x.CustomerId == "CustomerIdTest1").First();

            Assert.AreEqual(1, inserted.Amount);
            Assert.AreEqual(true, inserted.Bin);
            Assert.AreEqual("CustomerIdTest1", inserted.CustomerId);
            Assert.AreEqual(date.Date, inserted.Date.Date);
            Assert.AreEqual("OrderId", inserted.OrderId);
            Assert.AreEqual("ProductId", inserted.ProductId);
            Assert.AreEqual("StoreId", inserted.StoreId);
            Assert.AreEqual(true, inserted.Win);
            Assert.AreNotEqual(null, inserted.Id);
            Assert.AreNotEqual("", inserted.Id);
        }

        [TestMethod()]
        public async Task CanDeleteBid()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanDeleteBid"
            });

            var bid = _bidRepository.Table.Where(x => x.CustomerId == "CanDeleteBid").First();
            await _auctionService.DeleteBid(bid);
            var deleted = _bidRepository.Table.Where(x => x.CustomerId == "CanDeleteBid").FirstOrDefault();

            Assert.AreEqual(null, deleted);
        }

        [TestMethod()]
        public async Task CanGetBid()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBid"
            });

            var bid = _bidRepository.Table.Where(x => x.CustomerId == "CanGetBid").First();
            var found = await _auctionService.GetBid(bid.Id);

            Assert.AreEqual(bid.CustomerId, found.CustomerId);
            Assert.AreEqual(bid.Id, found.Id);
        }

        [TestMethod()]
        public async Task CanGetLatestBid()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetLatestBidC",
                ProductId = "CanGetLatestBidP"
            });

            var found = await _auctionService.GetLatestBid("CanGetLatestBidP");

            Assert.AreEqual("CanGetLatestBidC", found.CustomerId);
        }

        [TestMethod()]
        public async Task CanGetBidsByProductId()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByProductId1",
                ProductId = "CanGetBidsByProductId"
            });

            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByProductId2",
                ProductId = "CanGetBidsByProductId"
            });

            var found = await _auctionService.GetBidsByProductId("CanGetBidsByProductId");

            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByProductId1").Count());
            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByProductId2").Count());
        }

        [TestMethod()]
        public async Task CanGetBidsByCustomerId()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByCustomerId1",
                ProductId = "CanGetBidsByCustomerId"
            });

            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanGetBidsByCustomerId2",
                ProductId = "CanGetBidsByCustomerId"
            });

            var found = await _auctionService.GetBidsByCustomerId("CanGetBidsByCustomerId1");

            Assert.AreEqual(1, found.Where(x => x.CustomerId == "CanGetBidsByCustomerId1").Count());
            Assert.AreEqual(0, found.Where(x => x.CustomerId == "CanGetBidsByCustomerId2").Count());
        }

        [TestMethod()]
        public async Task CanUpdateBid()
        {
            await _auctionService.InsertBid(new Bid
            {
                CustomerId = "CanUpdateBid"
            });

            var before = (await _auctionService.GetBidsByCustomerId("CanUpdateBid")).First();
            before.CustomerId = "CanUpdateBid2";
            await _auctionService.UpdateBid(before);
            var after = await _auctionService.GetBid(before.Id);

            Assert.AreEqual("CanUpdateBid2", after.CustomerId);
        }

        [TestMethod()]
        public async Task CanUpdateHighestBid()
        {
            Product p = new Product();
            p.Name = "CanUpdateHighestBid";
            p.HighestBid = 10;
            p.HighestBidder = "a";
            await _productRepository.InsertAsync(p);

            await _auctionService.UpdateHighestBid(p, 15, "b");
            var updated = _productRepository.Table.Where(x => x.Name == "CanUpdateHighestBid").First();

            Assert.AreEqual(15, updated.HighestBid);
            Assert.AreEqual("b", updated.HighestBidder);
        }

        [TestMethod()]
        public async Task CanGetAuctionsToEnd()
        {
            Product p = new Product();
            p.Name = "CanGetAuctionsToEnd";
            p.AvailableEndDateTimeUtc = DateTime.Now.AddDays(-5);
            p.ProductType = ProductType.Auction;
            await _productRepository.InsertAsync(p);

            var found = await _auctionService.GetAuctionsToEnd();

            Assert.AreEqual(1, found.Count);
        }

        [TestMethod()]
        public async Task CanUpdateAuctionEnded()
        {
            Product p = new Product();
            p.Name = "CanUpdateAuctionEnded";
            p.AvailableEndDateTimeUtc = DateTime.Now.AddDays(5);
            p.ProductType = ProductType.Auction;
            _productRepository.Insert(p);

            var before = _productRepository.Table.Where(x => x.Name == "CanUpdateAuctionEnded").First();
            await _auctionService.UpdateAuctionEnded(before, true, true);
            var after = _productRepository.Table.Where(x => x.Name == "CanUpdateAuctionEnded").First();

            Assert.AreEqual(true, after.AuctionEnded);
            Assert.AreEqual(DateTime.UtcNow.Date, after.AvailableEndDateTimeUtc.Value.Date);
        }

        [TestMethod()]
        public async Task CanCancelBidByOrder()
        {
            Product cancelProductBid = new Product();
            cancelProductBid.Name = "CanCancelBidByOrder";
            cancelProductBid.AvailableEndDateTimeUtc = DateTime.Now.AddDays(5);
            cancelProductBid.ProductType = ProductType.Auction;
            cancelProductBid.HighestBid = 10;
            cancelProductBid.HighestBidder = "H";
            _productRepository.Insert(cancelProductBid);

            var productService = new Mock<IProductService>();
            productService.Setup(x => x.GetProductById(cancelProductBid.Id, false)).ReturnsAsync(cancelProductBid);
            var _cancelproductService = productService.Object;
            var _cancelauctionService = new AuctionService(_bidRepository, _cancelproductService, _productRepository, _cacheManager, _eventPublisher);

            Bid bid = new Bid();
            bid.Amount = 1;
            bid.OrderId = "o";
            bid.ProductId = cancelProductBid.Id;
            _bidRepository.Insert(bid);
            await _cancelauctionService.CancelBidByOrder("o");

            var found = await _cancelauctionService.GetBidsByProductId("CanCancelBidByOrder");
            var product = _productRepository.Table.Where(x => x.Name == "CanCancelBidByOrder").FirstOrDefault();

            Assert.AreEqual(0, found.Count);
            Assert.AreEqual(0, product.HighestBid);
            Assert.AreEqual("", product.HighestBidder);
            Assert.AreEqual(false, product.AuctionEnded);
        }
    }
}
