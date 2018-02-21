using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Grand.Services.Events;
using Grand.Core;
using Grand.Core.Domain.Customers;

namespace Grand.Services.Tests.Catalog
{
    [TestClass()]
    public class ProductReservationsTests
    {
        private IProductReservationService _productReservationService;
        private IRepository<Product> _productRepository;
        private IRepository<ProductReservation> _productReservationRepository;
        private IRepository<CustomerReservationsHelper> _customerReservationsHelperRepository;
        private IEventPublisher _eventPublisher;
        private IWorkContext _workContext;

        [TestInitialize()]
        public void TestInitialize()
        {
            _productReservationRepository = new MongoDBRepositoryTest<ProductReservation>();
            _productRepository = new MongoDBRepositoryTest<Product>();
            _customerReservationsHelperRepository = new MongoDBRepositoryTest<CustomerReservationsHelper>();
            _workContext = new Mock<IWorkContext>().Object;

            var eventPublisher = new Mock<IEventPublisher>();
            eventPublisher.Setup(x => x.Publish(new object()));
            _eventPublisher = eventPublisher.Object;

            _productReservationService = new ProductReservationService(_productReservationRepository,
                _customerReservationsHelperRepository, _eventPublisher, _workContext);

            _productRepository.Insert(new Product
            {
                ProductType = ProductType.Reservation,
                Name = "Temp"
            });
        }

        [TestMethod()]
        public void CanAddProductReservation()
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

            _productReservationService.InsertProductReservation(toInsert);
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
        public void CanGetProductReservationByProductId()
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

            _productReservationService.InsertProductReservation(toInsert);

            var found = _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("CanGetProductReservationByProductId", found.First().Resource);
        }

        [TestMethod()]
        public void CanGetProductReservationById()
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

            _productReservationService.InsertProductReservation(toInsert1);
            _productReservationService.InsertProductReservation(toInsert2);

            var byProductId = _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            var first = byProductId.First();

            var found = _productReservationService.GetProductReservation(first.Id);

            Assert.AreEqual(first.Resource, found.Resource);
        }

        [TestMethod()]
        public void CanDeleteProductReservation()
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

            _productReservationService.InsertProductReservation(toInsert1);
            _productReservationService.InsertProductReservation(toInsert2);

            var before = _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            _productReservationService.DeleteProductReservation(before.First());
            var after = _productReservationService.GetProductReservationsByProductId(product.Id, null, null);
            Assert.AreEqual(before.Count - 1, after.Count);
            Assert.AreEqual("CanDeleteProductReservation2", after.First().Resource);
        }

        [TestMethod()]
        public void CanUpdateProductReservation()
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

            _productReservationService.InsertProductReservation(toInsert);
            var before = _productReservationService.GetProductReservationsByProductId(product.Id, null, null).First();
            before.Resource = "Updated";
            _productReservationService.UpdateProductReservation(before);
            var after = _productReservationService.GetProductReservationsByProductId(product.Id, null, null);

            Assert.AreEqual(1, after.Count);
            Assert.AreEqual(after.First().Resource, "Updated");
        }

        [TestMethod()]
        public void CanAddCustomerReservationHelper()
        {
            var toInsert = new CustomerReservationsHelper
            {
                CustomerId = "CanAddCustomerReservationHelperC",
                ReservationId = "CanAddCustomerReservationHelperR",
                ShoppingCartItemId = "CanAddCustomerReservationHelperS"
            };

            _productReservationService.InsertCustomerReservationsHelper(toInsert);
            var inserted = _customerReservationsHelperRepository.Table.
                Where(x => x.CustomerId == "CanAddCustomerReservationHelperC").FirstOrDefault();

            Assert.AreNotEqual(null, inserted);
            Assert.AreNotEqual(null, inserted.Id);
            Assert.AreEqual("CanAddCustomerReservationHelperC", inserted.CustomerId);
            Assert.AreEqual("CanAddCustomerReservationHelperR", inserted.ReservationId);
            Assert.AreEqual("CanAddCustomerReservationHelperS", inserted.ShoppingCartItemId);
        }

        [TestMethod()]
        public void CanGetCustomerReservationsHelperById()
        {
            var toInsert = new CustomerReservationsHelper
            {
                CustomerId = "CanGetCustomerReservationsHelperById",
                ReservationId = "CanGetCustomerReservationsHelperById",
                ShoppingCartItemId = "CanGetCustomerReservationsHelperById"
            };

            _productReservationService.InsertCustomerReservationsHelper(toInsert);
            var inserted = _customerReservationsHelperRepository.Table.
                Where(x => x.CustomerId == "CanGetCustomerReservationsHelperById").FirstOrDefault();

            var found = _productReservationService.GetCustomerReservationsHelperById(inserted.Id);

            Assert.AreEqual(inserted.ShoppingCartItemId, found.ShoppingCartItemId);
        }

        [TestMethod()]
        public void CanGetCustomerReservationsHelperBySciId()
        {
            var toInsert = new CustomerReservationsHelper
            {
                CustomerId = "CanGetCustomerReservationsHelperBySciIdC",
                ReservationId = "CanGetCustomerReservationsHelperBySciIdR",
                ShoppingCartItemId = "CanGetCustomerReservationsHelperBySciIdS"
            };

            _productReservationService.InsertCustomerReservationsHelper(toInsert);

            var found = _productReservationService.GetCustomerReservationsHelperBySciId("CanGetCustomerReservationsHelperBySciIdS");

            Assert.AreEqual(1, found.Count);
            Assert.AreNotEqual(0, found.Where(x => x.ShoppingCartItemId == "CanGetCustomerReservationsHelperBySciIdS").Count());
        }

        [TestMethod()]
        public void CanDeleteCustomerReservationsHelper()
        {
            var product = _productRepository.Insert(new Product
            {
                Name = "CanDeleteCustomerReservationsHelper",
                ProductType = ProductType.Reservation
            });

            var toInsert1 = new CustomerReservationsHelper
            {
                ShoppingCartItemId = "CanDeleteCustomerReservationsHelper1"
            };

            var toInsert2 = new CustomerReservationsHelper
            {
                ShoppingCartItemId = "CanDeleteCustomerReservationsHelper2"
            };

            _productReservationService.InsertCustomerReservationsHelper(toInsert1);
            _productReservationService.InsertCustomerReservationsHelper(toInsert2);

            var before = _productReservationService.GetCustomerReservationsHelperBySciId("CanDeleteCustomerReservationsHelper1");
            _productReservationService.DeleteCustomerReservationsHelper(before.First());
            var after = _productReservationService.GetCustomerReservationsHelperBySciId("CanDeleteCustomerReservationsHelper1");
            Assert.AreEqual(0, after.Count);
        }

        [TestMethod()]
        public void CanCancelReservationsByOrderId()
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

            _productReservationService.InsertProductReservation(toInsert1);
            _productReservationService.InsertProductReservation(toInsert2);
            _productReservationService.InsertProductReservation(toInsert3);

            var before = _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
            _productReservationService.CancelReservationsByOrderId("1");
            var after = _productReservationService.GetProductReservationsByProductId(product.Id, true, null);

            Assert.AreEqual(0, before.Count);
            Assert.AreEqual(2, after.Count);
        }
    }
}