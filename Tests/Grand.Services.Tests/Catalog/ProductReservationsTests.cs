using Grand.Core;
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
    public class ProductReservationsTests
    {
        private IProductReservationService _productReservationService;
        private IRepository<Product> _productRepository;
        private IRepository<ProductReservation> _productReservationRepository;
        private IRepository<CustomerReservationsHelper> _customerReservationsHelperRepository;
        private IMediator _eventPublisher;
        private IWorkContext _workContext;

        [TestInitialize()]
        public void TestInitialize()
        {
            _productReservationRepository = new MongoDBRepositoryTest<ProductReservation>();
            _productRepository = new MongoDBRepositoryTest<Product>();
            _customerReservationsHelperRepository = new MongoDBRepositoryTest<CustomerReservationsHelper>();
            _workContext = new Mock<IWorkContext>().Object;

            var eventPublisher = new Mock<IMediator>();
            //eventPublisher.Setup(x => x.PublishAsync(new object()));
            _eventPublisher = eventPublisher.Object;

            _productReservationService = new ProductReservationService(_productReservationRepository,
                _customerReservationsHelperRepository, _eventPublisher);

            _productRepository.Insert(new Product
            {
                ProductType = ProductType.Reservation,
                Name = "Temp"
            });
        }

        [TestMethod()]
        public async Task CanAddProductReservation()
        {
            var date = DateTime.UtcNow;
            var productId = _productRepository.Table.FirstOrDefault()?.Id;

            var toInsert = new ProductReservation
            {
                Date = date,
                Duration = "1h",
                OrderId = "",
                Parameter = "Parameter",
                ProductId = productId,
                Resource = "Resource"
            };

            await _productReservationService.InsertProductReservation(toInsert);
            var inserted = _productReservationRepository.Table.Where(x => x.Date == date).FirstOrDefault();

            Assert.AreNotEqual(null, inserted);
            Assert.AreNotEqual(null, inserted.Id);
            Assert.AreEqual(date.Date, inserted.Date.Date);
            Assert.AreEqual("1h", inserted.Duration);
            Assert.AreEqual("", inserted.OrderId);
            Assert.AreEqual("Parameter", inserted.Parameter);
            Assert.AreEqual(productId, inserted.ProductId);
            Assert.AreEqual("Resource", inserted.Resource);
        }

        [TestMethod()]
        public async Task CanGetProductReservationByProductId()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanGetProductReservationByProductId",
                ProductType = ProductType.Reservation
            });

            var toInsert = new ProductReservation
            {
                Resource = "CanGetProductReservationByProductId",
                ProductId = product.Id
            };

            await _productReservationService.InsertProductReservation(toInsert);

            var found = await _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("CanGetProductReservationByProductId", found.First().Resource);
        }

        [TestMethod()]
        public async Task CanGetProductReservationById()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanGetProductReservationById",
                ProductType = ProductType.Reservation
            });

            var toInsert1 = new ProductReservation
            {
                Resource = "CanGetProductReservationById",
                ProductId = product.Id
            };

            var toInsert2 = new ProductReservation
            {
                Resource = "CanGetProductReservationById",
                ProductId = product.Id
            };

            await _productReservationService.InsertProductReservation(toInsert1);
            await _productReservationService.InsertProductReservation(toInsert2);

            var byProductId = await _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            var first = byProductId.First();

            var found = await _productReservationService.GetProductReservation(first.Id);

            Assert.AreEqual(first.Resource, found.Resource);
        }

        [TestMethod()]
        public async Task CanDeleteProductReservation()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanDeleteProductReservation",
                ProductType = ProductType.Reservation
            });

            var toInsert1 = new ProductReservation
            {
                Resource = "CanDeleteProductReservation1",
                ProductId = product.Id
            };

            var toInsert2 = new ProductReservation
            {
                Resource = "CanDeleteProductReservation2",
                ProductId = product.Id
            };

            await _productReservationService.InsertProductReservation(toInsert1);
            await _productReservationService.InsertProductReservation(toInsert2);

            var before = await _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            await _productReservationService.DeleteProductReservation(before.First());
            var after = await _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            Assert.AreEqual(before.Count - 1, after.Count);
            Assert.AreEqual("CanDeleteProductReservation2", after.First().Resource);
        }

        [TestMethod()]
        public async Task CanUpdateProductReservation()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanUpdateProductReservation",
                ProductType = ProductType.Reservation
            });

            var toInsert = new ProductReservation
            {
                Resource = "CanUpdateProductReservation",
                ProductId = product.Id
            };

            await _productReservationService.InsertProductReservation(toInsert);
            var before = (await _productReservationService.GetProductReservationsByProductId(product.Id, null, null)).First();
            before.Resource = "Updated";
            await _productReservationService.UpdateProductReservation(before);
            var after = await _productReservationService.GetProductReservationsByProductId(product.Id, null, null);

            Assert.AreEqual(1, after.Count);
            Assert.AreEqual(after.First().Resource, "Updated");
        }


        //[TestMethod()]
        //public async Task CanGetCustomerReservationsHelperById()
        //{
        //    var toInsert = new CustomerReservationsHelper
        //    {
        //        CustomerId = "CanGetCustomerReservationsHelperById",
        //        ReservationId = "CanGetCustomerReservationsHelperById",
        //        ShoppingCartItemId = "CanGetCustomerReservationsHelperById"
        //    };

        //    await _productReservationService.InsertCustomerReservationsHelper(toInsert);
        //    var inserted = _customerReservationsHelperRepository.Table.
        //        Where(x => x.CustomerId == "CanGetCustomerReservationsHelperById").FirstOrDefault();

        //    var found = await _productReservationService.GetCustomerReservationsHelperById(inserted.Id);

        //    Assert.AreEqual(inserted.ShoppingCartItemId, found.ShoppingCartItemId);
        //}

        //[TestMethod()]
        //public async Task CanGetCustomerReservationsHelperBySciId()
        //{
        //    var toInsert = new CustomerReservationsHelper
        //    {
        //        CustomerId = "CanGetCustomerReservationsHelperBySciIdC",
        //        ReservationId = "CanGetCustomerReservationsHelperBySciIdR",
        //        ShoppingCartItemId = "CanGetCustomerReservationsHelperBySciIdS"
        //    };

        //    await _productReservationService.InsertCustomerReservationsHelper(toInsert);

        //    var found = await _productReservationService.GetCustomerReservationsHelperBySciId("CanGetCustomerReservationsHelperBySciIdS");

        //    Assert.AreEqual(1, found.Count);
        //    Assert.AreNotEqual(0, found.Where(x => x.ShoppingCartItemId == "CanGetCustomerReservationsHelperBySciIdS").Count());
        //}

        [TestMethod()]
        public async Task CanCancelReservationsByOrderId()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanCancelReservationsByOrderId",
                ProductType = ProductType.Reservation
            });

            var toInsert1 = new ProductReservation
            {
                ProductId = product.Id,
                OrderId = "1"
            };

            var toInsert2 = new ProductReservation
            {
                ProductId = product.Id,
                OrderId = "1"
            };

            var toInsert3 = new ProductReservation
            {
                ProductId = product.Id,
                OrderId = "2"
            };

            await _productReservationService.InsertProductReservation(toInsert1);
            await _productReservationService.InsertProductReservation(toInsert2);
            await _productReservationService.InsertProductReservation(toInsert3);

            var before = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
            await _productReservationService.CancelReservationsByOrderId("1");
            var after = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);

            Assert.AreEqual(0, before.Count);
            Assert.AreEqual(2, after.Count);
        }
    }
}