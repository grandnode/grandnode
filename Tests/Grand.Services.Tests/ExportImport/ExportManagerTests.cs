using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Discounts;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Grand.Services.ExportImport.Tests
{
    [TestClass()]
    public class ExportManagerTests {
        private ICategoryService _categoryService;
        private IManufacturerService _manufacturerService;
        private IProductAttributeService _productAttributeService;
        private IPictureService _pictureService;
        private INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private IExportManager _exportManager;
        private IStoreService _storeService;
        private IProductService _productService;
        private IDiscountService _discountService;
        private IServiceProvider _serviceProvider;

        [TestInitialize()]
        public void TestInitialize() {
            _storeService = new Mock<IStoreService>().Object;
            _categoryService = new Mock<ICategoryService>().Object;
            _manufacturerService = new Mock<IManufacturerService>().Object;
            _productAttributeService = new Mock<IProductAttributeService>().Object;
            _pictureService = new Mock<IPictureService>().Object;
            _productService = new Mock<IProductService>().Object;
            _newsLetterSubscriptionService = new Mock<INewsLetterSubscriptionService>().Object;
            _discountService = new Mock<IDiscountService>().Object;
            _serviceProvider = new Mock<IServiceProvider>().Object;
            _exportManager = new ExportManager(_categoryService,
                _manufacturerService, _productAttributeService,
                _pictureService, _newsLetterSubscriptionService,
                _storeService, _productService, _discountService, _serviceProvider);
        }

        [TestMethod()]
        public void Can_export_orders_xlsx() {
            var orders = new List<Order>{
                new Order
                {
                OrderGuid = Guid.NewGuid(),
                OrderStatus = OrderStatus.Complete,
                ShippingStatus = ShippingStatus.Shipped,
                PaymentStatus = PaymentStatus.Paid,
                PaymentMethodSystemName = "PaymentMethodSystemName1",
                CustomerCurrencyCode = "RUR",
                CurrencyRate = 1.1M,
                CustomerTaxDisplayType = TaxDisplayType.ExcludingTax,
                VatNumber = "123456789",
                OrderSubtotalInclTax = 2.1M,
                OrderSubtotalExclTax = 3.1M,
                OrderSubTotalDiscountInclTax = 4.1M,
                OrderSubTotalDiscountExclTax = 5.1M,
                OrderShippingInclTax = 6.1M,
                OrderShippingExclTax = 7.1M,
                PaymentMethodAdditionalFeeInclTax = 8.1M,
                PaymentMethodAdditionalFeeExclTax = 9.1M,
                TaxRates = "1,3,5,7",
                OrderTax = 10.1M,
                OrderDiscount = 11.1M,
                OrderTotal = 12.1M,
                RefundedAmount  = 13.1M,
                CheckoutAttributeDescription = "CheckoutAttributeDescription1",
                CheckoutAttributesXml = "CheckoutAttributesXml1",
                CustomerLanguageId = "14",
                AffiliateId= "15",
                CustomerIp="CustomerIp1",
                UrlReferrer = "",
                AllowStoringCreditCardNumber= true,
                CardType= "Visa",
                CardName = "John Smith",
                CardNumber = "4111111111111111",
                MaskedCreditCardNumber= "************1111",
                CardCvv2= "123",
                CardExpirationMonth= "12",
                CardExpirationYear = "2010",
                AuthorizationTransactionId = "AuthorizationTransactionId1",
                AuthorizationTransactionCode="AuthorizationTransactionCode1",
                AuthorizationTransactionResult="AuthorizationTransactionResult1",
                CaptureTransactionId= "CaptureTransactionId1",
                CaptureTransactionResult = "CaptureTransactionResult1",
                SubscriptionTransactionId = "SubscriptionTransactionId1",
                PaidDateUtc= new DateTime(2010, 01, 01),
                BillingAddress = GetTestBillingAddress(),
                ShippingAddress = GetTestShippingAddress(),
                ShippingMethod = "ShippingMethod1",
                ShippingRateComputationMethodSystemName="ShippingRateComputationMethodSystemName1",
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 04)
            }
            };
            string fileName = Path.GetTempFileName();
        }

        protected Address GetTestBillingAddress() {
            return new Address {
                FirstName = "FirstName 1",
                LastName = "LastName 1",
                Email = "Email 1",
                Company = "Company 1",
                City = "City 1",
                Address1 = "Address1a",
                Address2 = "Address1a",
                ZipPostalCode = "ZipPostalCode 1",
                PhoneNumber = "PhoneNumber 1",
                FaxNumber = "FaxNumber 1",
                CreatedOnUtc = new DateTime(2010, 01, 01),
            };
        }

        protected Address GetTestShippingAddress() {
            return new Address {
                FirstName = "FirstName 2",
                LastName = "LastName 2",
                Email = "Email 2",
                Company = "Company 2",
                City = "City 2",
                Address1 = "Address2a",
                Address2 = "Address2b",
                ZipPostalCode = "ZipPostalCode 2",
                PhoneNumber = "PhoneNumber 2",
                FaxNumber = "FaxNumber 2",
                CreatedOnUtc = new DateTime(2010, 01, 01),
            };
        }

        protected Country GetTestCountry() {
            return new Country {
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 1,
                SubjectToVat = true,
                Published = true,
                DisplayOrder = 1
            };
        }

        protected Customer GetTestCustomer() {
            return new Customer {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "some comment here",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01)
            };
        }
    }
}