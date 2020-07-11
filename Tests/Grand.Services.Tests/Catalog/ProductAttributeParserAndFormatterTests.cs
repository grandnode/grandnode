using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Core.Tests.Caching;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Tax;
using Grand.Services.Tests;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog.Tests
{
    [TestClass()]
    public class ProductAttributeParserTests {
        private IRepository<ProductAttribute> _productAttributeRepo;
        private IProductAttributeService _productAttributeService;
        private IProductAttributeParser _productAttributeParser;
        private IMediator _eventPublisher;

        private IWorkContext _workContext;
        private ICurrencyService _currencyService;
        private ILocalizationService _localizationService;
        private ITaxService _taxService;
        private IPriceFormatter _priceFormatter;
        private IPriceCalculationService _priceCalculationService;
        private IDownloadService _downloadService;
        private IWebHelper _webHelper;
        private ShoppingCartSettings _shoppingCartSettings;
        private IProductAttributeFormatter _productAttributeFormatter;
        private IProductService _productService;
        private ProductAttribute pa1, pa2, pa3;
        private ProductAttributeMapping pam1_1, pam2_1, pam3_1;
        private ProductAttributeValue pav1_1, pav1_2, pav2_1, pav2_2;

        private IRepository<Product> _productRepo;
        private Product _product;

        [TestInitialize()]
        public void TestInitialize() {

            _productRepo = new MongoDBRepositoryTest<Product>();
            //dropdown list: adding 2 options "atributes" that can be selected via DropDownList
            pa1 = new ProductAttribute {
                Id = "1",
                Name = "Color"
            };
            pam1_1 = new ProductAttributeMapping {
                Id = "11",
                ProductId = "1",
                TextPrompt = "Select color:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1,
                ProductAttributeId = pa1.Id
            };
            pav1_1 = new ProductAttributeValue {
                Id = "11",
                Name = "Green",
                DisplayOrder = 1,
                ProductAttributeMappingId = pam1_1.Id
            };
            pav1_2 = new ProductAttributeValue {
                Id = "12",
                Name = "Red",
                DisplayOrder = 2,
                ProductAttributeMappingId = pam1_1.Id
            };
            //adding colors (as product atributes)
            pam1_1.ProductAttributeValues.Add(pav1_1);
            pam1_1.ProductAttributeValues.Add(pav1_2);

            //checkboxes - adding 2 options that can be ticked via CheckBox'es
            pa2 = new ProductAttribute {
                Id = "2",
                Name = "Some custom option",
            };
            pam2_1 = new ProductAttributeMapping {
                Id = "21",
                ProductId = "1",
                TextPrompt = "Select at least one option:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.Checkboxes,
                DisplayOrder = 2,
                ProductAttributeId = pa2.Id
            };
            pav2_1 = new ProductAttributeValue {
                Id = "21",
                Name = "Option 1",
                DisplayOrder = 1,
                ProductAttributeMappingId = pam2_1.Id
            };
            pav2_2 = new ProductAttributeValue {
                Id = "22",
                Name = "Option 2",
                DisplayOrder = 2,
                ProductAttributeMappingId = pam2_1.Id
            };
            pam2_1.ProductAttributeValues.Add(pav2_1);
            pam2_1.ProductAttributeValues.Add(pav2_2);

            //adds custom text (user can add its own text)
            pa3 = new ProductAttribute {
                Id = "3",
                Name = "Custom text",
            };
            pam3_1 = new ProductAttributeMapping {
                Id = "31",
                ProductId = "1",
                TextPrompt = "Enter custom text:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.TextBox,
                DisplayOrder = 1,
                ProductAttributeId = pa3.Id
            };

            _product = new Product();
            _product.ProductAttributeMappings.Add(pam1_1);
            _product.ProductAttributeMappings.Add(pam2_1);
            _product.ProductAttributeMappings.Add(pam3_1);
            _productRepo.Insert(_product); //26 april

            var tempEventPublisher = new Mock<IMediator>();
            {
                //tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }

            var cacheManager = new TestMemoryCacheManager(new Mock<IMemoryCache>().Object, _eventPublisher);
            _productAttributeRepo = new Mock<IRepository<ProductAttribute>>().Object;

            _productAttributeService = new ProductAttributeService(cacheManager,
                _productAttributeRepo,
                _productRepo,
                _eventPublisher);
            _productAttributeParser = new ProductAttributeParser();
            _priceCalculationService = new Mock<IPriceCalculationService>().Object;

            var tempWorkContext = new Mock<IWorkContext>();
            {
                var workingLanguage = new Language();
                tempWorkContext.Setup(x => x.WorkingLanguage).Returns(workingLanguage);
                _workContext = tempWorkContext.Object;
            }
            _currencyService = new Mock<ICurrencyService>().Object;
            var tempLocalizationService = new Mock<ILocalizationService>();
            {
                tempLocalizationService.Setup(x => x.GetResource("GiftCardAttribute.For.Virtual")).Returns("For: {0} <{1}>");
                tempLocalizationService.Setup(x => x.GetResource("GiftCardAttribute.From.Virtual")).Returns("From: {0} <{1}>");
                tempLocalizationService.Setup(x => x.GetResource("GiftCardAttribute.For.Physical")).Returns("For: {0}");
                tempLocalizationService.Setup(x => x.GetResource("GiftCardAttribute.From.Physical")).Returns("From: {0}");
                _localizationService = tempLocalizationService.Object;
            }

            _taxService = new Mock<ITaxService>().Object;
            _priceFormatter = new Mock<IPriceFormatter>().Object;
            _downloadService = new Mock<IDownloadService>().Object;
            _webHelper = new Mock<IWebHelper>().Object;
            _shoppingCartSettings = new Mock<ShoppingCartSettings>().Object;
            _productService = new Mock<IProductService>().Object;

            _productAttributeFormatter = new ProductAttributeFormatter(_workContext,
                _productAttributeService,
                _productAttributeParser,
                _currencyService,
                _localizationService,
                _taxService,
                _priceFormatter,
                _downloadService,
                _webHelper,
                _priceCalculationService,
                _productService,
                _shoppingCartSettings);
        }

        [TestMethod()]
        public void Can_add_and_parse_productAttributes() {
            string attributes = "";
            //color: green
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam1_1, pav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam2_1, pav2_1.Id.ToString());
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam2_1, pav2_2.Id.ToString());
            //custom text
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam3_1, "Some custom text goes here");

            var parsed_attributeValues = _productAttributeParser.ParseProductAttributeValues(_product, attributes);//!
            Assert.IsTrue(parsed_attributeValues.Contains(pav1_1));
            Assert.IsFalse(parsed_attributeValues.Contains(pav1_2));
            Assert.IsTrue(parsed_attributeValues.Contains(pav2_1));
            Assert.IsTrue(parsed_attributeValues.Contains(pav2_2));
            Assert.IsTrue(parsed_attributeValues.Contains(pav2_2));

            var parsedValues = _productAttributeParser.ParseValues(attributes, pam3_1.Id);
            Assert.AreEqual(1, parsedValues.Count);
            Assert.IsTrue(parsedValues.Contains("Some custom text goes here"));
            Assert.IsFalse(parsedValues.Contains("Some other custom text"));
        }

        [TestMethod()]
        public void Can_add_and_remove_productAttributes() {
            string attributes = "";
            //color: green
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam1_1, pav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam2_1, pav2_1.Id.ToString());
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam2_1, pav2_2.Id.ToString());
            //custom text
            attributes = _productAttributeParser.AddProductAttribute(attributes, pam3_1, "Some custom text goes here");
            //delete some of them
            attributes = _productAttributeParser.RemoveProductAttribute(attributes, pam2_1);
            attributes = _productAttributeParser.RemoveProductAttribute(attributes, pam3_1);

            var parsed_attributeValues = _productAttributeParser.ParseProductAttributeValues(_product, attributes);//!
            Assert.IsTrue(parsed_attributeValues.Contains(pav1_1));
            Assert.IsFalse(parsed_attributeValues.Contains(pav1_2));
            Assert.IsFalse(parsed_attributeValues.Contains(pav2_1));
            Assert.IsFalse(parsed_attributeValues.Contains(pav2_2));
            Assert.IsFalse(parsed_attributeValues.Contains(pav2_2));

            var parsedValues = _productAttributeParser.ParseValues(attributes, pam3_1.Id);
            Assert.AreEqual(0, parsedValues.Count);
        }

        [TestMethod()]
        public void Can_add_and_parse_giftCardAttributes() {
            string attributes = "";
            attributes = _productAttributeParser.AddGiftCardAttribute(attributes,
                "recipientName 1", "recipientEmail@gmail.com",
                "senderName 1", "senderEmail@gmail.com", "custom message");

            string recipientName, recipientEmail, senderName, senderEmail, giftCardMessage;
            _productAttributeParser.GetGiftCardAttribute(attributes,
                out recipientName,
                out recipientEmail,
                out senderName,
                out senderEmail,
                out giftCardMessage);
            Assert.AreEqual("recipientName 1", recipientName);
            Assert.AreEqual("recipientEmail@gmail.com", recipientEmail);
            Assert.AreEqual("senderName 1", senderName);
            Assert.AreEqual("senderEmail@gmail.com", senderEmail);
            Assert.AreEqual("custom message", giftCardMessage);
        }

        [TestMethod()]
        public async Task Can_render_virtual_gift_cart() {
            string attributes = _productAttributeParser.AddGiftCardAttribute("",
                "recipientName 1", "recipientEmail@gmail.com",
                "senderName 1", "senderEmail@gmail.com", "custom message");

            var product = new Product {
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
            };
            var customer = new Customer();
            string formattedAttributes = await _productAttributeFormatter.FormatAttributes(product,
                attributes, customer, "<br />", false, false, true, true);
            Assert.AreEqual(
                "From: senderName 1 <senderEmail@gmail.com><br />For: recipientName 1 <recipientEmail@gmail.com>",
                formattedAttributes);
        }

        [TestMethod()]
        public async Task Can_render_physical_gift_cart() {
            string attributes = _productAttributeParser.AddGiftCardAttribute("",
                "recipientName 1", "recipientEmail@gmail.com",
                "senderName 1", "senderEmail@gmail.com", "custom message");

            var product = new Product {
                IsGiftCard = true,
                GiftCardType = GiftCardType.Physical,
            };
            var customer = new Customer();
            string formattedAttributes = await _productAttributeFormatter.FormatAttributes(product,
                attributes, customer, "<br />", false, false, true, true);
            Assert.AreEqual("From: senderName 1<br />For: recipientName 1", formattedAttributes);
        }
    }
}