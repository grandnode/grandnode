using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Media;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System.Linq;

namespace Grand.Services.Orders.Tests
{
    [TestClass()]
    public class CheckoutAttributeParserTests {
        private IRepository<CheckoutAttribute> _checkoutAttributeRepo;
        private IEventPublisher _eventPublisher;
        private IStoreMappingService _storeMappingService;
        private ICheckoutAttributeService _checkoutAttributeService;
        private ICheckoutAttributeParser _checkoutAttributeParser;
        private IWorkContext _workContext;
        private ICurrencyService _currencyService;
        private ITaxService _taxService;
        private IPriceFormatter _priceFormatter;
        private IDownloadService _downloadService;
        private IWebHelper _webHelper;
        private ICheckoutAttributeFormatter _checkoutAttributeFormatter;

        private CheckoutAttribute ca1, ca2, ca3;
        private CheckoutAttributeValue cav1_1, cav1_2, cav2_1, cav2_2;

        [TestInitialize()]
        public void TestInitialize() {
            //color choosing via DropDownList
            ca1 = new CheckoutAttribute {
                Id = "1",
                Name = "Color",
                TextPrompt = "Select color:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1,
            };
            cav1_1 = new CheckoutAttributeValue {
                Id = "11",
                Name = "Green",
                DisplayOrder = 1,
                //CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id,
            };
            cav1_2 = new CheckoutAttributeValue {
                Id = "12",
                Name = "Red",
                DisplayOrder = 2,
                //CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id,
            };
            ca1.CheckoutAttributeValues.Add(cav1_1);
            ca1.CheckoutAttributeValues.Add(cav1_2);

            //choosing via CheckBox'es
            ca2 = new CheckoutAttribute {
                Id = "2",
                Name = "Custom option",
                TextPrompt = "Select custom option:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.Checkboxes,
                DisplayOrder = 2,
                //CheckoutAttributeValues
            };

            cav2_1 = new CheckoutAttributeValue {
                Id = "21",
                Name = "Option 1",
                DisplayOrder = 1,
                //CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id,
            };
            cav2_2 = new CheckoutAttributeValue {
                Id = "22",
                Name = "Option 2",
                DisplayOrder = 2,
                //CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id,
            };
            ca2.CheckoutAttributeValues.Add(cav2_1);
            ca2.CheckoutAttributeValues.Add(cav2_2);

            //via MultiTextBoxes
            ca3 = new CheckoutAttribute {
                Id = "3",
                Name = "Custom text",
                TextPrompt = "Enter custom text:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.MultilineTextbox,
                DisplayOrder = 3,
            };



            var tempCheckoutAttributeRepo = new Mock<IRepository<CheckoutAttribute>>();
            {
                var IMongoCollection = new Mock<IMongoCollection<CheckoutAttribute>>().Object;
                IMongoCollection.InsertOne(ca1);
                IMongoCollection.InsertOne(ca2);
                IMongoCollection.InsertOne(ca3);
                tempCheckoutAttributeRepo.Setup(x => x.Table).Returns(IMongoCollection.AsQueryable());
                tempCheckoutAttributeRepo.Setup(x => x.GetById(ca1.Id)).Returns(ca1);
                tempCheckoutAttributeRepo.Setup(x => x.GetById(ca2.Id)).Returns(ca2);
                tempCheckoutAttributeRepo.Setup(x => x.GetById(ca3.Id)).Returns(ca3);
                _checkoutAttributeRepo = tempCheckoutAttributeRepo.Object;
            }

            var cacheManager = new GrandNullCache();

            _storeMappingService = new Mock<IStoreMappingService>().Object;



            var tempEventPublisher = new Mock<IEventPublisher>();
            {
                tempEventPublisher.Setup(x => x.Publish(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }


            _checkoutAttributeService = new CheckoutAttributeService(cacheManager, _checkoutAttributeRepo,
                _storeMappingService, _eventPublisher, null, null);

            _checkoutAttributeParser = new CheckoutAttributeParser(_checkoutAttributeService);



            var workingLanguage = new Language();
            var tempWorkContext = new Mock<IWorkContext>();
            {
                tempWorkContext.Setup(x => x.WorkingLanguage).Returns(workingLanguage);
                _workContext = tempWorkContext.Object;
            }
            _currencyService = new Mock<ICurrencyService>().Object;
            _taxService = new Mock<ITaxService>().Object;
            _priceFormatter = new Mock<IPriceFormatter>().Object;
            _downloadService = new Mock<IDownloadService>().Object;
            _webHelper = new Mock<IWebHelper>().Object;

            _checkoutAttributeFormatter = new CheckoutAttributeFormatter(_workContext,
                _checkoutAttributeService,
                _checkoutAttributeParser,
                _currencyService,
                _taxService,
                _priceFormatter,
                _downloadService,
                _webHelper);
        }

        [TestMethod()]
        public void Can_add_and_parse_checkoutAttributes() {
            string attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "absolutely any value");

            var parsed_attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(attributes);
            Assert.IsTrue(parsed_attributeValues.Contains(cav1_1));
            Assert.IsFalse(parsed_attributeValues.Contains(cav1_2)); //not inserted
            Assert.IsTrue(parsed_attributeValues.Contains(cav2_1));
            Assert.IsTrue(parsed_attributeValues.Contains(cav2_2));
            Assert.IsTrue(parsed_attributeValues.Contains(cav2_2));

            var parsedValues = _checkoutAttributeParser.ParseValues(attributes, ca3.Id);
            Assert.AreEqual(1, parsedValues.Count);
            Assert.IsTrue(parsedValues.Contains("absolutely any value"));
            Assert.IsFalse(parsedValues.Contains("it shouldn't be valid in any case"));
        }
    }
}