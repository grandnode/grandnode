using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tasks;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Catalog;
using MongoDB.Bson;
using MongoDB.Driver;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Configuration;

namespace Nop.Services.Installation
{
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<GrandNodeVersion> _versionRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<BlogComment> _blogcommentRepository;
        private readonly IRepository<BlogPost> _blogpostRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<Download> _downloadRepository;
        private readonly IRepository<GiftCard> _giftcardRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ReturnRequest> _returnrequestRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IRepository<Log> _logRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductAlsoPurchased> _productalsopurchasedRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumPost> _forumpostRepository;
        private readonly IRepository<ForumTopic> _forumtopicRepository;
        private readonly IRepository<ForumSubscription> _forumsubscriptionRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountUsageHistory> _discountusageRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<NewsLetterSubscription> _newslettersubscriptionRepository;
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<PrivateMessage> _privatemessageRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IRepository<TopicTemplate> _topicTemplateRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IRepository<QueuedEmail> _queuedemailRepository;
        private readonly IRepository<RewardPointsHistory> _rewardpointshistoryRepository;
        private readonly IRepository<SearchTerm> _searchtermRepository;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<ExternalAuthenticationRecord> _externalAuthenticationRepository;
        private readonly IRepository<ReturnRequestReason> _returnRequestReasonRepository;
        private readonly IRepository<ReturnRequestAction> _returnRequestActionRepository;
        private readonly IRepository<ContactUs> _contactUsRepository;
        private readonly IRepository<CustomerAction> _customerAction;
        private readonly IRepository<CustomerActionType> _customerActionType;
        private readonly IRepository<CustomerActionConditionType> _customerActionConditionType;
        private readonly ICustomerActionService _customerActionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;



        #endregion

        #region Ctor

        public CodeFirstInstallationService(
            IRepository<GrandNodeVersion> versionRepository,
            IRepository<Address> addressRepository,
            IRepository<Affiliate> affiliateRepository,
            IRepository<BlogComment> blogcommentRepository,
            IRepository<BlogPost> blogpostRepository,
            IRepository<Campaign> campaignRepository,
            IRepository<Download> downloadRepository,
            IRepository<GiftCard> giftcardRepository,
            IRepository<Order> orderRepository,
            IRepository<Store> storeRepository,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<Language> languageRepository,
            IRepository<LocaleStringResource> lsrRepository,
            IRepository<Log> logRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerRoleProduct> customerRoleProductRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAlsoPurchased> productalsopurchasedRepository,
            IRepository<Picture> pictureRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<ForumPost> forumpostRepository,
            IRepository<ForumTopic> forumtopicRepository,
            IRepository<ForumSubscription> forumsubscriptionRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Discount> discountRepository,
            IRepository<BlogPost> blogPostRepository,
            IRepository<Topic> topicRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<NewsLetterSubscription> newslettersubscriptionRepository,
            IRepository<Poll> pollRepository,
            IRepository<PrivateMessage> privatemessageRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<DeliveryDate> deliveryDateRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductTemplate> productTemplateRepository,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IRepository<TopicTemplate> topicTemplateRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IRepository<QueuedEmail> queuedemailRepository,
            IRepository<ReturnRequest> returnrequestRepository,
            IRepository<RewardPointsHistory> rewardpointshistoryRepository,
            IRepository<SearchTerm> searchtermRepository,
            IRepository<Setting> settingRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<PermissionRecord> permissionRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<ExternalAuthenticationRecord> externalAuthenticationRepository,
            IRepository<DiscountUsageHistory> discountusageRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<ContactUs> contactUsRepository,
            IRepository<CustomerAction> customerAction,
            IRepository<CustomerActionType> customerActionType,
            IRepository<CustomerActionConditionType> customerActionConditionType,
            IGenericAttributeService genericAttributeService,
            ICustomerActionService customerActionService,
            IWebHelper webHelper)
        {
            this._versionRepository = versionRepository;
            this._addressRepository = addressRepository;
            this._affiliateRepository = affiliateRepository;
            this._blogcommentRepository = blogcommentRepository;
            this._blogpostRepository = blogpostRepository;
            this._campaignRepository = campaignRepository;
            this._downloadRepository = downloadRepository;
            this._orderRepository = orderRepository;
            this._giftcardRepository = giftcardRepository;
            this._storeRepository = storeRepository;
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._taxCategoryRepository = taxCategoryRepository;
            this._languageRepository = languageRepository;
            this._lsrRepository = lsrRepository;
            this._logRepository = logRepository;
            this._currencyRepository = currencyRepository;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._customerRoleProductRepository = customerRoleProductRepository;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._checkoutAttributeRepository = checkoutAttributeRepository;
            this._productAttributeRepository = productAttributeRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._pictureRepository = pictureRepository;
            this._productalsopurchasedRepository = productalsopurchasedRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._forumGroupRepository = forumGroupRepository;
            this._forumRepository = forumRepository;
            this._forumpostRepository = forumpostRepository;
            this._forumtopicRepository = forumtopicRepository;
            this._forumsubscriptionRepository = forumsubscriptionRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._discountRepository = discountRepository;
            this._blogPostRepository = blogPostRepository;
            this._topicRepository = topicRepository;
            this._productReviewRepository = productReviewRepository;
            this._newsItemRepository = newsItemRepository;
            this._newslettersubscriptionRepository = newslettersubscriptionRepository;
            this._pollRepository = pollRepository;
            this._privatemessageRepository = privatemessageRepository;
            this._shippingMethodRepository = shippingMethodRepository;
            this._deliveryDateRepository = deliveryDateRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._activityLogRepository = activityLogRepository;
            this._productTagRepository = productTagRepository;
            this._productTemplateRepository = productTemplateRepository;
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._manufacturerTemplateRepository = manufacturerTemplateRepository;
            this._topicTemplateRepository = topicTemplateRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._genericAttributeService = genericAttributeService;
            this._queuedemailRepository = queuedemailRepository;
            this._returnrequestRepository = returnrequestRepository;
            this._rewardpointshistoryRepository = rewardpointshistoryRepository;
            this._searchtermRepository = searchtermRepository;
            this._settingRepository = settingRepository;
            this._shipmentRepository = shipmentRepository;
            this._warehouseRepository = warehouseRepository;
            this._webHelper = webHelper;
            this._permissionRepository = permissionRepository;
            this._vendorRepository = vendorRepository;
            this._externalAuthenticationRepository = externalAuthenticationRepository;
            this._discountusageRepository = discountusageRepository;
            this._returnRequestReasonRepository = returnRequestReasonRepository;
            this._contactUsRepository = contactUsRepository;
            this._returnRequestActionRepository = returnRequestActionRepository;
            this._customerAction = customerAction;
            this._customerActionType = customerActionType;
            this._customerActionConditionType = customerActionConditionType;
            this._customerActionService = customerActionService;
        }

        #endregion

        #region Utilities

        protected virtual void InstallVersion()
        {
            var version = new GrandNodeVersion
            {
                DataBaseVersion = GrandVersion.CurrentVersion
            };
            _versionRepository.Insert(version);
        }

        protected virtual void InstallStores()
        {
            //var storeUrl = "http://www.yourStore.com/";
            var storeUrl = _webHelper.GetStoreLocation(false);
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "Your store name",
                    Url = storeUrl,
                    SslEnabled = false,
                    Hosts = "yourstore.com,www.yourstore.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "Your company name",
                    CompanyAddress = "your company country, state, zip, street, etc",
                    CompanyPhoneNumber = "(123) 456-78901",
                    CompanyVat = null,
                },
            };

            _storeRepository.Insert(stores);
        }

        protected virtual void InstallMeasures()
        {
            var measureDimensions = new List<MeasureDimension>
            {
                new MeasureDimension
                {
                    Name = "inch(es)",
                    SystemKeyword = "inches",
                    Ratio = 1M,
                    DisplayOrder = 1,
                },
                new MeasureDimension
                {
                    Name = "feet",
                    SystemKeyword = "feet",
                    Ratio = 0.08333333M,
                    DisplayOrder = 2,
                },
                new MeasureDimension
                {
                    Name = "meter(s)",
                    SystemKeyword = "meters",
                    Ratio = 0.0254M,
                    DisplayOrder = 3,
                },
                new MeasureDimension
                {
                    Name = "millimetre(s)",
                    SystemKeyword = "millimetres",
                    Ratio = 25.4M,
                    DisplayOrder = 4,
                }
            };

            _measureDimensionRepository.Insert(measureDimensions);

            var measureWeights = new List<MeasureWeight>
            {
                new MeasureWeight
                {
                    Name = "ounce(s)",
                    SystemKeyword = "ounce",
                    Ratio = 16M,
                    DisplayOrder = 1,
                },
                new MeasureWeight
                {
                    Name = "lb(s)",
                    SystemKeyword = "lb",
                    Ratio = 1M,
                    DisplayOrder = 2,
                },
                new MeasureWeight
                {
                    Name = "kg(s)",
                    SystemKeyword = "kg",
                    Ratio = 0.45359237M,
                    DisplayOrder = 3,
                },
                new MeasureWeight
                {
                    Name = "gram(s)",
                    SystemKeyword = "grams",
                    Ratio = 453.59237M,
                    DisplayOrder = 4,
                }
            };

            _measureWeightRepository.Insert(measureWeights);
        }

        protected virtual void InstallTaxCategories()
        {
            var taxCategories = new List<TaxCategory>
                               {
                                   new TaxCategory
                                       {
                                           Name = "Books",
                                           DisplayOrder = 1,
                                       },
                                   new TaxCategory
                                       {
                                           Name = "Electronics & Software",
                                           DisplayOrder = 5,
                                       },
                                   new TaxCategory
                                       {
                                           Name = "Downloadable Products",
                                           DisplayOrder = 10,
                                       },
                                   new TaxCategory
                                       {
                                           Name = "Jewelry",
                                           DisplayOrder = 15,
                                       },
                                   new TaxCategory
                                       {
                                           Name = "Apparel",
                                           DisplayOrder = 20,
                                       },
                               };
            _taxCategoryRepository.Insert(taxCategories);

        }

        protected virtual void InstallLanguages()
        {
            var language = new Language
            {
                Name = "English",
                LanguageCulture = "en-US",
                UniqueSeoCode = "en",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 1
            };
            _languageRepository.Insert(language);
        }

        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/"), "*.nopres.xml", SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXmlInstall(language, localesXml);
            }

        }

        protected virtual void InstallCurrencies()
        {
            var currencies = new List<Currency>
            {
                new Currency
                {
                    Name = "US Dollar",
                    CurrencyCode = "USD",
                    Rate = 1,
                    DisplayLocale = "en-US",
                    CustomFormatting = "",
                    Published = true,
                    DisplayOrder = 1,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Australian Dollar",
                    CurrencyCode = "AUD",
                    Rate = 1.14M,
                    DisplayLocale = "en-AU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 2,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "British Pound",
                    CurrencyCode = "GBP",
                    Rate = 0.62M,
                    DisplayLocale = "en-GB",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 3,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Canadian Dollar",
                    CurrencyCode = "CAD",
                    Rate = 1.12M,
                    DisplayLocale = "en-CA",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 4,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Chinese Yuan Renminbi",
                    CurrencyCode = "CNY",
                    Rate = 6.11M,
                    DisplayLocale = "zh-CN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 5,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Euro",
                    CurrencyCode = "EUR",
                    Rate = 0.79M,
                    DisplayLocale = "",
                    //CustomFormatting = "�0.00",
                    CustomFormatting = string.Format("{0}0.00", "\u20ac"),
                    Published = true,
                    DisplayOrder = 6,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Hong Kong Dollar",
                    CurrencyCode = "HKD",
                    Rate = 7.75M,
                    DisplayLocale = "zh-HK",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 7,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Japanese Yen",
                    CurrencyCode = "JPY",
                    Rate = 109.27M,
                    DisplayLocale = "ja-JP",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 8,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Russian Rouble",
                    CurrencyCode = "RUB",
                    Rate = 43.51M,
                    DisplayLocale = "ru-RU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 9,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Swedish Krona",
                    CurrencyCode = "SEK",
                    Rate = 7.39M,
                    DisplayLocale = "sv-SE",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 10,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Romanian Leu",
                    CurrencyCode = "RON",
                    Rate = 3.52M,
                    DisplayLocale = "ro-RO",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 11,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
            };
            _currencyRepository.Insert(currencies);
        }

        protected virtual void InstallCountriesAndStates()
        {
            var cUsa = new Country
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 840,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = true,
            };

            var states = new List<StateProvince>();
            _countryRepository.Insert(cUsa);

            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "AA (Armed Forces Americas)",
                Abbreviation = "AA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "AE (Armed Forces Europe)",
                Abbreviation = "AE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Alabama",
                Abbreviation = "AL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Alaska",
                Abbreviation = "AK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "American Samoa",
                Abbreviation = "AS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "AP (Armed Forces Pacific)",
                Abbreviation = "AP",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Arizona",
                Abbreviation = "AZ",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Arkansas",
                Abbreviation = "AR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "California",
                Abbreviation = "CA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Colorado",
                Abbreviation = "CO",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Connecticut",
                Abbreviation = "CT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Delaware",
                Abbreviation = "DE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "District of Columbia",
                Abbreviation = "DC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Federated States of Micronesia",
                Abbreviation = "FM",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Florida",
                Abbreviation = "FL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Georgia",
                Abbreviation = "GA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Guam",
                Abbreviation = "GU",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Hawaii",
                Abbreviation = "HI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Idaho",
                Abbreviation = "ID",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Illinois",
                Abbreviation = "IL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Indiana",
                Abbreviation = "IN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Iowa",
                Abbreviation = "IA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Kansas",
                Abbreviation = "KS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Kentucky",
                Abbreviation = "KY",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Louisiana",
                Abbreviation = "LA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Maine",
                Abbreviation = "ME",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Marshall Islands",
                Abbreviation = "MH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Maryland",
                Abbreviation = "MD",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Massachusetts",
                Abbreviation = "MA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Michigan",
                Abbreviation = "MI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Minnesota",
                Abbreviation = "MN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Mississippi",
                Abbreviation = "MS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Missouri",
                Abbreviation = "MO",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Montana",
                Abbreviation = "MT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Nebraska",
                Abbreviation = "NE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Nevada",
                Abbreviation = "NV",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "New Hampshire",
                Abbreviation = "NH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "New Jersey",
                Abbreviation = "NJ",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "New Mexico",
                Abbreviation = "NM",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "New York",
                Abbreviation = "NY",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "North Carolina",
                Abbreviation = "NC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "North Dakota",
                Abbreviation = "ND",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Northern Mariana Islands",
                Abbreviation = "MP",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Ohio",
                Abbreviation = "OH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Oklahoma",
                Abbreviation = "OK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Oregon",
                Abbreviation = "OR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Palau",
                Abbreviation = "PW",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Pennsylvania",
                Abbreviation = "PA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Puerto Rico",
                Abbreviation = "PR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Rhode Island",
                Abbreviation = "RI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "South Carolina",
                Abbreviation = "SC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "South Dakota",
                Abbreviation = "SD",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Tennessee",
                Abbreviation = "TN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Texas",
                Abbreviation = "TX",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Utah",
                Abbreviation = "UT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Vermont",
                Abbreviation = "VT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Virgin Islands",
                Abbreviation = "VI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Virginia",
                Abbreviation = "VA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Washington",
                Abbreviation = "WA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "West Virginia",
                Abbreviation = "WV",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Wisconsin",
                Abbreviation = "WI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 1,
                Name = "Wyoming",
                Abbreviation = "WY",
                Published = true,
                DisplayOrder = 1,
            });
            var cCanada = new Country
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Canada",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "CA",
                ThreeLetterIsoCode = "CAN",
                NumericIsoCode = 124,
                SubjectToVat = false,
                DisplayOrder = 100,
                Published = true,
            };
            _countryRepository.Insert(cCanada);

            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Alberta",
                Abbreviation = "AB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "British Columbia",
                Abbreviation = "BC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Manitoba",
                Abbreviation = "MB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "New Brunswick",
                Abbreviation = "NB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Newfoundland and Labrador",
                Abbreviation = "NL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Northwest Territories",
                Abbreviation = "NT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Nova Scotia",
                Abbreviation = "NS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Nunavut",
                Abbreviation = "NU",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Ontario",
                Abbreviation = "ON",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Prince Edward Island",
                Abbreviation = "PE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Quebec",
                Abbreviation = "QC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Saskatchewan",
                Abbreviation = "SK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince
            {
                CountryId = 2,
                Name = "Yukon Territory",
                Abbreviation = "YT",
                Published = true,
                DisplayOrder = 1,
            });

            _stateProvinceRepository.Insert(states);

            var countries = new List<Country>
                                {
                                    new Country
                                    {
                                        Name = "Argentina",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AR",
                                        ThreeLetterIsoCode = "ARG",
                                        NumericIsoCode = 32,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Armenia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AM",
                                        ThreeLetterIsoCode = "ARM",
                                        NumericIsoCode = 51,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Aruba",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AW",
                                        ThreeLetterIsoCode = "ABW",
                                        NumericIsoCode = 533,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Australia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AU",
                                        ThreeLetterIsoCode = "AUS",
                                        NumericIsoCode = 36,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Austria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AT",
                                        ThreeLetterIsoCode = "AUT",
                                        NumericIsoCode = 40,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Azerbaijan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AZ",
                                        ThreeLetterIsoCode = "AZE",
                                        NumericIsoCode = 31,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bahamas",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BS",
                                        ThreeLetterIsoCode = "BHS",
                                        NumericIsoCode = 44,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bangladesh",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BD",
                                        ThreeLetterIsoCode = "BGD",
                                        NumericIsoCode = 50,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Belarus",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BY",
                                        ThreeLetterIsoCode = "BLR",
                                        NumericIsoCode = 112,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Belgium",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BE",
                                        ThreeLetterIsoCode = "BEL",
                                        NumericIsoCode = 56,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Belize",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BZ",
                                        ThreeLetterIsoCode = "BLZ",
                                        NumericIsoCode = 84,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bermuda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BM",
                                        ThreeLetterIsoCode = "BMU",
                                        NumericIsoCode = 60,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bolivia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BO",
                                        ThreeLetterIsoCode = "BOL",
                                        NumericIsoCode = 68,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bosnia and Herzegowina",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BA",
                                        ThreeLetterIsoCode = "BIH",
                                        NumericIsoCode = 70,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Brazil",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BR",
                                        ThreeLetterIsoCode = "BRA",
                                        NumericIsoCode = 76,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bulgaria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BG",
                                        ThreeLetterIsoCode = "BGR",
                                        NumericIsoCode = 100,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cayman Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KY",
                                        ThreeLetterIsoCode = "CYM",
                                        NumericIsoCode = 136,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Chile",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CL",
                                        ThreeLetterIsoCode = "CHL",
                                        NumericIsoCode = 152,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "China",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CN",
                                        ThreeLetterIsoCode = "CHN",
                                        NumericIsoCode = 156,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Colombia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CO",
                                        ThreeLetterIsoCode = "COL",
                                        NumericIsoCode = 170,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Costa Rica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CR",
                                        ThreeLetterIsoCode = "CRI",
                                        NumericIsoCode = 188,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Croatia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HR",
                                        ThreeLetterIsoCode = "HRV",
                                        NumericIsoCode = 191,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cuba",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CU",
                                        ThreeLetterIsoCode = "CUB",
                                        NumericIsoCode = 192,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cyprus",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CY",
                                        ThreeLetterIsoCode = "CYP",
                                        NumericIsoCode = 196,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Czech Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CZ",
                                        ThreeLetterIsoCode = "CZE",
                                        NumericIsoCode = 203,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Denmark",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DK",
                                        ThreeLetterIsoCode = "DNK",
                                        NumericIsoCode = 208,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Dominican Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DO",
                                        ThreeLetterIsoCode = "DOM",
                                        NumericIsoCode = 214,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Ecuador",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EC",
                                        ThreeLetterIsoCode = "ECU",
                                        NumericIsoCode = 218,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Egypt",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EG",
                                        ThreeLetterIsoCode = "EGY",
                                        NumericIsoCode = 818,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Finland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FI",
                                        ThreeLetterIsoCode = "FIN",
                                        NumericIsoCode = 246,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "France",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FR",
                                        ThreeLetterIsoCode = "FRA",
                                        NumericIsoCode = 250,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Georgia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GE",
                                        ThreeLetterIsoCode = "GEO",
                                        NumericIsoCode = 268,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Germany",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DE",
                                        ThreeLetterIsoCode = "DEU",
                                        NumericIsoCode = 276,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Gibraltar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GI",
                                        ThreeLetterIsoCode = "GIB",
                                        NumericIsoCode = 292,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Greece",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GR",
                                        ThreeLetterIsoCode = "GRC",
                                        NumericIsoCode = 300,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guatemala",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GT",
                                        ThreeLetterIsoCode = "GTM",
                                        NumericIsoCode = 320,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Hong Kong",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HK",
                                        ThreeLetterIsoCode = "HKG",
                                        NumericIsoCode = 344,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Hungary",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HU",
                                        ThreeLetterIsoCode = "HUN",
                                        NumericIsoCode = 348,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "India",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IN",
                                        ThreeLetterIsoCode = "IND",
                                        NumericIsoCode = 356,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Indonesia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ID",
                                        ThreeLetterIsoCode = "IDN",
                                        NumericIsoCode = 360,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Ireland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IE",
                                        ThreeLetterIsoCode = "IRL",
                                        NumericIsoCode = 372,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Israel",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IL",
                                        ThreeLetterIsoCode = "ISR",
                                        NumericIsoCode = 376,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Italy",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IT",
                                        ThreeLetterIsoCode = "ITA",
                                        NumericIsoCode = 380,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Jamaica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JM",
                                        ThreeLetterIsoCode = "JAM",
                                        NumericIsoCode = 388,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Japan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JP",
                                        ThreeLetterIsoCode = "JPN",
                                        NumericIsoCode = 392,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Jordan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JO",
                                        ThreeLetterIsoCode = "JOR",
                                        NumericIsoCode = 400,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Kazakhstan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KZ",
                                        ThreeLetterIsoCode = "KAZ",
                                        NumericIsoCode = 398,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Korea, Democratic People's Republic of",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KP",
                                        ThreeLetterIsoCode = "PRK",
                                        NumericIsoCode = 408,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Kuwait",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KW",
                                        ThreeLetterIsoCode = "KWT",
                                        NumericIsoCode = 414,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Malaysia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MY",
                                        ThreeLetterIsoCode = "MYS",
                                        NumericIsoCode = 458,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mexico",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MX",
                                        ThreeLetterIsoCode = "MEX",
                                        NumericIsoCode = 484,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Netherlands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NL",
                                        ThreeLetterIsoCode = "NLD",
                                        NumericIsoCode = 528,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "New Zealand",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NZ",
                                        ThreeLetterIsoCode = "NZL",
                                        NumericIsoCode = 554,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Norway",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NO",
                                        ThreeLetterIsoCode = "NOR",
                                        NumericIsoCode = 578,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Pakistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PK",
                                        ThreeLetterIsoCode = "PAK",
                                        NumericIsoCode = 586,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Paraguay",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PY",
                                        ThreeLetterIsoCode = "PRY",
                                        NumericIsoCode = 600,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Peru",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PE",
                                        ThreeLetterIsoCode = "PER",
                                        NumericIsoCode = 604,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Philippines",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PH",
                                        ThreeLetterIsoCode = "PHL",
                                        NumericIsoCode = 608,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Poland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PL",
                                        ThreeLetterIsoCode = "POL",
                                        NumericIsoCode = 616,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Portugal",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PT",
                                        ThreeLetterIsoCode = "PRT",
                                        NumericIsoCode = 620,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Puerto Rico",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PR",
                                        ThreeLetterIsoCode = "PRI",
                                        NumericIsoCode = 630,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Qatar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "QA",
                                        ThreeLetterIsoCode = "QAT",
                                        NumericIsoCode = 634,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Romania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RO",
                                        ThreeLetterIsoCode = "ROM",
                                        NumericIsoCode = 642,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Russia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RU",
                                        ThreeLetterIsoCode = "RUS",
                                        NumericIsoCode = 643,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Saudi Arabia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SA",
                                        ThreeLetterIsoCode = "SAU",
                                        NumericIsoCode = 682,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Singapore",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SG",
                                        ThreeLetterIsoCode = "SGP",
                                        NumericIsoCode = 702,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Slovakia (Slovak Republic)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SK",
                                        ThreeLetterIsoCode = "SVK",
                                        NumericIsoCode = 703,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Slovenia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SI",
                                        ThreeLetterIsoCode = "SVN",
                                        NumericIsoCode = 705,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "South Africa",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ZA",
                                        ThreeLetterIsoCode = "ZAF",
                                        NumericIsoCode = 710,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Spain",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ES",
                                        ThreeLetterIsoCode = "ESP",
                                        NumericIsoCode = 724,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Sweden",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SE",
                                        ThreeLetterIsoCode = "SWE",
                                        NumericIsoCode = 752,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Switzerland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CH",
                                        ThreeLetterIsoCode = "CHE",
                                        NumericIsoCode = 756,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Taiwan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TW",
                                        ThreeLetterIsoCode = "TWN",
                                        NumericIsoCode = 158,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Thailand",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TH",
                                        ThreeLetterIsoCode = "THA",
                                        NumericIsoCode = 764,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Turkey",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TR",
                                        ThreeLetterIsoCode = "TUR",
                                        NumericIsoCode = 792,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Ukraine",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UA",
                                        ThreeLetterIsoCode = "UKR",
                                        NumericIsoCode = 804,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "United Arab Emirates",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AE",
                                        ThreeLetterIsoCode = "ARE",
                                        NumericIsoCode = 784,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "United Kingdom",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GB",
                                        ThreeLetterIsoCode = "GBR",
                                        NumericIsoCode = 826,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "United States minor outlying islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UM",
                                        ThreeLetterIsoCode = "UMI",
                                        NumericIsoCode = 581,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Uruguay",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UY",
                                        ThreeLetterIsoCode = "URY",
                                        NumericIsoCode = 858,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Uzbekistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UZ",
                                        ThreeLetterIsoCode = "UZB",
                                        NumericIsoCode = 860,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Venezuela",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VE",
                                        ThreeLetterIsoCode = "VEN",
                                        NumericIsoCode = 862,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Serbia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RS",
                                        ThreeLetterIsoCode = "SRB",
                                        NumericIsoCode = 688,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Afghanistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AF",
                                        ThreeLetterIsoCode = "AFG",
                                        NumericIsoCode = 4,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Albania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AL",
                                        ThreeLetterIsoCode = "ALB",
                                        NumericIsoCode = 8,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Algeria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DZ",
                                        ThreeLetterIsoCode = "DZA",
                                        NumericIsoCode = 12,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "American Samoa",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AS",
                                        ThreeLetterIsoCode = "ASM",
                                        NumericIsoCode = 16,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Andorra",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AD",
                                        ThreeLetterIsoCode = "AND",
                                        NumericIsoCode = 20,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Angola",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AO",
                                        ThreeLetterIsoCode = "AGO",
                                        NumericIsoCode = 24,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Anguilla",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AI",
                                        ThreeLetterIsoCode = "AIA",
                                        NumericIsoCode = 660,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Antarctica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AQ",
                                        ThreeLetterIsoCode = "ATA",
                                        NumericIsoCode = 10,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Antigua and Barbuda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AG",
                                        ThreeLetterIsoCode = "ATG",
                                        NumericIsoCode = 28,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bahrain",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BH",
                                        ThreeLetterIsoCode = "BHR",
                                        NumericIsoCode = 48,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Barbados",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BB",
                                        ThreeLetterIsoCode = "BRB",
                                        NumericIsoCode = 52,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Benin",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BJ",
                                        ThreeLetterIsoCode = "BEN",
                                        NumericIsoCode = 204,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bhutan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BT",
                                        ThreeLetterIsoCode = "BTN",
                                        NumericIsoCode = 64,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Botswana",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BW",
                                        ThreeLetterIsoCode = "BWA",
                                        NumericIsoCode = 72,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Bouvet Island",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BV",
                                        ThreeLetterIsoCode = "BVT",
                                        NumericIsoCode = 74,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "British Indian Ocean Territory",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IO",
                                        ThreeLetterIsoCode = "IOT",
                                        NumericIsoCode = 86,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Brunei Darussalam",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BN",
                                        ThreeLetterIsoCode = "BRN",
                                        NumericIsoCode = 96,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Burkina Faso",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BF",
                                        ThreeLetterIsoCode = "BFA",
                                        NumericIsoCode = 854,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Burundi",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BI",
                                        ThreeLetterIsoCode = "BDI",
                                        NumericIsoCode = 108,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cambodia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KH",
                                        ThreeLetterIsoCode = "KHM",
                                        NumericIsoCode = 116,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cameroon",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CM",
                                        ThreeLetterIsoCode = "CMR",
                                        NumericIsoCode = 120,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cape Verde",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CV",
                                        ThreeLetterIsoCode = "CPV",
                                        NumericIsoCode = 132,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Central African Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CF",
                                        ThreeLetterIsoCode = "CAF",
                                        NumericIsoCode = 140,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Chad",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TD",
                                        ThreeLetterIsoCode = "TCD",
                                        NumericIsoCode = 148,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Christmas Island",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CX",
                                        ThreeLetterIsoCode = "CXR",
                                        NumericIsoCode = 162,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cocos (Keeling) Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CC",
                                        ThreeLetterIsoCode = "CCK",
                                        NumericIsoCode = 166,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Comoros",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KM",
                                        ThreeLetterIsoCode = "COM",
                                        NumericIsoCode = 174,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Congo",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CG",
                                        ThreeLetterIsoCode = "COG",
                                        NumericIsoCode = 178,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cook Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CK",
                                        ThreeLetterIsoCode = "COK",
                                        NumericIsoCode = 184,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cote D'Ivoire",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CI",
                                        ThreeLetterIsoCode = "CIV",
                                        NumericIsoCode = 384,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Djibouti",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DJ",
                                        ThreeLetterIsoCode = "DJI",
                                        NumericIsoCode = 262,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Dominica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DM",
                                        ThreeLetterIsoCode = "DMA",
                                        NumericIsoCode = 212,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "El Salvador",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SV",
                                        ThreeLetterIsoCode = "SLV",
                                        NumericIsoCode = 222,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Equatorial Guinea",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GQ",
                                        ThreeLetterIsoCode = "GNQ",
                                        NumericIsoCode = 226,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Eritrea",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ER",
                                        ThreeLetterIsoCode = "ERI",
                                        NumericIsoCode = 232,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Estonia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EE",
                                        ThreeLetterIsoCode = "EST",
                                        NumericIsoCode = 233,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Ethiopia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ET",
                                        ThreeLetterIsoCode = "ETH",
                                        NumericIsoCode = 231,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Falkland Islands (Malvinas)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FK",
                                        ThreeLetterIsoCode = "FLK",
                                        NumericIsoCode = 238,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Faroe Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FO",
                                        ThreeLetterIsoCode = "FRO",
                                        NumericIsoCode = 234,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Fiji",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FJ",
                                        ThreeLetterIsoCode = "FJI",
                                        NumericIsoCode = 242,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "French Guiana",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GF",
                                        ThreeLetterIsoCode = "GUF",
                                        NumericIsoCode = 254,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "French Polynesia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PF",
                                        ThreeLetterIsoCode = "PYF",
                                        NumericIsoCode = 258,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "French Southern Territories",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TF",
                                        ThreeLetterIsoCode = "ATF",
                                        NumericIsoCode = 260,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Gabon",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GA",
                                        ThreeLetterIsoCode = "GAB",
                                        NumericIsoCode = 266,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Gambia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GM",
                                        ThreeLetterIsoCode = "GMB",
                                        NumericIsoCode = 270,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Ghana",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GH",
                                        ThreeLetterIsoCode = "GHA",
                                        NumericIsoCode = 288,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Greenland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GL",
                                        ThreeLetterIsoCode = "GRL",
                                        NumericIsoCode = 304,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Grenada",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GD",
                                        ThreeLetterIsoCode = "GRD",
                                        NumericIsoCode = 308,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guadeloupe",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GP",
                                        ThreeLetterIsoCode = "GLP",
                                        NumericIsoCode = 312,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guam",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GU",
                                        ThreeLetterIsoCode = "GUM",
                                        NumericIsoCode = 316,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guinea",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GN",
                                        ThreeLetterIsoCode = "GIN",
                                        NumericIsoCode = 324,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guinea-bissau",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GW",
                                        ThreeLetterIsoCode = "GNB",
                                        NumericIsoCode = 624,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Guyana",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GY",
                                        ThreeLetterIsoCode = "GUY",
                                        NumericIsoCode = 328,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Haiti",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HT",
                                        ThreeLetterIsoCode = "HTI",
                                        NumericIsoCode = 332,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Heard and Mc Donald Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HM",
                                        ThreeLetterIsoCode = "HMD",
                                        NumericIsoCode = 334,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Honduras",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HN",
                                        ThreeLetterIsoCode = "HND",
                                        NumericIsoCode = 340,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Iceland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IS",
                                        ThreeLetterIsoCode = "ISL",
                                        NumericIsoCode = 352,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Iran (Islamic Republic of)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IR",
                                        ThreeLetterIsoCode = "IRN",
                                        NumericIsoCode = 364,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Iraq",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IQ",
                                        ThreeLetterIsoCode = "IRQ",
                                        NumericIsoCode = 368,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Kenya",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KE",
                                        ThreeLetterIsoCode = "KEN",
                                        NumericIsoCode = 404,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Kiribati",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KI",
                                        ThreeLetterIsoCode = "KIR",
                                        NumericIsoCode = 296,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Korea",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KR",
                                        ThreeLetterIsoCode = "KOR",
                                        NumericIsoCode = 410,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Kyrgyzstan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KG",
                                        ThreeLetterIsoCode = "KGZ",
                                        NumericIsoCode = 417,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Lao People's Democratic Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LA",
                                        ThreeLetterIsoCode = "LAO",
                                        NumericIsoCode = 418,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Latvia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LV",
                                        ThreeLetterIsoCode = "LVA",
                                        NumericIsoCode = 428,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Lebanon",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LB",
                                        ThreeLetterIsoCode = "LBN",
                                        NumericIsoCode = 422,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Lesotho",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LS",
                                        ThreeLetterIsoCode = "LSO",
                                        NumericIsoCode = 426,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Liberia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LR",
                                        ThreeLetterIsoCode = "LBR",
                                        NumericIsoCode = 430,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Libyan Arab Jamahiriya",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LY",
                                        ThreeLetterIsoCode = "LBY",
                                        NumericIsoCode = 434,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Liechtenstein",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LI",
                                        ThreeLetterIsoCode = "LIE",
                                        NumericIsoCode = 438,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Lithuania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LT",
                                        ThreeLetterIsoCode = "LTU",
                                        NumericIsoCode = 440,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Luxembourg",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LU",
                                        ThreeLetterIsoCode = "LUX",
                                        NumericIsoCode = 442,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Macau",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MO",
                                        ThreeLetterIsoCode = "MAC",
                                        NumericIsoCode = 446,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Macedonia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MK",
                                        ThreeLetterIsoCode = "MKD",
                                        NumericIsoCode = 807,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Madagascar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MG",
                                        ThreeLetterIsoCode = "MDG",
                                        NumericIsoCode = 450,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Malawi",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MW",
                                        ThreeLetterIsoCode = "MWI",
                                        NumericIsoCode = 454,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Maldives",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MV",
                                        ThreeLetterIsoCode = "MDV",
                                        NumericIsoCode = 462,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mali",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ML",
                                        ThreeLetterIsoCode = "MLI",
                                        NumericIsoCode = 466,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Malta",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MT",
                                        ThreeLetterIsoCode = "MLT",
                                        NumericIsoCode = 470,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Marshall Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MH",
                                        ThreeLetterIsoCode = "MHL",
                                        NumericIsoCode = 584,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Martinique",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MQ",
                                        ThreeLetterIsoCode = "MTQ",
                                        NumericIsoCode = 474,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mauritania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MR",
                                        ThreeLetterIsoCode = "MRT",
                                        NumericIsoCode = 478,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mauritius",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MU",
                                        ThreeLetterIsoCode = "MUS",
                                        NumericIsoCode = 480,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mayotte",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "YT",
                                        ThreeLetterIsoCode = "MYT",
                                        NumericIsoCode = 175,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Micronesia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FM",
                                        ThreeLetterIsoCode = "FSM",
                                        NumericIsoCode = 583,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Moldova",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MD",
                                        ThreeLetterIsoCode = "MDA",
                                        NumericIsoCode = 498,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Monaco",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MC",
                                        ThreeLetterIsoCode = "MCO",
                                        NumericIsoCode = 492,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mongolia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MN",
                                        ThreeLetterIsoCode = "MNG",
                                        NumericIsoCode = 496,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Montenegro",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ME",
                                        ThreeLetterIsoCode = "MNE",
                                        NumericIsoCode = 499,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Montserrat",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MS",
                                        ThreeLetterIsoCode = "MSR",
                                        NumericIsoCode = 500,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Morocco",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MA",
                                        ThreeLetterIsoCode = "MAR",
                                        NumericIsoCode = 504,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Mozambique",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MZ",
                                        ThreeLetterIsoCode = "MOZ",
                                        NumericIsoCode = 508,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Myanmar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MM",
                                        ThreeLetterIsoCode = "MMR",
                                        NumericIsoCode = 104,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Namibia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NA",
                                        ThreeLetterIsoCode = "NAM",
                                        NumericIsoCode = 516,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Nauru",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NR",
                                        ThreeLetterIsoCode = "NRU",
                                        NumericIsoCode = 520,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Nepal",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NP",
                                        ThreeLetterIsoCode = "NPL",
                                        NumericIsoCode = 524,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Netherlands Antilles",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AN",
                                        ThreeLetterIsoCode = "ANT",
                                        NumericIsoCode = 530,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "New Caledonia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NC",
                                        ThreeLetterIsoCode = "NCL",
                                        NumericIsoCode = 540,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Nicaragua",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NI",
                                        ThreeLetterIsoCode = "NIC",
                                        NumericIsoCode = 558,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Niger",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NE",
                                        ThreeLetterIsoCode = "NER",
                                        NumericIsoCode = 562,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Nigeria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NG",
                                        ThreeLetterIsoCode = "NGA",
                                        NumericIsoCode = 566,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Niue",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NU",
                                        ThreeLetterIsoCode = "NIU",
                                        NumericIsoCode = 570,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Norfolk Island",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NF",
                                        ThreeLetterIsoCode = "NFK",
                                        NumericIsoCode = 574,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Northern Mariana Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MP",
                                        ThreeLetterIsoCode = "MNP",
                                        NumericIsoCode = 580,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Oman",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "OM",
                                        ThreeLetterIsoCode = "OMN",
                                        NumericIsoCode = 512,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Palau",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PW",
                                        ThreeLetterIsoCode = "PLW",
                                        NumericIsoCode = 585,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Panama",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PA",
                                        ThreeLetterIsoCode = "PAN",
                                        NumericIsoCode = 591,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Papua New Guinea",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PG",
                                        ThreeLetterIsoCode = "PNG",
                                        NumericIsoCode = 598,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Pitcairn",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PN",
                                        ThreeLetterIsoCode = "PCN",
                                        NumericIsoCode = 612,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Reunion",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RE",
                                        ThreeLetterIsoCode = "REU",
                                        NumericIsoCode = 638,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Rwanda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RW",
                                        ThreeLetterIsoCode = "RWA",
                                        NumericIsoCode = 646,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Saint Kitts and Nevis",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KN",
                                        ThreeLetterIsoCode = "KNA",
                                        NumericIsoCode = 659,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Saint Lucia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LC",
                                        ThreeLetterIsoCode = "LCA",
                                        NumericIsoCode = 662,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Saint Vincent and the Grenadines",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VC",
                                        ThreeLetterIsoCode = "VCT",
                                        NumericIsoCode = 670,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Samoa",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "WS",
                                        ThreeLetterIsoCode = "WSM",
                                        NumericIsoCode = 882,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "San Marino",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SM",
                                        ThreeLetterIsoCode = "SMR",
                                        NumericIsoCode = 674,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Sao Tome and Principe",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ST",
                                        ThreeLetterIsoCode = "STP",
                                        NumericIsoCode = 678,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Senegal",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SN",
                                        ThreeLetterIsoCode = "SEN",
                                        NumericIsoCode = 686,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Seychelles",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SC",
                                        ThreeLetterIsoCode = "SYC",
                                        NumericIsoCode = 690,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Sierra Leone",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SL",
                                        ThreeLetterIsoCode = "SLE",
                                        NumericIsoCode = 694,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Solomon Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SB",
                                        ThreeLetterIsoCode = "SLB",
                                        NumericIsoCode = 90,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Somalia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SO",
                                        ThreeLetterIsoCode = "SOM",
                                        NumericIsoCode = 706,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "South Georgia & South Sandwich Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GS",
                                        ThreeLetterIsoCode = "SGS",
                                        NumericIsoCode = 239,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Sri Lanka",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "LK",
                                        ThreeLetterIsoCode = "LKA",
                                        NumericIsoCode = 144,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "St. Helena",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SH",
                                        ThreeLetterIsoCode = "SHN",
                                        NumericIsoCode = 654,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "St. Pierre and Miquelon",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PM",
                                        ThreeLetterIsoCode = "SPM",
                                        NumericIsoCode = 666,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Sudan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SD",
                                        ThreeLetterIsoCode = "SDN",
                                        NumericIsoCode = 736,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Suriname",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SR",
                                        ThreeLetterIsoCode = "SUR",
                                        NumericIsoCode = 740,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Svalbard and Jan Mayen Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SJ",
                                        ThreeLetterIsoCode = "SJM",
                                        NumericIsoCode = 744,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Swaziland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SZ",
                                        ThreeLetterIsoCode = "SWZ",
                                        NumericIsoCode = 748,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Syrian Arab Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SY",
                                        ThreeLetterIsoCode = "SYR",
                                        NumericIsoCode = 760,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tajikistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TJ",
                                        ThreeLetterIsoCode = "TJK",
                                        NumericIsoCode = 762,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tanzania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TZ",
                                        ThreeLetterIsoCode = "TZA",
                                        NumericIsoCode = 834,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Togo",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TG",
                                        ThreeLetterIsoCode = "TGO",
                                        NumericIsoCode = 768,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tokelau",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TK",
                                        ThreeLetterIsoCode = "TKL",
                                        NumericIsoCode = 772,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tonga",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TO",
                                        ThreeLetterIsoCode = "TON",
                                        NumericIsoCode = 776,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Trinidad and Tobago",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TT",
                                        ThreeLetterIsoCode = "TTO",
                                        NumericIsoCode = 780,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tunisia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TN",
                                        ThreeLetterIsoCode = "TUN",
                                        NumericIsoCode = 788,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Turkmenistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TM",
                                        ThreeLetterIsoCode = "TKM",
                                        NumericIsoCode = 795,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Turks and Caicos Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TC",
                                        ThreeLetterIsoCode = "TCA",
                                        NumericIsoCode = 796,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Tuvalu",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TV",
                                        ThreeLetterIsoCode = "TUV",
                                        NumericIsoCode = 798,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Uganda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UG",
                                        ThreeLetterIsoCode = "UGA",
                                        NumericIsoCode = 800,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Vanuatu",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VU",
                                        ThreeLetterIsoCode = "VUT",
                                        NumericIsoCode = 548,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Vatican City State (Holy See)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VA",
                                        ThreeLetterIsoCode = "VAT",
                                        NumericIsoCode = 336,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Viet Nam",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VN",
                                        ThreeLetterIsoCode = "VNM",
                                        NumericIsoCode = 704,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Virgin Islands (British)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VG",
                                        ThreeLetterIsoCode = "VGB",
                                        NumericIsoCode = 92,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Virgin Islands (U.S.)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VI",
                                        ThreeLetterIsoCode = "VIR",
                                        NumericIsoCode = 850,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Wallis and Futuna Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "WF",
                                        ThreeLetterIsoCode = "WLF",
                                        NumericIsoCode = 876,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Western Sahara",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EH",
                                        ThreeLetterIsoCode = "ESH",
                                        NumericIsoCode = 732,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Yemen",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "YE",
                                        ThreeLetterIsoCode = "YEM",
                                        NumericIsoCode = 887,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Zambia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ZM",
                                        ThreeLetterIsoCode = "ZMB",
                                        NumericIsoCode = 894,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Zimbabwe",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ZW",
                                        ThreeLetterIsoCode = "ZWE",
                                        NumericIsoCode = 716,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                };
            _countryRepository.Insert(countries);
        }

        protected virtual void InstallShippingMethods()
        {
            var shippingMethods = new List<ShippingMethod>
                                {
                                    new ShippingMethod
                                        {
                                            Name = "Ground",
                                            Description ="Compared to other shipping methods, ground shipping is carried out closer to the earth",
                                            DisplayOrder = 1
                                        },
                                    new ShippingMethod
                                        {
                                            Name = "Next Day Air",
                                            Description ="The one day air shipping",
                                            DisplayOrder = 3
                                        },
                                    new ShippingMethod
                                        {
                                            Name = "2nd Day Air",
                                            Description ="The two day air shipping",
                                            DisplayOrder = 3
                                        }
                                };
            _shippingMethodRepository.Insert(shippingMethods);
        }

        protected virtual void InstallDeliveryDates()
        {
            var deliveryDates = new List<DeliveryDate>
                                {
                                    new DeliveryDate
                                        {
                                            Name = "1-2 days",
                                            DisplayOrder = 1
                                        },
                                    new DeliveryDate
                                        {
                                            Name = "3-5 days",
                                            DisplayOrder = 5
                                        },
                                    new DeliveryDate
                                        {
                                            Name = "1 week",
                                            DisplayOrder = 10
                                        },
                                };
            _deliveryDateRepository.Insert(deliveryDates);
        }

        protected virtual void InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new CustomerRole
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Administrators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            _customerRoleRepository.Insert(crAdministrators);

            var crForumModerators = new CustomerRole
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Forum Moderators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.ForumModerators,
            };
            _customerRoleRepository.Insert(crForumModerators);

            var crRegistered = new CustomerRole
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Registered",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            _customerRoleRepository.Insert(crRegistered);

            var crGuests = new CustomerRole
            {
                Id = 4,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Guests",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };
            _customerRoleRepository.Insert(crGuests);

            var crVendors = new CustomerRole
            {
                Id = 5,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Vendors",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Vendors,
            };
            _customerRoleRepository.Insert(crVendors);


            //admin user
            var adminUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Password = defaultUserPassword,
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = "",
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            var defaultAdminUserAddress = new Address
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = "admin@yourStore.com",
                FaxNumber = "",
                Company = "GrandNode LTD",
                Address1 = "21 West 52nd Street",
                Address2 = "",
                City = "New York",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            adminUser.Addresses.Add(defaultAdminUserAddress);
            adminUser.BillingAddress = defaultAdminUserAddress;
            adminUser.ShippingAddress = defaultAdminUserAddress;
            adminUser.CustomerRoles.Add(crAdministrators);
            adminUser.CustomerRoles.Add(crForumModerators);
            adminUser.CustomerRoles.Add(crRegistered);
            _customerRepository.Insert(adminUser);
            //set default customer name
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, "John");
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, "Smith");


            //search engine (crawler) built-in user
            var searchEngineUser = new Customer
            {
                Email = "builtin@search_engine_record.com",
                CustomerGuid = Guid.NewGuid(),
                PasswordFormat = PasswordFormat.Clear,
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.SearchEngine,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            searchEngineUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(searchEngineUser);


            //built-in user for background tasks
            var backgroundTaskUser = new Customer
            {
                Email = "builtin@background-task-record.com",
                CustomerGuid = Guid.NewGuid(),
                PasswordFormat = PasswordFormat.Clear,
                AdminComment = "Built-in system record used for background tasks.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.BackgroundTask,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            backgroundTaskUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(backgroundTaskUser);
        }

        protected virtual void InstallCustomerAction()
        {
            var customerActionConditionType = new List<CustomerActionConditionType>()
            {
                new CustomerActionConditionType()
                {
                    Id = 1,
                    Name = "Product"
                },
                new CustomerActionConditionType()
                {
                    Id = 2,
                    Name = "Category"
                },
                new CustomerActionConditionType()
                {
                    Id = 3,
                    Name = "Manufacturer"
                },
                new CustomerActionConditionType()
                {
                    Id = 4,
                    Name = "Vendor"
                },
                new CustomerActionConditionType()
                {
                    Id = 5,
                    Name = "Product attribute"
                },
                new CustomerActionConditionType()
                {
                    Id = 6,
                    Name = "Product specification"
                },
                new CustomerActionConditionType()
                {
                    Id = 7,
                    Name = "Customer role"
                },
                new CustomerActionConditionType()
                {
                    Id = 8,
                    Name = "Customer tag"
                },
                new CustomerActionConditionType()
                {
                    Id = 9,
                    Name = "Customer register field"
                },
                new CustomerActionConditionType()
                {
                    Id = 10,
                    Name = "Custom Customer Attribute"
                },
                new CustomerActionConditionType()
                {
                    Id = 11,
                    Name = "URL Referrer"
                },
                new CustomerActionConditionType()
                {
                    Id = 12,
                    Name = "Url Current"
                }
            };
            _customerActionConditionType.Insert(customerActionConditionType);

            var customerActionType = new List<CustomerActionType>()
            {
                new CustomerActionType()
                {
                    Id = 1,
                    Name = "Add to cart",
                    SystemKeyword = "AddToCart",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                },
                new CustomerActionType()
                {
                    Id = 2,
                    Name = "Add order",
                    SystemKeyword = "AddOrder",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                },
                new CustomerActionType()
                {
                    Id = 3,
                    Name = "Viewed",
                    SystemKeyword = "Viewed",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 6, 7, 8, 9, 10}
                },
                new CustomerActionType()
                {
                    Id = 4,
                    Name = "Url",
                    SystemKeyword = "Url",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10, 11, 12}
                },
                new CustomerActionType()
                {
                    Id = 5,
                    Name = "Customer Registration",
                    SystemKeyword = "Registration",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10}
                }
            };
            _customerActionType.Insert(customerActionType);

        }

        protected virtual void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
            var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));
        }

        protected virtual void InstallEmailAccounts()
        {
            var emailAccounts = new List<EmailAccount>
                               {
                                   new EmailAccount
                                       {
                                           Email = "test@mail.com",
                                           DisplayName = "Store name",
                                           Host = "smtp.mail.com",
                                           Port = 25,
                                           Username = "123",
                                           Password = "123",
                                           EnableSsl = false,
                                           UseDefaultCredentials = false
                                       },
                               };
            _emailAccountRepository.Insert(emailAccounts);
        }

        protected virtual void InstallMessageTemplates()
        {
            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
                               {
                                   new MessageTemplate
                                       {
                                           Name = "Blog.BlogComment",
                                           Subject = "%Store.Name%. New blog comment.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new blog comment has been created for blog post \"%BlogComment.BlogPostTitle%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.BackInStock",
                                           Subject = "%Store.Name%. Back in stock notification",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%, <br />Product <a target=\"_blank\" href=\"%BackInStockSubscription.ProductUrl%\">%BackInStockSubscription.ProductName%</a> is in stock.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.EmailValidationMessage",
                                           Subject = "%Store.Name%. Email validation",
                                           Body = "<a href=\"%Store.URL%\">%Store.Name%</a>  <br />  <br />  To activate your account <a href=\"%Customer.AccountActivationURL%\">click here</a>.     <br />  <br />  %Store.Name%",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewPM",
                                           Subject = "%Store.Name%. You have received a new private message",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />You have received a new private message.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.PasswordRecovery",
                                           Subject = "%Store.Name%. Password recovery",
                                           Body = "<a href=\"%Store.URL%\">%Store.Name%</a>  <br />  <br />  To change your password <a href=\"%Customer.PasswordRecoveryURL%\">click here</a>.     <br />  <br />  %Store.Name%",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.WelcomeMessage",
                                           Subject = "Welcome to %Store.Name%",
                                           Body = "We welcome you to <a href=\"%Store.URL%\"> %Store.Name%</a>.<br /><br />You can now take part in the various services we have to offer you. Some of these services include:<br /><br />Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.<br />Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.<br />Order History - View your history of purchases that you have made with us.<br />Products Reviews - Share your opinions on products with our other customers.<br /><br />For help with any of our online services, please email the store-owner: <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.<br /><br />Note: This email address was provided on our registration page. If you own the email and did not register on our site, please send an email to <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumPost",
                                           Subject = "%Store.Name%. New Post Notification.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new post has been created in the topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.<br /><br />Post author: %Forums.PostAuthor%<br />Post body: %Forums.PostBody%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumTopic",
                                           Subject = "%Store.Name%. New Topic Notification.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> has been created at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "GiftCard.Notification",
                                           Subject = "%GiftCard.SenderName% has sent you a gift card for %Store.Name%",
                                           Body = "<p>You have received a gift card for %Store.Name%</p><p>Dear %GiftCard.RecipientName%, <br /><br />%GiftCard.SenderName% (%GiftCard.SenderEmail%) has sent you a %GiftCard.Amount% gift cart for <a href=\"%Store.URL%\"> %Store.Name%</a></p><p>You gift card code is %GiftCard.CouponCode%</p><p>%GiftCard.Message%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewCustomer.Notification",
                                           Subject = "%Store.Name%. New customer registration",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new customer registered with your store. Below are the customer's details:<br />Full name: %Customer.FullName%<br />Email: %Customer.Email%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewReturnRequest.StoreOwnerNotification",
                                           Subject = "%Store.Name%. New return request.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% has just submitted a new return request. Details are below:<br />Request ID: %ReturnRequest.ID%<br />Product: %ReturnRequest.Product.Quantity% x Product: %ReturnRequest.Product.Name%<br />Reason for return: %ReturnRequest.Reason%<br />Requested action: %ReturnRequest.RequestedAction%<br />Customer comments:<br />%ReturnRequest.CustomerComment%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "News.NewsComment",
                                           Subject = "%Store.Name%. New news comment.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new news comment has been created for news \"%NewsComment.NewsTitle%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.ActivationMessage",
                                           Subject = "%Store.Name%. Subscription activation message.",
                                           Body = "<p><a href=\"%NewsLetterSubscription.ActivationUrl%\">Click here to confirm your subscription to our list.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.DeactivationMessage",
                                           Subject = "%Store.Name%. Subscription deactivation message.",
                                           Body = "<p><a href=\"%NewsLetterSubscription.DeactivationUrl%\">Click here to unsubscribe from our newsletter.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewVATSubmitted.StoreOwnerNotification",
                                           Subject = "%Store.Name%. New VAT number is submitted.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just submitted a new VAT number. Details are below:<br />VAT number: %Customer.VatNumber%<br />VAT number status: %Customer.VatNumberStatus%<br />Received name: %VatValidationResult.Name%<br />Received address: %VatValidationResult.Address%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderCancelled.CustomerNotification",
                                           Subject = "%Store.Name%. Your order cancelled",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Your order has been cancelled. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderCompleted.CustomerNotification",
                                           Subject = "%Store.Name%. Your order completed",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Your order has been completed. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentDelivered.CustomerNotification",
                                           Subject = "Your order from %Store.Name% has been delivered.",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /> <br /> Hello %Order.CustomerFullName%, <br /> Good news! You order has been delivered. <br /> Order Number: %Order.OrderNumber%<br /> Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br /> Date Ordered: %Order.CreatedOn%<br /> <br /> <br /> <br /> Billing Address<br /> %Order.BillingFirstName% %Order.BillingLastName%<br /> %Order.BillingAddress1%<br /> %Order.BillingCity% %Order.BillingZipPostalCode%<br /> %Order.BillingStateProvince% %Order.BillingCountry%<br /> <br /> <br /> <br /> Shipping Address<br /> %Order.ShippingFirstName% %Order.ShippingLastName%<br /> %Order.ShippingAddress1%<br /> %Order.ShippingCity% %Order.ShippingZipPostalCode%<br /> %Order.ShippingStateProvince% %Order.ShippingCountry%<br /> <br /> Shipping Method: %Order.ShippingMethod% <br /> <br /> Delivered Products: <br /> <br /> %Shipment.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.CustomerNotification",
                                           Subject = "Order receipt from %Store.Name%.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Purchase Receipt for Order #%Order.OrderNumber%",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentSent.CustomerNotification",
                                           Subject = "Your order from %Store.Name% has been shipped.",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%!, <br />Good news! You order has been shipped. <br />Order Number: %Order.OrderNumber%<br />Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod% <br /> <br /> Shipped Products: <br /> <br /> %Shipment.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Product.ProductReview",
                                           Subject = "%Store.Name%. New product review.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new product review has been written for product \"%ProductReview.ProductName%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "QuantityBelow.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Quantity below notification. %Product.Name%",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Product.Name% (ID: %Product.ID%) low quantity. <br /><br />Quantity: %Product.StockQuantity%<br /></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "QuantityBelow.AttributeCombination.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Quantity below notification. %Product.Name%",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Product.Name% (ID: %Product.ID%) low quantity. <br />%AttributeCombination.Formatted%<br />Quantity: %AttributeCombination.StockQuantity%<br /></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ReturnRequestStatusChanged.CustomerNotification",
                                           Subject = "%Store.Name%. Return request status was changed.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%,<br />Your return request #%ReturnRequest.ID% status has been changed.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.EmailAFriend",
                                           Subject = "%Store.Name%. Referred Item",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />%EmailAFriend.Email% was shopping on %Store.Name% and wanted to share the following item with you. <br /><br /><b><a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">%Product.Name%</a></b> <br />%Product.ShortDescription% <br /><br />For more info click <a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">here</a> <br /><br /><br />%EmailAFriend.PersonalMessage%<br /><br />%Store.Name%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Wishlist.EmailAFriend",
                                           Subject = "%Store.Name%. Wishlist",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />%Wishlist.Email% was shopping on %Store.Name% and wanted to share a wishlist with you. <br /><br /><br />For more info click <a target=\"_blank\" href=\"%Wishlist.URLForCustomer%\">here</a> <br /><br /><br />%Wishlist.PersonalMessage%<br /><br />%Store.Name%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewOrderNote",
                                           Subject = "%Store.Name%. New order note has been added",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%, <br />New order note has been added to your account:<br />\"%Order.NewNoteText%\".<br /><a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "RecurringPaymentCancelled.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Recurring payment cancelled",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just cancelled a recurring payment ID=%RecurringPayment.ID%.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.VendorNotification",
                                           Subject = "%Store.Name%. Order placed",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just placed an order. <br /><br />Order Number: %Order.OrderNumber%<br />Date Ordered: %Order.CreatedOn%<br /><br />%Order.Product(s)%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Order #%Order.OrderNumber% has been just paid<br />Date Ordered: %Order.CreatedOn%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.CustomerNotification",
                                           Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Order #%Order.OrderNumber% has been just paid. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.VendorNotification",
                                           Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Order #%Order.OrderNumber% has been just paid. <br /><br />Order Number: %Order.OrderNumber%<br />Date Ordered: %Order.CreatedOn%<br /><br />%Order.Product(s)%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                        {
                                           Name = "OrderRefunded.CustomerNotification",
                                           Subject = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.<br /><br />Amount refunded: %Order.AmountRefunded%<br /><br />Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                        {
                                           Name = "OrderRefunded.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                                           Body = "%Store.Name%. Order #%Order.OrderNumber% refunded', N'<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Order #%Order.OrderNumber% has been just refunded<br /><br />Amount refunded: %Order.AmountRefunded%<br /><br />Date Ordered: %Order.CreatedOn%</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "VendorAccountApply.StoreOwnerNotification",
                                           Subject = "%Store.Name%. New vendor account submitted.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just submitted for a vendor account. Details are below:<br />Vendor name: %Vendor.Name%<br />Vendor email: %Vendor.Email%<br /><br />You can activate it in admin area.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       }
                               };
            _messageTemplateRepository.Insert(messageTemplates);
        }

        protected virtual void InstallTopics()
        {
            var defaultTopicTemplate =
                _topicTemplateRepository.Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                throw new Exception("Topic template cannot be loaded");

            var topics = new List<Topic>
                               {
                                   new Topic
                                       {
                                           SystemName = "AboutUs",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterColumn1 = true,
                                           DisplayOrder = 20,
                                           Title = "About us",
                                           Body = "<p>Put your &quot;About Us&quot; information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "CheckoutAsGuestOrRegister",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p><strong>Register and save time!</strong><br />Register with us for future convenience:</p><ul><li>Fast and easy check out</li><li>Easy access to your order history and status</li></ul>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "ConditionsOfUse",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterColumn1 = true,
                                           DisplayOrder = 15,
                                           Title = "Conditions of Use",
                                           Body = "<p>Put your conditions of use information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "ContactUs",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Put your contact information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "ForumWelcomeMessage",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "Forums",
                                           Body = "<p>Put your welcome message here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "HomePageText",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "Welcome to our store",
                                           Body = "<p>Online shopping is the process consumers go through to purchase products or services over the Internet. You can edit this in the admin site.</p><p>If you have questions, see the <a href=\"http://www.grandnode.com/\">Documentation</a>, or post in the <a href=\"http://www.grandnode.com/boards/\">Forums</a> at <a href=\"http://www.grandnode.com\">grandnode.com</a></p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "LoginRegistrationInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "About login / registration",
                                           Body = "<p>Put your login / registration information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "PrivacyInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterColumn1 = true,
                                           DisplayOrder = 10,
                                           Title = "Privacy notice",
                                           Body = "<p>Put your privacy policy information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "PageNotFound",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p><strong>The page you requested was not found, and we have a fine guess why.</strong></p><ul><li>If you typed the URL directly, please make sure the spelling is correct.</li><li>The page no longer exists. In this case, we profusely apologize for the inconvenience and for any damage this may cause.</li></ul>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "ShippingInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterColumn1 = true,
                                           DisplayOrder = 5,
                                           Title = "Shipping & returns",
                                           Body = "<p>Put your shipping &amp; returns information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                                   new Topic
                                       {
                                           SystemName = "ApplyVendor",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Put your apply vendor instructions here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id
                                       },
                               };
            _topicRepository.Insert(topics);

            var ltopics = from p in _topicRepository.Table
                          select p;
            //search engine names
            foreach (var topic in ltopics)
            {
                var seName = topic.ValidateSeName("", !String.IsNullOrEmpty(topic.Title) ? topic.Title : topic.SystemName, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = topic.Id,
                    EntityName = "Topic",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = seName
                });
                topic.SeName = seName;
                _topicRepository.Update(topic);
            }

        }

        protected virtual void InstallSettings()
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            settingService.SaveSetting(new PdfSettings
            {
                LogoPictureId = 0,
                LetterPageSizeEnabled = false,
                RenderOrderNotes = true,
                FontFileName = "FreeSerif.ttf",
                InvoiceFooterTextColumn1 = null,
                InvoiceFooterTextColumn2 = null,
            });

            settingService.SaveSetting(new CommonSettings
            {
                StoreInDatabaseContactUsForm = true,
                UseSystemEmailForContactUsForm = true,
                UseStoredProceduresIfSupported = true,
                SitemapEnabled = true,
                SitemapIncludeCategories = true,
                SitemapIncludeManufacturers = true,
                SitemapIncludeProducts = false,
                DisplayJavaScriptDisabledWarning = false,
                UseFullTextSearch = false,
                FullTextMode = FulltextSearchMode.ExactMatch,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                RenderXuaCompatible = false,
                XuaCompatibleValue = "IE=edge"
            });

            settingService.SaveSetting(new SeoSettings
            {
                PageTitleSeparator = ". ",
                PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                DefaultTitle = "Your store",
                DefaultMetaKeywords = "",
                DefaultMetaDescription = "",
                GenerateProductMetaDescription = true,
                ConvertNonWesternChars = false,
                AllowUnicodeCharsInUrls = true,
                CanonicalUrlsEnabled = false,
                WwwRequirement = WwwRequirement.NoMatter,
                //we disable bundling out of the box because it requires a lot of server resources
                EnableJsBundling = false,
                EnableCssBundling = false,
                TwitterMetaTags = true,
                OpenGraphMetaTags = true,
                ReservedUrlRecordSlugs = new List<string>
                    {
                        "admin",
                        "install",
                        "recentlyviewedproducts",
                        "newproducts",
                        "compareproducts",
                        "clearcomparelist",
                        "setproductreviewhelpfulness",
                        "login",
                        "register",
                        "logout",
                        "cart",
                        "wishlist",
                        "emailwishlist",
                        "checkout",
                        "onepagecheckout",
                        "contactus",
                        "passwordrecovery",
                        "subscribenewsletter",
                        "blog",
                        "boards",
                        "inboxupdate",
                        "sentupdate",
                        "news",
                        "sitemap",
                        "search",
                        "config",
                        "eucookielawaccept",
                        "page-not-found"
                    },
            });

            settingService.SaveSetting(new AdminAreaSettings
            {
                DefaultGridPageSize = 15,
                GridPageSizes = "10, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false
            });

            settingService.SaveSetting(new CatalogSettings
            {
                AllowViewUnpublishedProductPage = true,
                DisplayDiscontinuedMessageForUnpublishedProducts = true,
                PublishBackProductWhenCancellingOrders = false,
                ShowProductSku = false,
                ShowManufacturerPartNumber = false,
                ShowGtin = false,
                ShowFreeShippingNotification = true,
                AllowProductSorting = true,
                AllowProductViewModeChanging = true,
                DefaultViewMode = "grid",
                ShowProductsFromSubcategories = true,
                ShowCategoryProductNumber = false,
                ShowCategoryProductNumberIncludingSubcategories = false,
                CategoryBreadcrumbEnabled = true,
                ShowShareButton = true,
                PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/250/addthis_widget.js#pubid=grandnode\"></script><!-- AddThis Button END -->",
                ProductReviewsMustBeApproved = false,
                DefaultProductRatingValue = 5,
                AllowAnonymousUsersToReviewProduct = false,
                NotifyStoreOwnerAboutNewProductReviews = false,
                EmailAFriendEnabled = true,
                AllowAnonymousUsersToEmailAFriend = false,
                RecentlyViewedProductsNumber = 3,
                RecentlyViewedProductsEnabled = true,
                RecommendedProductsEnabled = false,
                NewProductsNumber = 6,
                NewProductsEnabled = true,
                CompareProductsEnabled = true,
                CompareProductsNumber = 4,
                ProductSearchAutoCompleteEnabled = true,
                ProductSearchAutoCompleteNumberOfProducts = 10,
                ProductSearchTermMinimumLength = 3,
                ShowProductImagesInSearchAutoComplete = false,
                ShowBestsellersOnHomepage = false,
                NumberOfBestsellersOnHomepage = 4,
                SearchPageProductsPerPage = 6,
                SearchPageAllowCustomersToSelectPageSize = true,
                SearchPagePageSizeOptions = "6, 3, 9, 18",
                ProductsAlsoPurchasedEnabled = true,
                ProductsAlsoPurchasedNumber = 3,
                AjaxProcessAttributeChange = true,
                NumberOfProductTags = 15,
                ProductsByTagPageSize = 6,
                IncludeShortDescriptionInCompareProducts = false,
                IncludeFullDescriptionInCompareProducts = false,
                IncludeFeaturedProductsInNormalLists = false,
                DisplayTierPricesWithDiscounts = true,
                IgnoreDiscounts = false,
                IgnoreFeaturedProducts = false,
                IgnoreAcl = true,
                IgnoreStoreLimitations = true,
                IgnoreFilterableSpecAttributeOption = true,
                IgnoreFilterableAvailableStartEndDateTime = true,
                CacheProductPrices = false,
                ProductsByTagAllowCustomersToSelectPageSize = true,
                ProductsByTagPageSizeOptions = "6, 3, 9, 18",
                MaximumBackInStockSubscriptions = 200,
                ManufacturersBlockItemsToDisplay = 2,
                DisplayTaxShippingInfoFooter = false,
                DisplayTaxShippingInfoProductDetailsPage = false,
                DisplayTaxShippingInfoProductBoxes = false,
                DisplayTaxShippingInfoShoppingCart = false,
                DisplayTaxShippingInfoWishlist = false,
                DisplayTaxShippingInfoOrderDetailsPage = false,
                DefaultCategoryPageSizeOptions = "6, 3, 9",
                DefaultManufacturerPageSize = 6
            });

            settingService.SaveSetting(new LocalizationSettings
            {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.Name == "English").Id,
                UseImagesForLanguageSelection = false,
                SeoFriendlyUrlsForLanguagesEnabled = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                LoadAllUrlRecordsOnStartup = false,
                IgnoreRtlPropertyForAdminArea = false,
            });

            settingService.SaveSetting(new CustomerSettings
            {
                UsernamesEnabled = false,
                CheckUsernameAvailabilityEnabled = false,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = "SHA1",
                PasswordMinLength = 6,
                PasswordRecoveryLinkDaysValid = 7,
                UserRegistrationType = UserRegistrationType.Standard,
                AllowCustomersToUploadAvatars = false,
                AvatarMaximumSizeBytes = 20000,
                DefaultAvatarEnabled = true,
                ShowCustomersLocation = false,
                ShowCustomersJoinDate = false,
                AllowViewingProfiles = false,
                NotifyNewCustomerRegistration = false,
                HideDownloadableProductsTab = false,
                HideBackInStockSubscriptionsTab = false,
                DownloadableProductsValidateUser = false,
                CustomerNameFormat = CustomerNameFormat.ShowFirstName,
                GenderEnabled = true,
                DateOfBirthEnabled = true,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = null,
                CompanyEnabled = true,
                StreetAddressEnabled = false,
                StreetAddress2Enabled = false,
                ZipPostalCodeEnabled = false,
                CityEnabled = false,
                CountryEnabled = false,
                CountryRequired = false,
                StateProvinceEnabled = false,
                StateProvinceRequired = false,
                PhoneEnabled = false,
                FaxEnabled = false,
                AcceptPrivacyPolicyEnabled = false,
                NewsletterEnabled = true,
                NewsletterTickedByDefault = true,
                HideNewsletterBlock = false,
                RegistrationFreeShipping = false,
                NewsletterBlockAllowToUnsubscribe = false,
                OnlineCustomerMinutes = 20,
                StoreLastVisitedPage = false,
                SuffixDeletedCustomers = false,
            });

            settingService.SaveSetting(new AddressSettings
            {
                CompanyEnabled = true,
                StreetAddressEnabled = true,
                StreetAddressRequired = true,
                StreetAddress2Enabled = true,
                ZipPostalCodeEnabled = true,
                ZipPostalCodeRequired = true,
                CityEnabled = true,
                CityRequired = true,
                CountryEnabled = true,
                StateProvinceEnabled = true,
                PhoneEnabled = true,
                PhoneRequired = true,
                FaxEnabled = true,
            });

            settingService.SaveSetting(new MediaSettings
            {
                AvatarPictureSize = 120,
                ProductThumbPictureSize = 415,
                ProductDetailsPictureSize = 550,
                ProductThumbPictureSizeOnProductDetailsPage = 100,
                AssociatedProductPictureSize = 220,
                CategoryThumbPictureSize = 450,
                ManufacturerThumbPictureSize = 420,
                VendorThumbPictureSize = 450,
                CartThumbPictureSize = 80,
                MiniCartThumbPictureSize = 70,
                AutoCompleteSearchThumbPictureSize = 20,
                ImageSquarePictureSize = 32,
                MaximumImageSize = 1980,
                DefaultPictureZoomEnabled = false,
                DefaultImageQuality = 80,
                MultipleThumbDirectories = false
            });

            settingService.SaveSetting(new StoreInformationSettings
            {
                StoreClosed = false,
                DefaultStoreTheme = "GrandNodeTheme",
                AllowCustomerToSelectTheme = true,
                DisplayMiniProfilerInPublicStore = false,
                DisplayEuCookieLawWarning = false,
                FacebookLink = "https://www.facebook.com/grandnodecom",
                TwitterLink = "https://twitter.com/grandnode",
                YoutubeLink = "http://www.youtube.com/user/grandnode",
                GooglePlusLink = "https://plus.google.com/104905799281078572776",
                HidePoweredByGrandNode = false
            });

            settingService.SaveSetting(new ExternalAuthenticationSettings
            {
                AutoRegisterEnabled = true,
                RequireEmailValidation = false
            });

            settingService.SaveSetting(new RewardPointsSettings
            {
                Enabled = true,
                ExchangeRate = 1,
                PointsForRegistration = 0,
                PointsForPurchases_Amount = 10,
                PointsForPurchases_Points = 1,
                PointsForPurchases_Awarded = OrderStatus.Complete,
                PointsForPurchases_Canceled = OrderStatus.Cancelled,
                DisplayHowMuchWillBeEarned = true,
                PointsAccumulatedForAllStores = true,
            });

            settingService.SaveSetting(new CurrencySettings
            {
                DisplayCurrencyLabel = false,
                PrimaryStoreCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                AutoUpdateEnabled = false
            });

            settingService.SaveSetting(new MeasureSettings
            {
                BaseDimensionId = _measureDimensionRepository.Table.Single(m => m.SystemKeyword == "inches").Id,
                BaseWeightId = _measureWeightRepository.Table.Single(m => m.SystemKeyword == "lb").Id,
            });

            settingService.SaveSetting(new MessageTemplatesSettings
            {
                CaseInvariantReplacement = false,
                Color1 = "#b9babe",
                Color2 = "#ebecee",
                Color3 = "#dde2e6",
            });

            settingService.SaveSetting(new ShoppingCartSettings
            {
                DisplayCartAfterAddingProduct = false,
                DisplayWishlistAfterAddingProduct = false,
                MaximumShoppingCartItems = 1000,
                MaximumWishlistItems = 1000,
                AllowOutOfStockItemsToBeAddedToWishlist = false,
                MoveItemsFromWishlistToCart = true,
                ShowProductImagesOnShoppingCart = true,
                ShowProductImagesOnWishList = true,
                ShowDiscountBox = true,
                ShowGiftCardBox = true,
                CrossSellsNumber = 4,
                EmailWishlistEnabled = true,
                AllowAnonymousUsersToEmailWishlist = false,
                MiniShoppingCartEnabled = true,
                ShowProductImagesInMiniShoppingCart = true,
                MiniShoppingCartProductNumber = 5,
                RoundPricesDuringCalculation = true,
                GroupTierPricesForDistinctShoppingCartItems = false,
                AllowCartItemEditing = true,
                RenderAssociatedAttributeValueQuantity = false
            });

            settingService.SaveSetting(new OrderSettings
            {
                IsReOrderAllowed = true,
                MinOrderSubtotalAmount = 0,
                MinOrderSubtotalAmountIncludingTax = false,
                MinOrderTotalAmount = 0,
                AnonymousCheckoutAllowed = true,
                TermsOfServiceOnShoppingCartPage = true,
                TermsOfServiceOnOrderConfirmPage = false,
                OnePageCheckoutEnabled = true,
                OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab = false,
                DisableBillingAddressCheckoutStep = false,
                DisableOrderCompletedPage = false,
                AttachPdfInvoiceToOrderPlacedEmail = false,
                AttachPdfInvoiceToOrderCompletedEmail = false,
                AttachPdfInvoiceToOrderPaidEmail = false,
                ReturnRequestsEnabled = true,
                NumberOfDaysReturnRequestAvailable = 365,
                MinimumOrderPlacementInterval = 30,
            });

            settingService.SaveSetting(new SecuritySettings
            {
                ForceSslForAllPages = false,
                EncryptionKey = CommonHelper.GenerateRandomDigitCode(16),
                AdminAreaAllowedIpAddresses = null,
                EnableXsrfProtectionForAdminArea = true,
                EnableXsrfProtectionForPublicStore = true,
                HoneypotEnabled = false,
                HoneypotInputName = "hpinput"
            });

            settingService.SaveSetting(new ShippingSettings
            {
                ActiveShippingRateComputationMethodSystemNames = new List<string> { "Shipping.FixedRate" },
                AllowPickUpInStore = true,
                PickUpInStoreFee = decimal.Zero,
                UseWarehouseLocation = false,
                NotifyCustomerAboutShippingFromMultipleLocations = false,
                FreeShippingOverXEnabled = false,
                FreeShippingOverXValue = decimal.Zero,
                FreeShippingOverXIncludingTax = false,
                EstimateShippingEnabled = true,
                DisplayShipmentEventsToCustomers = false,
                DisplayShipmentEventsToStoreOwner = false,
                ReturnValidOptionsIfThereAreAny = true,
                BypassShippingMethodSelectionIfOnlyOne = false,
                UseCubeRootMethod = true
            });

            settingService.SaveSetting(new PaymentSettings
            {
                ActivePaymentMethodSystemNames = new List<string>
                    {
                        "Payments.CheckMoneyOrder",
                        "Payments.Manual",
                        "Payments.PayInStore",
                        "Payments.PurchaseOrder",
                    },
                AllowRePostingPayments = true,
                BypassPaymentMethodSelectionIfOnlyOne = true,
            });

            settingService.SaveSetting(new TaxSettings
            {
                TaxBasedOn = TaxBasedOn.BillingAddress,
                TaxDisplayType = TaxDisplayType.ExcludingTax,
                ActiveTaxProviderSystemName = "Tax.FixedRate",
                DefaultTaxAddressId = 0,
                DisplayTaxSuffix = false,
                DisplayTaxRates = false,
                PricesIncludeTax = false,
                AllowCustomersToSelectTaxDisplayType = false,
                ForceTaxExclusionFromOrderSubtotal = false,
                HideZeroTax = false,
                HideTaxInOrderSummary = false,
                ShippingIsTaxable = false,
                ShippingPriceIncludesTax = false,
                ShippingTaxClassId = 0,
                PaymentMethodAdditionalFeeIsTaxable = false,
                PaymentMethodAdditionalFeeIncludesTax = false,
                PaymentMethodAdditionalFeeTaxClassId = 0,
                EuVatEnabled = false,
                EuVatShopCountryId = 0,
                EuVatAllowVatExemption = true,
                EuVatUseWebService = false,
                EuVatAssumeValid = false,
                EuVatEmailAdminWhenNewVatSubmitted = false
            });

            settingService.SaveSetting(new DateTimeSettings
            {
                DefaultStoreTimeZoneId = "",
                AllowCustomersToSetTimeZone = false
            });

            settingService.SaveSetting(new BlogSettings
            {
                Enabled = true,
                PostsPageSize = 10,
                AllowNotRegisteredUsersToLeaveComments = true,
                NotifyAboutNewBlogComments = false,
                NumberOfTags = 15,
                ShowHeaderRssUrl = false,
            });
            settingService.SaveSetting(new NewsSettings
            {
                Enabled = true,
                AllowNotRegisteredUsersToLeaveComments = true,
                NotifyAboutNewNewsComments = false,
                ShowNewsOnMainPage = true,
                MainPageNewsCount = 3,
                NewsArchivePageSize = 10,
                ShowHeaderRssUrl = false,
            });

            settingService.SaveSetting(new ForumSettings
            {
                ForumsEnabled = false,
                RelativeDateTimeFormattingEnabled = true,
                AllowCustomersToDeletePosts = false,
                AllowCustomersToEditPosts = false,
                AllowCustomersToManageSubscriptions = false,
                AllowGuestsToCreatePosts = false,
                AllowGuestsToCreateTopics = false,
                TopicSubjectMaxLength = 450,
                PostMaxLength = 4000,
                StrippedTopicMaxLength = 45,
                TopicsPageSize = 10,
                PostsPageSize = 10,
                SearchResultsPageSize = 10,
                ActiveDiscussionsPageSize = 50,
                LatestCustomerPostsPageSize = 10,
                ShowCustomersPostCount = true,
                ForumEditor = EditorType.BBCodeEditor,
                SignaturesEnabled = true,
                AllowPrivateMessages = false,
                ShowAlertForPM = false,
                PrivateMessagesPageSize = 10,
                ForumSubscriptionsPageSize = 10,
                NotifyAboutPrivateMessages = false,
                PMSubjectMaxLength = 450,
                PMTextMaxLength = 4000,
                HomePageActiveDiscussionsTopicCount = 5,
                ActiveDiscussionsFeedEnabled = false,
                ActiveDiscussionsFeedCount = 25,
                ForumFeedsEnabled = false,
                ForumFeedCount = 10,
                ForumSearchTermMinimumLength = 3,
            });

            settingService.SaveSetting(new VendorSettings
            {
                DefaultVendorPageSizeOptions = "6, 3, 9",
                VendorsBlockItemsToDisplay = 0,
                ShowVendorOnProductDetailsPage = true,
                AllowCustomersToContactVendors = true,
                AllowCustomersToApplyForVendorAccount = true
            });

            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            settingService.SaveSetting(new EmailAccountSettings
            {
                DefaultEmailAccountId = eaGeneral.Id
            });

            settingService.SaveSetting(new WidgetSettings
            {
                ActiveWidgetSystemNames = new List<string> { "Widgets.NivoSlider" },
            });
        }

        protected virtual void InstallCheckoutAttributes()
        {
            var ca1 = new CheckoutAttribute
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Gift wrapping",
                IsRequired = true,
                ShippableProductRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1,
            };
            ca1.CheckoutAttributeValues.Add(new CheckoutAttributeValue
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "No",
                PriceAdjustment = 0,
                DisplayOrder = 1,
                IsPreSelected = true,
                CheckoutAttributeId = 1,
            });
            ca1.CheckoutAttributeValues.Add(new CheckoutAttributeValue
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Yes",
                PriceAdjustment = 10,
                DisplayOrder = 2,
                CheckoutAttributeId = 1,
            });
            var checkoutAttributes = new List<CheckoutAttribute>
                                {
                                    ca1,
                                };
            _checkoutAttributeRepository.Insert(checkoutAttributes);
        }

        protected virtual void InstallSpecificationAttributes()
        {
            var sa1 = new SpecificationAttribute
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Screensize",
                DisplayOrder = 1,
            };
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {

                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "13.0''",
                DisplayOrder = 2,
                SpecificationAttributeId = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "13.3''",
                DisplayOrder = 3,
                SpecificationAttributeId = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "14.0''",
                DisplayOrder = 4,
                SpecificationAttributeId = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 4,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "15.0''",
                DisplayOrder = 4,
                SpecificationAttributeId = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 5,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "15.6''",
                DisplayOrder = 5,
                SpecificationAttributeId = 1,
            });
            var sa2 = new SpecificationAttribute
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "CPU Type",
                DisplayOrder = 2,
            };
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Intel Core i5",
                DisplayOrder = 1,
                SpecificationAttributeId = 2,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Intel Core i7",
                DisplayOrder = 2,
                SpecificationAttributeId = 2,
            });
            var sa3 = new SpecificationAttribute
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Memory",
                DisplayOrder = 3,
            };
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "4 GB",
                DisplayOrder = 1,
                SpecificationAttributeId = 3,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "8 GB",
                DisplayOrder = 2,
                SpecificationAttributeId = 3,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "16 GB",
                DisplayOrder = 3,
                SpecificationAttributeId = 3,
            });
            var sa4 = new SpecificationAttribute
            {
                Id = 4,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "Hardrive",
                DisplayOrder = 5,
            };
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "128 GB",
                DisplayOrder = 7,
                SpecificationAttributeId = 4,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "500 GB",
                DisplayOrder = 4,
                SpecificationAttributeId = 4,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                Name = "1 TB",
                DisplayOrder = 3,
                SpecificationAttributeId = 4,
            });
            var specificationAttributes = new List<SpecificationAttribute>
                                {
                                    sa1,
                                    sa2,
                                    sa3,
                                    sa4
                                };
            _specificationAttributeRepository.Insert(specificationAttributes);
        }

        protected virtual void InstallProductAttributes()
        {
            var productAttributes = new List<ProductAttribute>
            {
                new ProductAttribute
                {
                    Name = "Color",
                },
                new ProductAttribute
                {
                    Name = "Custom Text",
                },
                new ProductAttribute
                {
                    Name = "HDD",
                },
                new ProductAttribute
                {
                    Name = "OS",
                },
                new ProductAttribute
                {
                    Name = "Processor",
                },
                new ProductAttribute
                {
                    Name = "RAM",
                },
                new ProductAttribute
                {
                    Name = "Size",
                },
                new ProductAttribute
                {
                    Name = "Software",
                },
            };
            _productAttributeRepository.Insert(productAttributes);
        }

        protected virtual void InstallCategories()
        {
            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = CommonHelper.MapPath("~/content/samples/");



            var categoryTemplateInGridAndLines = _categoryTemplateRepository
                .Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
            if (categoryTemplateInGridAndLines == null)
                throw new Exception("Category template cannot be loaded");


            //categories
            var allCategories = new List<Category>();
            var categoryComputers = new Category
            {
                Id = 1,
                Name = "Computers",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_computers.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Computers")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryComputers);

            var categoryDesktops = new Category
            {
                Id = 2,
                Name = "Desktops",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_desktops.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Desktops")).Id,
                PriceRanges = "-1000;1000-1200;1200-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryDesktops);

            var categoryNotebooks = new Category
            {
                Id = 3,
                Name = "Notebooks",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_notebooks.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Notebooks")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryNotebooks);

            var categorySoftware = new Category
            {
                Id = 4,
                Name = "Software",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_software.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Software")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categorySoftware);

            var categoryElectronics = new Category
            {
                Id = 5,
                Name = "Electronics",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_electronics.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Electronics")).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryElectronics);

            var categoryCameraPhoto = new Category
            {
                Id = 6,
                Name = "Camera & photo",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_camera_photo.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Camera, photo")).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryCameraPhoto);

            var categoryCellPhones = new Category
            {
                Id = 7,
                Name = "Cell phones",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_cell_phones.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Cell phones")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryCellPhones);

            var categoryOthers = new Category
            {
                Id = 8,
                Name = "Others",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_accessories.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Accessories")).Id,
                IncludeInTopMenu = true,
                PriceRanges = "-100;100-;",
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryOthers);

            var categoryApparel = new Category
            {
                Id = 9,
                Name = "Apparel",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_apparel.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Apparel")).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryApparel);

            var categoryShoes = new Category
            {
                Id = 10,
                Name = "Shoes",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_shoes.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Shoes")).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryShoes);

            var categoryClothing = new Category
            {
                Id = 11,
                Name = "Clothing",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_clothing.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Clothing")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryClothing);

            var categoryAccessories = new Category
            {
                Id = 12,
                Name = "Accessories",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_apparel_accessories.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Apparel Accessories")).Id,
                IncludeInTopMenu = true,
                PriceRanges = "-100;100-;",
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryAccessories);

            var categoryDigitalDownloads = new Category
            {
                Id = 13,
                Name = "Digital downloads",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_digital_downloads.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Digital downloads")).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 4,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryDigitalDownloads);

            var categoryBooks = new Category
            {
                Id = 14,
                Name = "Books",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MetaKeywords = "Books, Dictionary, Textbooks",
                MetaDescription = "Books category description",
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_book.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Book")).Id,
                PriceRanges = "-25;25-50;50-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryBooks);

            var categoryJewelry = new Category
            {
                Id = 15,
                Name = "Jewelry",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_jewelry.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Jewelry")).Id,
                PriceRanges = "0-500;500-700;700-3000;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 6,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryJewelry);

            var categoryGiftCards = new Category
            {
                Id = 16,
                Name = "Gift Cards",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_gift_cards.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Gift Cards")).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 7,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryGiftCards);

            _categoryRepository.Insert(allCategories);
            //search engine names
            foreach (var category in allCategories)
            {
                category.SeName = category.ValidateSeName("", category.Name, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = category.Id,
                    EntityName = "Category",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = category.SeName,
                });
                _categoryRepository.Update(category);
            }
        }

        protected virtual void InstallManufacturers()
        {
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = CommonHelper.MapPath("~/content/samples/");

            var manufacturerTemplateInGridAndLines =
                _manufacturerTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
            if (manufacturerTemplateInGridAndLines == null)
                throw new Exception("Manufacturer template cannot be loaded");

            var allManufacturers = new List<Manufacturer>();
            var manufacturerAsus = new Manufacturer
            {
                Id = 1,
                Name = "Apple",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_apple.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Apple")).Id,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            _manufacturerRepository.Insert(manufacturerAsus);
            allManufacturers.Add(manufacturerAsus);


            var manufacturerHp = new Manufacturer
            {
                Id = 2,
                Name = "HP",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_hp.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Hp")).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            _manufacturerRepository.Insert(manufacturerHp);
            allManufacturers.Add(manufacturerHp);


            var manufacturerNike = new Manufacturer
            {
                Id = 3,
                Name = "Nike",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_nike.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Nike")).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            _manufacturerRepository.Insert(manufacturerNike);
            allManufacturers.Add(manufacturerNike);

            //search engine names
            foreach (var manufacturer in allManufacturers)
            {
                manufacturer.SeName = manufacturer.ValidateSeName("", manufacturer.Name, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = manufacturer.Id,
                    EntityName = "Manufacturer",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = manufacturer.SeName
                });
                _manufacturerRepository.Update(manufacturer);
            }
        }

        protected virtual void InstallProducts(string defaultUserEmail)
        {
            var productTemplateSimple = _productTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Simple product");
            if (productTemplateSimple == null)
                throw new Exception("Simple product template could not be loaded");
            var productTemplateGrouped = _productTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Grouped product (with variants)");
            if (productTemplateGrouped == null)
                throw new Exception("Simple product template could not be loaded");

            //delivery date
            var deliveryDate = _deliveryDateRepository.Table.FirstOrDefault();
            if (deliveryDate == null)
                throw new Exception("No default deliveryDate could be loaded");

            //default customer/user
            var defaultCustomer = _customerRepository.Table.FirstOrDefault(x => x.Email == defaultUserEmail);
            if (defaultCustomer == null)
                throw new Exception("Cannot load default customer");

            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = CommonHelper.MapPath("~/content/samples/");

            //downloads
            var downloadService = EngineContext.Current.Resolve<IDownloadService>();
            var sampleDownloadsPath = CommonHelper.MapPath("~/content/samples/");

            //default store
            var defaultStore = _storeRepository.Table.FirstOrDefault();
            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            //products
            var allProducts = new List<Product>();

            #region Desktops


            var productBuildComputer = new Product
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Build your own computer",
                ShortDescription = "Build it",
                FullDescription = "<p>Fight back against cluttered workspaces with the stylish IBM zBC12 All-in-One desktop PC, featuring powerful computing resources and a stunning 20.1-inch widescreen display with stunning XBRITE-HiColor LCD technology. The black IBM zBC12 has a built-in microphone and MOTION EYE camera with face-tracking technology that allows for easy communication with friends and family. And it has a built-in DVD burner and Sony's Movie Store software so you can create a digital entertainment library for personal viewing at your convenience. Easy to setup and even easier to use, this JS-series All-in-One includes an elegantly designed keyboard and a USB mouse.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "build-your-own-computer",
                AllowCustomerReviews = true,
                Price = 1200M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                ShowOnHomePage = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        Id = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Processor").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        ProductId = 1,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                Id = 1,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2.2 GHz Intel Pentium Dual-Core E2200",
                                DisplayOrder = 1,
                                ProductId = 1,
                                ProductAttributeMappingId = 1
                            },
                            new ProductAttributeValue
                            {
                                Id = 2,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2.5 GHz Intel Pentium Dual-Core E2200",
                                IsPreSelected = true,
                                PriceAdjustment = 15,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 1
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 2,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 1,
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "RAM").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                Id = 1,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2 GB",
                                DisplayOrder = 1,
                                ProductId = 1,
                                ProductAttributeMappingId = 2
                            },
                            new ProductAttributeValue
                            {
                                Id = 2,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "4GB",
                                PriceAdjustment = 20,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 2
                            },
                            new ProductAttributeValue
                            {
                                Id = 3,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "8GB",
                                PriceAdjustment = 60,
                                DisplayOrder = 3,
                                ProductId = 1,
                                ProductAttributeMappingId = 2
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 3,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 1,
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "HDD").Id,
                        AttributeControlType = AttributeControlType.RadioList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                Id = 1,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "320 GB",
                                DisplayOrder = 1,
                                ProductId = 1,
                                ProductAttributeMappingId = 3
                            },
                            new ProductAttributeValue
                            {
                                Id = 2,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "400 GB",
                                PriceAdjustment = 100,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 3
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 4,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 1,
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "OS").Id,
                        AttributeControlType = AttributeControlType.RadioList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                Id = 1,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Vista Home",
                                PriceAdjustment = 50,
                                IsPreSelected = true,
                                DisplayOrder = 1,
                                ProductId = 1,
                                ProductAttributeMappingId = 4
                            },
                            new ProductAttributeValue
                            {
                                Id = 2,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Vista Premium",
                                PriceAdjustment = 60,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 4
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 5,
                        ProductId = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Software").Id,
                        AttributeControlType = AttributeControlType.Checkboxes,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                Id = 1,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Microsoft Office",
                                PriceAdjustment = 50,
                                IsPreSelected = true,
                                DisplayOrder = 1,
                                ProductId = 1,
                                ProductAttributeMappingId = 5
                            },
                            new ProductAttributeValue
                            {
                                Id = 2,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Acrobat Reader",
                                PriceAdjustment = 10,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 5
                            },
                            new ProductAttributeValue
                            {
                                Id = 3,
                                _id = ObjectId.GenerateNewId().ToString(),
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Total Commander",
                                PriceAdjustment = 5,
                                DisplayOrder = 2,
                                ProductId = 1,
                                ProductAttributeMappingId = 5
                            }
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        Id = 1,
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Desktops").Id,
                        DisplayOrder = 1,
                        ProductId = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productBuildComputer);

            var Picture1 = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name));
            var Picture2 = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name));

            _productRepository.Insert(productBuildComputer);

            var productpicture1 = new ProductPicture
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductId = productBuildComputer.Id,
                PictureId = Picture1.Id,
                DisplayOrder = 1
            };
            var productpicture2 = new ProductPicture
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductId = productBuildComputer.Id,
                PictureId = Picture2.Id,
                DisplayOrder = 1
            };
            productBuildComputer.ProductPictures.Add(productpicture1);
            productBuildComputer.ProductPictures.Add(productpicture2);
            _productRepository.Update(productBuildComputer);

            var productDigitalStorm = new Product
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Digital Storm VANQUISH 3 Custom Performance PC",
                ShortDescription = "Digital Storm Vanquish 3 Desktop PC",
                FullDescription = "<p>Blow the doors off today?s most demanding games with maximum detail, speed, and power for an immersive gaming experience without breaking the bank.</p><p>Stay ahead of the competition, VANQUISH 3 is fully equipped to easily handle future upgrades, keeping your system on the cutting edge for years to come.</p><p>Each system is put through an extensive stress test, ensuring you experience zero bottlenecks and get the maximum performance from your hardware.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "compaq-presario-sr1519x-pentium-4-desktop-pc-with-cdrw",
                AllowCustomerReviews = true,
                Price = 1259M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Desktops").Id,
                        DisplayOrder = 1,
                        ProductId = 2,
                        Id = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productDigitalStorm);
            productDigitalStorm.ProductPictures.Add(new ProductPicture
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DigitalStorm.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productDigitalStorm.Name)).Id,
                DisplayOrder = 1,
                ProductId = 2,
            });
            _productRepository.Insert(productDigitalStorm);

            var productLenovoIdeaCentre = new Product
            {
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo IdeaCentre 600 All-in-One PC",
                ShortDescription = "",
                FullDescription = "<p>The A600 features a 21.5in screen, DVD or optional Blu-Ray drive, support for the full beans 1920 x 1080 HD, Dolby Home Cinema certification and an optional hybrid analogue/digital TV tuner.</p><p>Connectivity is handled by 802.11a/b/g - 802.11n is optional - and an ethernet port. You also get four USB ports, a Firewire slot, a six-in-one card reader and a 1.3- or two-megapixel webcam.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "hp-iq506-touchsmart-desktop-pc",
                AllowCustomerReviews = true,
                Price = 500M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        Id = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Desktops").Id,
                        DisplayOrder = 1,
                        ProductId = 3,
                    }
                }
            };
            allProducts.Add(productLenovoIdeaCentre);
            productLenovoIdeaCentre.ProductPictures.Add(new ProductPicture
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LenovoIdeaCentre.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLenovoIdeaCentre.Name)).Id,
                DisplayOrder = 1,
                ProductId = 3,
            });
            _productRepository.Insert(productLenovoIdeaCentre);

            #endregion

            #region Notebooks

            var productAppleMacBookPro = new Product
            {
                Id = 4,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Apple MacBook Pro 13-inch",
                ShortDescription = "A groundbreaking Retina display. A new force-sensing trackpad. All-flash architecture. Powerful dual-core and quad-core Intel processors. Together, these features take the notebook to a new level of performance. And they will do the same for you in everything you create.",
                FullDescription = "<p>With fifth-generation Intel Core processors, the latest graphics, and faster flash storage, the incredibly advanced MacBook Pro with Retina display moves even further ahead in performance and battery life.* *Compared with the previous generation.</p><p>Retina display with 2560-by-1600 resolution</p><p>Fifth-generation dual-core Intel Core i5 processor</p><p>Intel Iris Graphics</p><p>Up to 9 hours of battery life1</p><p>Faster flash storage2</p><p>802.11ac Wi-Fi</p><p>Two Thunderbolt 2 ports for connecting high-performance devices and transferring data at lightning speed</p><p>Two USB 3 ports (compatible with USB 2 devices) and HDMI</p><p>FaceTime HD camera</p><p>Pages, Numbers, Keynote, iPhoto, iMovie, GarageBand included</p><p>OS X, the world's most advanced desktop operating system</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "asus-eee-pc-1000ha-10-inch-netbook",
                AllowCustomerReviews = true,
                Price = 1800M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 3,
                Length = 3,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                ShowOnHomePage = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        Id = 1,
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                        ProductId = 4,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        Id = 1,
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Apple").Id,
                        DisplayOrder = 2,
                        ProductId = 4,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        Id = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 4,
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "13.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 4,
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 3,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 4,
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "4 GB").Id
                    }
                }
            };
            allProducts.Add(productAppleMacBookPro);
            productAppleMacBookPro.ProductPictures.Add(new ProductPicture
            {
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_macbook_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleMacBookPro.Name)).Id,
                DisplayOrder = 1,
            });
            productAppleMacBookPro.ProductPictures.Add(new ProductPicture
            {
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_macbook_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleMacBookPro.Name)).Id,
                DisplayOrder = 2,
            });
            _productRepository.Insert(productAppleMacBookPro);


            var productAsusN551JK = new Product
            {
                Id = 5,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Asus N551JK-XO076H Laptop",
                ShortDescription = "Laptop Asus N551JK Intel Core i7-4710HQ 2.5 GHz, RAM 16GB, HDD 1TB, Video NVidia GTX 850M 4GB, BluRay, 15.6, Full HD, Win 8.1",
                FullDescription = "<p>The ASUS N550JX combines cutting-edge audio and visual technology to deliver an unsurpassed multimedia experience. A full HD wide-view IPS panel is tailor-made for watching movies and the intuitive touchscreen makes for easy, seamless navigation. ASUS has paired the N550JX?s impressive display with SonicMaster Premium, co-developed with Bang & Olufsen ICEpower� audio experts, for true surround sound. A quad-speaker array and external subwoofer combine for distinct vocals and a low bass that you can feel.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "asus-eee-pc-900ha-89-inch-netbook-black",
                AllowCustomerReviews = true,
                Price = 1500M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        Id = 1,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId = 5,
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                    }
                },
                ProductSpecificationAttributes =
                {
                     new ProductSpecificationAttribute
                    {
                        Id = 1,
                        ProductId = 5,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.6''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        ProductId = 5,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 3,
                        ProductId = 5,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "16 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 4,
                        ProductId = 5,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "1 TB").Id
                    }
                }
            };
            allProducts.Add(productAsusN551JK);
            productAsusN551JK.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asuspc_N551JK.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAsusN551JK.Name)).Id,
                Id = 1,
                ProductId = 5,
                _id = ObjectId.GenerateNewId().ToString(),
                DisplayOrder = 1,
            });
            _productRepository.Insert(productAsusN551JK);


            var productSamsungSeries = new Product
            {
                Id = 6,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Samsung Series 9 NP900X4C Premium Ultrabook",
                ShortDescription = "Samsung Series 9 NP900X4C-A06US 15-Inch Ultrabook (1.70 GHz Intel Core i5-3317U Processor, 8GB DDR3, 128GB SSD, Windows 8) Ash Black",
                FullDescription = "<p>Designed with mobility in mind, Samsung's durable, ultra premium, lightweight Series 9 laptop (model NP900X4C-A01US) offers mobile professionals and power users a sophisticated laptop equally suited for work and entertainment. Featuring a minimalist look that is both simple and sophisticated, its polished aluminum uni-body design offers an iconic look and feel that pushes the envelope with an edge just 0.58 inches thin. This Series 9 laptop also includes a brilliant 15-inch SuperBright Plus display with HD+ technology, 128 GB Solid State Drive (SSD), 8 GB of system memory, and up to 10 hours of battery life.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "hp-pavilion-artist-edition-dv2890nr-141-inch-laptop",
                AllowCustomerReviews = true,
                Price = 1590M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                //ShowOnHomePage = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        Id = 1,
                        ProductId = 6,
                        _id = ObjectId.GenerateNewId().ToString(),
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        Id = 1,
                        ProductId = 6,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        ProductId = 6,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 3,
                        ProductId = 6,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "8 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 4,
                        ProductId = 6,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "128 GB").Id
                    }
                }
            };
            allProducts.Add(productSamsungSeries);
            productSamsungSeries.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SamsungNP900X4C.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSamsungSeries.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 6,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productSamsungSeries);





            var productHpSpectre = new Product
            {
                Id = 7,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HP Spectre XT Pro UltraBook",
                ShortDescription = "HP Spectre XT Pro UltraBook / Intel Core i5-2467M / 13.3 / 4GB / 128GB / Windows 7 Professional / Laptop",
                FullDescription = "<p>Introducing HP ENVY Spectre XT, the Ultrabook designed for those who want style without sacrificing substance. It's sleek. It's thin. And with Intel. Corer i5 processor and premium materials, it's designed to go anywhere from the bistro to the boardroom, it's unlike anything you've ever seen from HP.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "hp-pavilion-elite-m9150f-desktop-pc",
                AllowCustomerReviews = true,
                Price = 1350M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        Id = 1,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "HP").Id,
                        DisplayOrder = 3,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        Id = 1,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "13.3''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 3,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "4 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 4,
                        ProductId = 7,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "128 GB").Id
                    }
                }
            };
            allProducts.Add(productHpSpectre);
            productHpSpectre.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HPSpectreXT_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpSpectre.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 7,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productHpSpectre.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HPSpectreXT_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpSpectre.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 7,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productHpSpectre);



            var productHpEnvy = new Product
            {
                Id = 8,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HP Envy 6-1180ca 15.6-Inch Sleekbook",
                ShortDescription = "HP ENVY 6-1202ea Ultrabook Beats Audio, 3rd generation Intel� CoreTM i7-3517U processor, 8GB RAM, 500GB HDD, Microsoft Windows 8, AMD Radeon HD 8750M (2 GB DDR3 dedicated)",
                FullDescription = "The UltrabookTM that's up for anything. Thin and light, the HP ENVY is the large screen UltrabookTM with Beats AudioTM. With a soft-touch base that makes it easy to grab and go, it's a laptop that's up for anything.<br><br><b>Features</b><br><br>- Windows 8 or other operating systems available<br><br><b>Top performance. Stylish design. Take notice.</b><br><br>- At just 19.8 mm (0.78 in) thin, the HP ENVY UltrabookTM is slim and light enough to take anywhere. It's the laptop that gets you noticed with the power to get it done.<br>- With an eye-catching metal design, it's a laptop that you want to carry with you. The soft-touch, slip-resistant base gives you the confidence to carry it with ease.<br><br><b>More entertaining. More gaming. More fun.</b><br><br>- Own the UltrabookTM with Beats AudioTM, dual speakers, a subwoofer, and an awesome display. Your music, movies and photo slideshows will always look and sound their best.<br>- Tons of video memory let you experience incredible gaming and multimedia without slowing down. Create and edit videos in a flash. And enjoy more of what you love to the fullest.<br>- The HP ENVY UltrabookTM is loaded with the ports you'd expect on a world-class laptop, but on a Sleekbook instead. Like HDMI, USB, RJ-45, and a headphone jack. You get all the right connections without compromising size.<br><br><b>Only from HP.</b><br><br>- Life heats up. That's why there's HP CoolSense technology, which automatically adjusts your notebook's temperature based on usage and conditions. It stays cool. You stay comfortable.<br>- With HP ProtectSmart, your notebook's data stays safe from accidental bumps and bruises. It senses motion and plans ahead, stopping your hard drive and protecting your entire digital life.<br>- Keep playing even in dimly lit rooms or on red eye flights. The optional backlit keyboard[1] is full-size so you don't compromise comfort. Backlit keyboard. Another bright idea.<br><br><b>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "hp-pavilion-g60-230us-160-inch-laptop",
                AllowCustomerReviews = true,
                Price = 1460M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "HP").Id,
                        DisplayOrder = 4,
                        Id = 1,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        Id = 1,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.6''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 3,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "8 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 4,
                        ProductId = 8,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "500 GB").Id
                    }
                }
            };
            allProducts.Add(productHpEnvy);
            productHpEnvy.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpEnvy6.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpEnvy.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 8,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productHpEnvy);


            var productLenovoThinkpad = new Product
            {
                Id = 9,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo Thinkpad X1 Carbon Laptop",
                ShortDescription = "Lenovo Thinkpad X1 Carbon Touch Intel Core i7 14 Ultrabook",
                FullDescription = "<p>The X1 Carbon brings a new level of quality to the ThinkPad legacy of high standards and innovation. It starts with the durable, carbon fiber-reinforced roll cage, making for the best Ultrabook construction available, and adds a host of other new features on top of the old favorites. Because for 20 years, we haven't stopped innovating. And you shouldn't stop benefiting from that.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "toshiba-satellite-a305-s6908-154-inch-laptop",
                AllowCustomerReviews = true,
                Price = 1360M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 9,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductSpecificationAttributes =
                {
                   new ProductSpecificationAttribute
                    {
                        Id = 1,
                        ProductId = 9,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "14.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        Id = 2,
                        ProductId = 9,
                        _id = ObjectId.GenerateNewId().ToString(),
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    }
                }
            };
            allProducts.Add(productLenovoThinkpad);
            productLenovoThinkpad.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LenovoThinkpad.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLenovoThinkpad.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 9,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productLenovoThinkpad);

            #endregion

            #region Software


            var productAdobePhotoshop = new Product
            {
                Id = 10,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adobe Photoshop CS4",
                ShortDescription = "Easily find and view all your photos",
                FullDescription = "<p>Adobe Photoshop CS4 software combines power and simplicity so you can make ordinary photos extraordinary; tell engaging stories in beautiful, personalized creations for print and web; and easily find and view all your photos. New Photoshop.com membership* works with Photoshop CS4 so you can protect your photos with automatic online backup and 2 GB of storage; view your photos anywhere you are; and share your photos in fun, interactive ways with invitation-only Online Albums.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "adobe-photoshop-elements-7",
                AllowCustomerReviews = true,
                Price = 75M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Software").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 10,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productAdobePhotoshop);
            productAdobePhotoshop.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AdobePhotoshop.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAdobePhotoshop.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 10,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productAdobePhotoshop);


            var productWindows8Pro = new Product
            {
                Id = 11,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Windows 8 Pro",
                ShortDescription = "Windows 8 is a Microsoft operating system that was released in 2012 as part of the company's Windows NT OS family. ",
                FullDescription = "<p>Windows 8 Pro is comparable to Windows 7 Professional and Ultimate and is targeted towards enthusiasts and business users; it includes all the features of Windows 8. Additional features include the ability to receive Remote Desktop connections, the ability to participate in a Windows Server domain, Encrypting File System, Hyper-V, and Virtual Hard Disk Booting, Group Policy as well as BitLocker and BitLocker To Go. Windows Media Center functionality is available only for Windows 8 Pro as a separate software package.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "corel-paint-shop-pro-photo-x2",
                AllowCustomerReviews = true,
                Price = 65M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Software").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 11,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productWindows8Pro);
            productWindows8Pro.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Windows8.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productWindows8Pro.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 11,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productWindows8Pro);





            var productSoundForge = new Product
            {
                Id = 12,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Sound Forge Pro 11",
                ShortDescription = "Advanced audio waveform editor.",
                FullDescription = "<p>Sound Forge? Pro is the application of choice for a generation of creative and prolific artists, producers, and editors. Record audio quickly on a rock-solid platform, address sophisticated audio processing tasks with surgical precision, and render top-notch master files with ease. New features include one-touch recording, metering for the new critical standards, more repair and restoration tools, and exclusive round-trip interoperability with SpectraLayers Pro. Taken together, these enhancements make this edition of Sound Forge Pro the deepest and most advanced audio editing platform available.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "major-league-baseball-2k9",
                AllowCustomerReviews = true,
                Price = 54.99M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Software").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 12,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productSoundForge);
            productSoundForge.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SoundForge.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSoundForge.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 12,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productSoundForge);


            #endregion

            #region Camera, Photo


            //this one is a grouped product with two associated ones
            var productNikonD5500DSLR = new Product
            {
                Id = 13,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.GroupedProduct,
                VisibleIndividually = true,
                Name = "Nikon D5500 DSLR",
                ShortDescription = "Slim, lightweight Nikon D5500 packs a vari-angle touchscreen",
                FullDescription = "Nikon has announced its latest DSLR, the D5500. A lightweight, compact DX-format camera with a 24.2MP sensor, it?s the first of its type to offer a vari-angle touchscreen. The D5500 replaces the D5300 in Nikon?s range, and while it offers much the same features the company says it?s a much slimmer and lighter prospect. There?s a deep grip for easier handling and built-in Wi-Fi that lets you transfer and share shots via your phone or tablet.",
                ProductTemplateId = productTemplateGrouped.Id,
                //SeName = "canon-digital-slr-camera",
                AllowCustomerReviews = true,
                Published = true,
                Price = 670M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Camera & photo").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 13,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNikonD5500DSLR);
            productNikonD5500DSLR.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNikonD5500DSLR.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 13,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productNikonD5500DSLR.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNikonD5500DSLR.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 13,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNikonD5500DSLR);
            var productNikonD5500DSLR_associated_1 = new Product
            {
                Id = 14,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false, //hide this products
                ParentGroupedProductId = productNikonD5500DSLR.Id,
                Name = "Nikon D5500 DSLR - Black",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "canon-digital-slr-camera-black",
                AllowCustomerReviews = true,
                Published = true,
                Price = 670M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allProducts.Add(productNikonD5500DSLR_associated_1);
            productNikonD5500DSLR_associated_1.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_black.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital SLR Camera - Black")).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 14,
                _id = ObjectId.GenerateNewId().ToString(),

            });
            _productRepository.Insert(productNikonD5500DSLR_associated_1);
            var productNikonD5500DSLR_associated_2 = new Product
            {
                Id = 15,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false, //hide this products
                ParentGroupedProductId = productNikonD5500DSLR.Id,
                Name = "Nikon D5500 DSLR - Red",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "canon-digital-slr-camera-silver",
                AllowCustomerReviews = true,
                Published = true,
                Price = 630M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allProducts.Add(productNikonD5500DSLR_associated_2);
            productNikonD5500DSLR_associated_2.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_red.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital SLR Camera - Silver")).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 15,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNikonD5500DSLR_associated_2);



            var productLeica = new Product
            {
                Id = 16,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Leica T Mirrorless Digital Camera",
                ShortDescription = "Leica T (Typ 701) Silver",
                FullDescription = "<p>The new Leica T offers a minimalist design that's crafted from a single block of aluminum.  Made in Germany and assembled by hand, this 16.3 effective mega pixel camera is easy to use.  With a massive 3.7 TFT LCD intuitive touch screen control, the user is able to configure and save their own menu system.  The Leica T has outstanding image quality and also has 16GB of built in memory.  This is Leica's first system camera to use Wi-Fi.  Add the T-App to your portable iOS device and be able to transfer and share your images (free download from the Apple App Store)</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "canon-vixia-hf100-camcorder",
                AllowCustomerReviews = true,
                Price = 530M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Camera & photo").Id,
                        DisplayOrder = 3,
                        Id = 1,
                        ProductId = 16,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productLeica);
            productLeica.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeicaT.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLeica.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 16,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productLeica);


            var productAppleICam = new Product
            {
                Id = 17,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Apple iCam",
                ShortDescription = "Photography becomes smart",
                FullDescription = "<p>A few months ago we featured the amazing WVIL camera, by many considered the future of digital photography. This is another very good looking concept, iCam is the vision of Italian designer Antonio DeRosa, the idea is to have a device that attaches to the iPhone 5, which then allows the user to have a camera with interchangeable lenses. The device would also feature a front-touch screen and a projector. Would be great if apple picked up on this and made it reality.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "panasonic-hdc-sdt750k-high-definition-3d-camcorder",
                AllowCustomerReviews = true,
                Price = 1300M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Camera & photo").Id,
                        DisplayOrder = 2,
                        Id = 1,
                        ProductId = 17,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Apple").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 17,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productAppleICam);
            productAppleICam.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_iCam.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleICam.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 17,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productAppleICam);

            #endregion

            #region Cell Phone

            var productHtcOne = new Product
            {
                Id = 18,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HTC One M8 Android L 5.0 Lollipop",
                ShortDescription = "HTC - One (M8) 4G LTE Cell Phone with 32GB Memory - Gunmetal (Sprint)",
                FullDescription = "<p><b>HTC One (M8) Cell Phone for Sprint:</b> With its brushed-metal design and wrap-around unibody frame, the HTC One (M8) is designed to fit beautifully in your hand. It's fun to use with amped up sound and a large Full HD touch screen, and intuitive gesture controls make it seem like your phone almost knows what you need before you do. <br><br>Sprint Easy Pay option available in store.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 245M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                ShowOnHomePage = true,
                MarkAsNew = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Cell phones").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 18,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productHtcOne);
            productHtcOne.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_M8.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOne.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 18,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productHtcOne);


            var productHtcOneMini = new Product
            {
                Id = 19,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HTC One Mini Blue",
                ShortDescription = "HTC One and HTC One Mini now available in bright blue hue",
                FullDescription = "<p>HTC One mini smartphone with 4.30-inch 720x1280 display powered by 1.4GHz processor alongside 1GB RAM and 4-Ultrapixel rear camera.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "samsung-rugby-a837-phone-black-att",
                AllowCustomerReviews = true,
                Price = 100M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Cell phones").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 19,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productHtcOneMini);
            productHtcOneMini.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_Mini_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOneMini.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 19,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productHtcOneMini.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_Mini_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOneMini.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 19,
                _id = ObjectId.GenerateNewId().ToString(),

            });
            _productRepository.Insert(productHtcOneMini);


            var productNokiaLumia = new Product
            {
                Id = 20,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nokia Lumia 1020",
                ShortDescription = "Nokia Lumia 1020 4G Cell Phone (Unlocked)",
                FullDescription = "<p>Capture special moments for friends and family with this Nokia Lumia 1020 32GB WHITE cell phone that features an easy-to-use 41.0MP rear-facing camera and a 1.2MP front-facing camera. The AMOLED touch screen offers 768 x 1280 resolution for crisp visuals.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "sony-dcr-sr85-1mp-60gb-hard-drive-handycam-camcorder",
                AllowCustomerReviews = true,
                Price = 349M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Cell phones").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 20,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNokiaLumia);
            productNokiaLumia.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Lumia1020.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNokiaLumia.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 20,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNokiaLumia);


            #endregion

            #region Others



            var productBeatsPill = new Product
            {
                Id = 21,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Beats Pill 2.0 Wireless Speaker",
                ShortDescription = "<b>Pill 2.0 Portable Bluetooth Speaker (1-Piece):</b> Watch your favorite movies and listen to music with striking sound quality. This lightweight, portable speaker is easy to take with you as you travel to any destination, keeping you entertained wherever you are. ",
                FullDescription = "<p<ul><li>Pair and play with your Bluetooth� device with 30 foot range</li><li>Built-in speakerphone</li><li>7 hour rechargeable battery</li><li>Power your other devices with USB charge out</li><li>Tap two Beats Pills? together for twice the sound with Beats Bond?</li></ul></p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "acer-aspire-one-89-mini-notebook-case-black",
                AllowCustomerReviews = true,
                Price = 79.99M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                MarkAsNew = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                TierPrices =
                {
                    new TierPrice
                    {
                        Quantity = 2,
                        Price = 19,
                        Id = 1,
                        ProductId = 21,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 5,
                        Price = 17,
                        Id = 2,
                        ProductId = 21,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 15,
                        Id = 3,
                        ProductId = 21,
                        _id = ObjectId.GenerateNewId().ToString(),

                    }
                },
                HasTierPrices = true,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Others").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 21,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productBeatsPill);
            productBeatsPill.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PillBeats_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBeatsPill.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 21,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productBeatsPill.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PillBeats_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBeatsPill.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 21,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productBeatsPill);


            var productUniversalTabletCover = new Product
            {
                Id = 22,
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Universal 7-8 Inch Tablet Cover",
                ShortDescription = "Universal protection for 7-inch & 8-inch tablets",
                FullDescription = "<p>Made of durable polyurethane, our Universal Cover is slim, lightweight, and strong, with protective corners that stretch to hold most 7 and 8-inch tablets securely. This tough case helps protects your tablet from bumps, scuffs, and dings.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "apc-back-ups-rs-800va-ups-800-va-ups-battery-lead-acid-br800blk",
                AllowCustomerReviews = true,
                Price = 39M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Others").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 22,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productUniversalTabletCover);
            productUniversalTabletCover.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_TabletCover.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productUniversalTabletCover.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 22,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productUniversalTabletCover);


            var productPortableSoundSpeakers = new Product
            {
                Id = 23,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Portable Sound Speakers",
                ShortDescription = "Universall portable sound speakers",
                FullDescription = "<p>Your phone cut the cord, now it's time for you to set your music free and buy a Bluetooth speaker. Thankfully, there's one suited for everyone out there.</p><p>Some Bluetooth speakers excel at packing in as much functionality as the unit can handle while keeping the price down. Other speakers shuck excess functionality in favor of premium build materials instead. Whatever path you choose to go down, you'll be greeted with many options to suit your personal tastes.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "microsoft-bluetooth-notebook-mouse-5000-macwindows",
                AllowCustomerReviews = true,
                Price = 37M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Others").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 23,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productPortableSoundSpeakers);
            productPortableSoundSpeakers.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Speakers.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPortableSoundSpeakers.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 23,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productPortableSoundSpeakers);


            #endregion

            #region Shoes


            var productNikeFloral = new Product
            {
                Id = 24,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Floral Roshe Customized Running Shoes",
                ShortDescription = "When you ran across these shoes, you will immediately fell in love and needed a pair of these customized beauties.",
                FullDescription = "<p>Each Rosh Run is personalized and exclusive, handmade in our workshop Custom. Run Your Rosh creations born from the hand of an artist specialized in sneakers, more than 10 years of experience.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "adidas-womens-supernova-csh-7-running-shoe",
                AllowCustomerReviews = true,
                Price = 40M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        Id = 1,
                        ProductId = 24,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Size").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "8",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "9",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "10",
                                DisplayOrder = 3,
                                Id = 3,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "11",
                                DisplayOrder = 4,
                                Id = 4,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 2,
                        ProductId = 24,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Color").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "White/Blue",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "White/Black",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 24,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 2,
                            },
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Shoes").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 24,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                        Id = 1,
                        ProductId = 24,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNikeFloral);
            productNikeFloral.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeFloralShoe_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeFloral.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 24,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productNikeFloral.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeFloralShoe_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeFloral.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 24,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNikeFloral);


            var productAdidas = new Product
            {
                Id = 25,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "adidas Consortium Campus 80s Running Shoes",
                ShortDescription = "adidas Consortium Campus 80s Primeknit Light Maroon/Running Shoes",
                FullDescription = "<p>One of three colorways of the adidas Consortium Campus 80s Primeknit set to drop alongside each other. This pair comes in light maroon and running white. Featuring a maroon-based primeknit upper with white accents. A limited release, look out for these at select adidas Consortium accounts worldwide.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "etnies-mens-digit-sneaker",
                AllowCustomerReviews = true,
                Price = 27.56M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                //ShowOnHomePage = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        Id = 1,
                        ProductId = 25,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Size").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "8",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "9",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "10",
                                DisplayOrder = 3,
                                Id = 3,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "11",
                                DisplayOrder = 4,
                                Id = 4,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 2
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        Id = 3,
                        ProductId = 25,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Color").Id,
                        AttributeControlType = AttributeControlType.ColorSquares,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Red",
                                IsPreSelected = true,
                                ColorSquaresRgb = "#663030",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 3
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Blue",
                                ColorSquaresRgb = "#363656",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 3,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Silver",
                                ColorSquaresRgb = "#c5c5d5",
                                DisplayOrder = 3,
                                 Id = 3,
                                ProductId = 25,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 3,
                           }
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Shoes").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 25,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productAdidas);
            productAdidas.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 25,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productAdidas.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name)).Id,
                DisplayOrder = 2,
                Id = 2,
                ProductId = 25,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productAdidas.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_3.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name)).Id,
                DisplayOrder = 3,
                Id = 3,
                ProductId = 25,
                _id = ObjectId.GenerateNewId().ToString(),
            });


            _productRepository.Insert(productAdidas);

            var productAttribute = EngineContext.Current.Resolve<IProductAttributeService>().GetAllProductAttributes().Where(x => x.Name == "Color").FirstOrDefault();

            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Red").First().PictureId = productAdidas.ProductPictures.ElementAt(0).PictureId;
            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Blue").First().PictureId = productAdidas.ProductPictures.ElementAt(1).PictureId;
            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Silver").First().PictureId = productAdidas.ProductPictures.ElementAt(2).PictureId;
            _productRepository.Update(productAdidas);




            var productNikeZoom = new Product
            {
                Id = 26,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike SB Zoom Stefan Janoski \"Medium Mint\"",
                ShortDescription = "Nike SB Zoom Stefan Janoski Dark Grey Medium Mint Teal ...",
                FullDescription = "The newly Nike SB Zoom Stefan Janoski gets hit with a \"Medium Mint\" accents that sits atop a Dark Grey suede. Expected to drop in October.",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "v-blue-juniors-cuffed-denim-short-with-rhinestones",
                AllowCustomerReviews = true,
                Price = 30M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Shoes").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 26,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                        Id = 1,
                        ProductId = 26,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNikeZoom);
            productNikeZoom.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeZoom.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeZoom.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 26,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNikeZoom);


            #endregion

            #region Clothing

            var productNikeTailwind = new Product
            {
                Id = 27,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Tailwind Loose Short-Sleeve Running Shirt",
                ShortDescription = "",
                FullDescription = "<p>Boost your adrenaline with the Nike� Women's Tailwind Running Shirt. The lightweight, slouchy fit is great for layering, and moisture-wicking fabrics keep you feeling at your best. This tee has a notched hem for an enhanced range of motion, while flat seams with reinforcement tape lessen discomfort and irritation over longer distances. Put your keys and card in the side zip pocket and take off in your Nike� running t-shirt.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "50s-rockabilly-polka-dot-top-jr-plus-size",
                AllowCustomerReviews = true,
                Published = true,
                Price = 15M,
                IsShipEnabled = true,
                Weight = 1,
                Length = 2,
                Width = 3,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        Id = 1,
                        ProductId = 27,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Size").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Small",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "1X",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },

                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2X",
                                DisplayOrder = 3,
                                Id = 3,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "3X",
                                DisplayOrder = 4,
                                Id = 4,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "4X",
                                DisplayOrder = 5,
                                Id = 5,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "5X",
                                DisplayOrder = 6,
                                Id = 6,
                                ProductId = 27,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            }
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 27,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                        Id = 1,
                        ProductId = 27,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNikeTailwind);
            productNikeTailwind.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeShirt.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeTailwind.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 27,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNikeTailwind);




            var productOversizedWomenTShirt = new Product
            {
                Id = 28,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Oversized Women T-Shirt",
                ShortDescription = "",
                FullDescription = "<p>This oversized women t-Shirt needs minimum ironing. It is a great product at a great value!</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "arrow-mens-wrinkle-free-pinpoint-solid-long-sleeve",
                AllowCustomerReviews = true,
                Price = 24M,
                IsShipEnabled = true,
                Weight = 4,
                Length = 3,
                Width = 3,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                TierPrices =
                {
                    new TierPrice
                    {
                        Quantity = 3,
                        Price = 21,
                        Id = 1,
                        ProductId = 28,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 7,
                        Price = 19,
                        Id = 2,
                        ProductId = 28,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 16,
                        Id = 3,
                        ProductId = 28,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                HasTierPrices = true,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 28,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productOversizedWomenTShirt);
            productOversizedWomenTShirt.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_WomenTShirt.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productOversizedWomenTShirt.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 28,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productOversizedWomenTShirt);


            var productCustomTShirt = new Product
            {
                Id = 29,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Custom T-Shirt",
                ShortDescription = "T-Shirt - Add Your Content",
                FullDescription = "<p>Comfort comes in all shapes and forms, yet this tee out does it all. Rising above the rest, our classic cotton crew provides the simple practicality you need to make it through the day. Tag-free, relaxed fit wears well under dress shirts or stands alone in laid-back style. Reinforced collar and lightweight feel give way to long-lasting shape and breathability. One less thing to worry about, rely on this tee to provide comfort and ease with every wear.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "custom-t-shirt",
                AllowCustomerReviews = true,
                Price = 15M,
                IsShipEnabled = true,
                Weight = 4,
                Length = 3,
                Width = 3,
                Height = 3,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Custom Text").Id,
                        TextPrompt = "Enter your text:",
                        AttributeControlType = AttributeControlType.TextBox,
                        IsRequired = true,
                        Id = 1,
                        ProductId = 29,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 29,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productCustomTShirt);
            productCustomTShirt.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CustomTShirt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCustomTShirt.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 29,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productCustomTShirt);


            var productLeviJeans = new Product
            {
                Id = 30,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Levi's 511 Jeans",
                ShortDescription = "Levi's Faded Black 511 Jeans ",
                FullDescription = "<p>Between a skinny and straight fit, our 511&trade; slim fit jeans are cut close without being too restricting. Slim throughout the thigh and leg opening for a long and lean look.</p><ul><li>Slouch1y at top; sits below the waist</li><li>Slim through the leg, close at the thigh and straight to the ankle</li><li>Stretch for added comfort</li><li>Classic five-pocket styling</li><li>99% Cotton, 1% Spandex, 11.2 oz. - Imported</li></ul>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "levis-skinny-511-jeans",
                AllowCustomerReviews = true,
                Price = 43.5M,
                OldPrice = 55M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                TierPrices =
                {
                    new TierPrice
                    {
                        Quantity = 3,
                        Price = 40,
                        Id = 1,
                        ProductId = 30,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 6,
                        Price = 38,
                        Id = 2,
                        ProductId = 30,
                        _id = ObjectId.GenerateNewId().ToString(),
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 35,
                        Id = 3,
                        ProductId = 30,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                },
                HasTierPrices = true,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 30,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productLeviJeans);

            productLeviJeans.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 30,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            productLeviJeans.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name)).Id,
                Id = 2,
                ProductId = 30,
                _id = ObjectId.GenerateNewId().ToString(),
                DisplayOrder = 2,
            });
            _productRepository.Insert(productLeviJeans);


            #endregion

            #region Accessories


            var productObeyHat = new Product
            {
                Id = 31,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Obey Propaganda Hat",
                ShortDescription = "",
                FullDescription = "<p>Printed poplin 5 panel camp hat with debossed leather patch and web closure</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "indiana-jones-shapeable-wool-hat",
                AllowCustomerReviews = true,
                Price = 30M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
                        Id = 1,
                        ProductId = 31,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Size").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Small",
                                DisplayOrder = 1,
                                Id = 1,
                                ProductId = 31,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Medium",
                                DisplayOrder = 2,
                                Id = 2,
                                ProductId = 31,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Large",
                                DisplayOrder = 3,
                                Id = 3,
                                ProductId = 31,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "X-Large",
                                DisplayOrder = 4,
                                Id = 4,
                                ProductId = 31,
                                _id = ObjectId.GenerateNewId().ToString(),
                                ProductAttributeMappingId = 1,
                            }
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Accessories").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 31,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productObeyHat);
            productObeyHat.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_hat.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productObeyHat.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 31,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productObeyHat);



            var productBelt = new Product
            {
                Id = 32,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Reversible Horseferry Check Belt",
                ShortDescription = "Reversible belt in Horseferry check with smooth leather trim",
                FullDescription = "<p>Reversible belt in Horseferry check with smooth leather trim</p><p>Leather lining, polished metal buckle</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "nike-golf-casual-belt",
                AllowCustomerReviews = true,
                Price = 45M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Accessories").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 32,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productBelt);
            productBelt.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Belt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBelt.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 32,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productBelt);



            var productSunglasses = new Product
            {
                Id = 33,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Ray Ban Aviator Sunglasses",
                ShortDescription = "Aviator sunglasses are one of the first widely popularized styles of modern day sunwear.",
                FullDescription = "<p>Since 1937, Ray-Ban can genuinely claim the title as the world's leading sunglasses and optical eyewear brand. Combining the best of fashion and sports performance, the Ray-Ban line of Sunglasses delivers a truly classic style that will have you looking great today and for years to come.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "ray-ban-aviator-sunglasses-rb-3025",
                AllowCustomerReviews = true,
                Price = 25M,
                IsShipEnabled = true,
                Weight = 7,
                Length = 7,
                Width = 7,
                Height = 7,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Apparel").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Accessories").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 33,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productSunglasses);
            productSunglasses.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Sunglasses.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productSunglasses.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 33,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productSunglasses);

            #endregion

            #region Digital Downloads


            var downloadNightVision1 = new Download
            {
                Id = 1,
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_NightVision_1.zip"),
                Extension = ".zip",
                Filename = "Night_Vision_1",
                IsNew = true,
            };
            downloadService.InsertDownload(downloadNightVision1);
            var downloadNightVision2 = new Download
            {
                Id = 2,
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_NightVision_2.txt"),
                Extension = ".txt",
                Filename = "Night_Vision_1",
                IsNew = true,
            };
            downloadService.InsertDownload(downloadNightVision2);
            var productNightVision = new Product
            {
                Id = 34,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Night Visions",
                ShortDescription = "Night Visions is the debut studio album by American rock band Imagine Dragons.",
                FullDescription = "<p>Original Release Date: September 4, 2012</p><p>Release Date: September 4, 2012</p><p>Genre - Alternative rock, indie rock, electronic rock</p><p>Label - Interscope/KIDinaKORNER</p><p>Copyright: (C) 2011 Interscope Records</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "poker-face",
                AllowCustomerReviews = true,
                Price = 2.8M,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Downloadable Products").Id,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                IsDownload = true,
                DownloadId = downloadNightVision1.Id,
                DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
                UnlimitedDownloads = true,
                HasUserAgreement = false,
                HasSampleDownload = true,
                SampleDownloadId = downloadNightVision2.Id,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Digital downloads").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 34,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productNightVision);
            productNightVision.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NightVisions.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNightVision.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 34,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productNightVision);



            var downloadIfYouWait1 = new Download
            {
                Id = 3,
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_IfYouWait_1.zip"),
                Extension = ".zip",
                Filename = "If_You_Wait_1",
                IsNew = true,
            };
            downloadService.InsertDownload(downloadIfYouWait1);
            var downloadIfYouWait2 = new Download
            {
                Id = 4,
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_IfYouWait_2.txt"),
                Extension = ".txt",
                Filename = "If_You_Wait_1",
                IsNew = true,
            };
            downloadService.InsertDownload(downloadIfYouWait2);
            var productIfYouWait = new Product
            {
                Id = 35,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "If You Wait",
                ShortDescription = "If You Wait is the debut studio album by English indie pop band London Grammar",
                FullDescription = "<p>Original Release Date: September 6, 2013</p><p>Genre - Electronica, dream pop downtempo, pop</p><p>Label - Metal & Dust/Ministry of Sound</p><p>Producer - Tim Bran, Roy Kerr London, Grammar</p><p>Length - 43:22</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "single-ladies-put-a-ring-on-it",
                AllowCustomerReviews = true,
                Price = 3M,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Downloadable Products").Id,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                IsDownload = true,
                DownloadId = downloadIfYouWait1.Id,
                DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
                UnlimitedDownloads = true,
                HasUserAgreement = false,
                HasSampleDownload = true,
                SampleDownloadId = downloadIfYouWait2.Id,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Digital downloads").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 35,
                        _id = ObjectId.GenerateNewId().ToString(),

                    }
                }
            };
            allProducts.Add(productIfYouWait);

            productIfYouWait.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_IfYouWait.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productIfYouWait.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 35,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productIfYouWait);


            var downloadScienceAndFaith = new Download
            {
                Id = 5,
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_ScienceAndFaith_1.zip"),
                Extension = ".zip",
                Filename = "Science_And_Faith",
                IsNew = true,
            };
            downloadService.InsertDownload(downloadScienceAndFaith);
            var productScienceAndFaith = new Product
            {
                Id = 36,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Science & Faith",
                ShortDescription = "Science & Faith is the second studio album by Irish pop rock band The Script.",
                FullDescription = "<p># Original Release Date: September 10, 2010<br /># Label: RCA, Epic/Phonogenic(America)<br /># Copyright: 2010 RCA Records.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "the-battle-of-los-angeles",
                AllowCustomerReviews = true,
                Price = 3M,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Downloadable Products").Id,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                IsDownload = true,
                DownloadId = downloadScienceAndFaith.Id,
                DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
                UnlimitedDownloads = true,
                HasUserAgreement = false,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Digital downloads").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 36,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productScienceAndFaith);
            productScienceAndFaith.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ScienceAndFaith.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productScienceAndFaith.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 36,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productScienceAndFaith);



            #endregion

            #region Books

            var productFahrenheit = new Product
            {
                Id = 37,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Fahrenheit 451 by Ray Bradbury",
                ShortDescription = "Fahrenheit 451 is a dystopian novel by Ray Bradbury published in 1953. It is regarded as one of his best works.",
                FullDescription = "<p>The novel presents a future American society where books are outlawed and firemen burn any that are found. The title refers to the temperature that Bradbury understood to be the autoignition point of paper.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "best-grilling-recipes",
                AllowCustomerReviews = true,
                Price = 27M,
                OldPrice = 30M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Books").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Books").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 37,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productFahrenheit);
            productFahrenheit.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Fahrenheit451.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productFahrenheit.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 37,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productFahrenheit);



            var productFirstPrizePies = new Product
            {
                Id = 38,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "First Prize Pies",
                ShortDescription = "Allison Kave made pies as a hobby, until one day her boyfriend convinced her to enter a Brooklyn pie-making contest. She won. In fact, her pies were such a hit that she turned pro.",
                FullDescription = "<p>First Prize Pies, a boutique, made-to-order pie business that originated on New York's Lower East Side, has become synonymous with tempting and unusual confections. For the home baker who is passionate about seasonal ingredients and loves a creative approach to recipes, First Prize Pies serves up 52 weeks of seasonal and eclectic pastries in an interesting pie-a-week format. Clear instructions, technical tips and creative encouragement guide novice bakers as well as pie mavens. With its nostalgia-evoking photos of homemade pies fresh out of the oven, First Prize Pies will be as giftable as it is practical.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "eatingwell-in-season",
                AllowCustomerReviews = true,
                Price = 51M,
                OldPrice = 67M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Books").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Books").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 38,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productFirstPrizePies);
            productFirstPrizePies.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_FirstPrizePies.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productFirstPrizePies.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 38,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productFirstPrizePies);







            var productPrideAndPrejudice = new Product
            {
                Id = 39,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Pride and Prejudice",
                ShortDescription = "Pride and Prejudice is a novel of manners by Jane Austen, first published in 1813.",
                FullDescription = "<p>Set in England in the early 19th century, Pride and Prejudice tells the story of Mr and Mrs Bennet's five unmarried daughters after the rich and eligible Mr Bingley and his status-conscious friend, Mr Darcy, have moved into their neighbourhood. While Bingley takes an immediate liking to the eldest Bennet daughter, Jane, Darcy has difficulty adapting to local society and repeatedly clashes with the second-eldest Bennet daughter, Elizabeth.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "the-best-skillet-recipes",
                AllowCustomerReviews = true,
                Price = 24M,
                OldPrice = 35M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Books").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Books").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 39,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productPrideAndPrejudice);
            productPrideAndPrejudice.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PrideAndPrejudice.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPrideAndPrejudice.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 39,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productPrideAndPrejudice);



            #endregion

            #region Jewelry



            var productElegantGemstoneNecklace = new Product
            {
                Id = 40,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Elegant Gemstone Necklace",
                ShortDescription = "Classic and elegant gemstone necklace now available in our store",
                FullDescription = "<p>For those who like jewelry, creating their ownelegant jewelry from gemstone beads provides an economical way to incorporate genuine gemstones into your jewelry wardrobe. Manufacturers create beads from all kinds of precious gemstones and semi-precious gemstones, which are available in bead shops, craft stores, and online marketplaces.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "diamond-pave-earrings",
                AllowCustomerReviews = true,
                Price = 569M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Jewelry").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                MarkAsNew = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Jewelry").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 40,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productElegantGemstoneNecklace);
            productElegantGemstoneNecklace.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_GemstoneNecklaces.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productElegantGemstoneNecklace.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 40,
                _id = ObjectId.GenerateNewId().ToString(),

            });
            _productRepository.Insert(productElegantGemstoneNecklace);


            var productFlowerGirlBracelet = new Product
            {
                Id = 41,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Flower Girl Bracelet",
                ShortDescription = "Personalised Flower Braceled",
                FullDescription = "<p>This is a great gift for your flower girl to wear on your wedding day. A delicate bracelet that is made with silver plated soldered cable chain, gives this bracelet a dainty look for young wrist. A Swarovski heart, shown in Rose, hangs off a silver plated flower. Hanging alongside the heart is a silver plated heart charm with Flower Girl engraved on both sides. This is a great style for the younger flower girl.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "diamond-tennis-bracelet",
                AllowCustomerReviews = true,
                Price = 360M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Jewelry").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Jewelry").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 41,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productFlowerGirlBracelet);
            productFlowerGirlBracelet.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_FlowerBracelet.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productFlowerGirlBracelet.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 41,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productFlowerGirlBracelet);


            var productEngagementRing = new Product
            {
                Id = 42,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Vintage Style Engagement Ring",
                ShortDescription = "1.24 Carat (ctw) in 14K White Gold (Certified)",
                FullDescription = "<p>Dazzle her with this gleaming 14 karat white gold vintage proposal. A ravishing collection of 11 decadent diamonds come together to invigorate a superbly ornate gold shank. Total diamond weight on this antique style engagement ring equals 1 1/4 carat (ctw). Item includes diamond certificate.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "vintage-style-three-stone-diamond-engagement-ring",
                AllowCustomerReviews = true,
                Price = 2100M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Jewelry").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Jewelry").Id,
                        DisplayOrder = 1,
                        Id = 1,
                        ProductId = 42,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(productEngagementRing);
            productEngagementRing.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_EngagementRing_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productEngagementRing.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 42,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(productEngagementRing);



            #endregion

            #region Gift Cards


            var product25GiftCard = new Product
            {
                Id = 43,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$25 Virtual Gift Card",
                ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "25-virtual-gift-card",
                AllowCustomerReviews = true,
                Price = 25M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                ShowOnHomePage = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Gift Cards").Id,
                        DisplayOrder = 2,
                        Id = 1,
                        ProductId = 43,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(product25GiftCard);
            product25GiftCard.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_25giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product25GiftCard.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 43,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(product25GiftCard);


            var product50GiftCard = new Product
            {
                Id = 44,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$50 Physical Gift Card",
                ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "50-physical-gift-card",
                AllowCustomerReviews = true,
                Price = 50M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Physical,
                IsShipEnabled = true,
                IsFreeShipping = true,
                DeliveryDateId = deliveryDate.Id,
                Weight = 1,
                Length = 1,
                Width = 1,
                Height = 1,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                MarkAsNew = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Gift Cards").Id,
                        DisplayOrder = 3,
                        Id = 1,
                        ProductId = 44,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(product50GiftCard);
            product50GiftCard.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_50giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product50GiftCard.Name)).Id,
                Id = 1,
                ProductId = 44,
                _id = ObjectId.GenerateNewId().ToString(),
                DisplayOrder = 1,
            });
            _productRepository.Insert(product50GiftCard);


            var product100GiftCard = new Product
            {
                Id = 45,
                _id = ObjectId.GenerateNewId().ToString(),
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$100 Physical Gift Card",
                ShortDescription = "$100 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                //SeName = "100-physical-gift-card",
                AllowCustomerReviews = true,
                Price = 100M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Physical,
                IsShipEnabled = true,
                DeliveryDateId = deliveryDate.Id,
                Weight = 1,
                Length = 1,
                Width = 1,
                Height = 1,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Gift Cards").Id,
                        DisplayOrder = 4,
                        Id = 1,
                        ProductId = 45,
                        _id = ObjectId.GenerateNewId().ToString(),
                    }
                }
            };
            allProducts.Add(product100GiftCard);
            product100GiftCard.ProductPictures.Add(new ProductPicture
            {
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_100giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product100GiftCard.Name)).Id,
                DisplayOrder = 1,
                Id = 1,
                ProductId = 45,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            _productRepository.Insert(product100GiftCard);

            #endregion

            //_productRepository.Insert(allProducts);
            //search engine names
            foreach (var product in allProducts)
            {
                product.SeName = product.ValidateSeName("", product.Name, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = product.Id,
                    EntityName = "Product",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = product.SeName,
                });

                _productRepository.Update(product);
            }


            #region Related Products

            //related products

            //var relatedProducts = new List<RelatedProduct>
            //{
            productFlowerGirlBracelet.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFlowerGirlBracelet.Id,
                    ProductId2 = productEngagementRing.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productFlowerGirlBracelet.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFlowerGirlBracelet.Id,
                    ProductId2 = productElegantGemstoneNecklace.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productEngagementRing.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productEngagementRing.Id,
                    ProductId2 = productFlowerGirlBracelet.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productEngagementRing.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productEngagementRing.Id,
                    ProductId2 = productElegantGemstoneNecklace.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productElegantGemstoneNecklace.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productElegantGemstoneNecklace.Id,
                    ProductId2 = productFlowerGirlBracelet.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productElegantGemstoneNecklace.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productElegantGemstoneNecklace.Id,
                    ProductId2 = productEngagementRing.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productIfYouWait.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productIfYouWait.Id,
                    ProductId2 = productNightVision.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productIfYouWait.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productIfYouWait.Id,
                    ProductId2 = productScienceAndFaith.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productNightVision.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNightVision.Id,
                    ProductId2 = productIfYouWait.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productNightVision.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNightVision.Id,
                    ProductId2 = productScienceAndFaith.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productPrideAndPrejudice.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productPrideAndPrejudice.Id,
                    ProductId2 = productFirstPrizePies.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productPrideAndPrejudice.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productPrideAndPrejudice.Id,
                    ProductId2 = productFahrenheit.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productFirstPrizePies.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFirstPrizePies.Id,
                    ProductId2 = productPrideAndPrejudice.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productFirstPrizePies.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFirstPrizePies.Id,
                    ProductId2 = productFahrenheit.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productFahrenheit.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFahrenheit.Id,
                    ProductId2 = productFirstPrizePies.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productFahrenheit.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productFahrenheit.Id,
                    ProductId2 = productPrideAndPrejudice.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productSamsungSeries.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productHpSpectre.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productAsusN551JK.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productSamsungSeries.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoThinkpad.RelatedProducts.Add(
                 new RelatedProduct
                 {
                     ProductId1 = productLenovoThinkpad.Id,
                     ProductId2 = productHpEnvy.Id,
                     Id = 4,
                     _id = ObjectId.GenerateNewId().ToString(),
                 });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productSamsungSeries.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productAsusN551JK.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAppleMacBookPro.RelatedProducts.Add(
                 new RelatedProduct
                 {
                     ProductId1 = productAppleMacBookPro.Id,
                     ProductId2 = productHpSpectre.Id,
                     Id = 4,
                     _id = ObjectId.GenerateNewId().ToString(),
                 });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productSamsungSeries.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productAsusN551JK.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpSpectre.RelatedProducts.Add(
                 new RelatedProduct
                 {
                     ProductId1 = productHpSpectre.Id,
                     ProductId2 = productHpEnvy.Id,
                     Id = 4,
                     _id = ObjectId.GenerateNewId().ToString(),
                 });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productAsusN551JK.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productHpSpectre.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productSamsungSeries.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productAsusN551JK.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productHpEnvy.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productSamsungSeries.RelatedProducts.Add(
                 new RelatedProduct
                 {
                     ProductId1 = productSamsungSeries.Id,
                     ProductId2 = productHpSpectre.Id,
                     Id = 4,
                     _id = ObjectId.GenerateNewId().ToString(),
                 });
            productLeica.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productHtcOneMini.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productNikonD5500DSLR.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productAppleICam.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productNokiaLumia.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productHtcOne.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productHtcOneMini.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHtcOne.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productNokiaLumia.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productHtcOne.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productBeatsPill.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHtcOne.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productHtcOne.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productNokiaLumia.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productBeatsPill.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productHtcOne.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productHtcOneMini.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productBeatsPill.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productLeviJeans.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeFloral.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeZoom.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productAdidas.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeTailwind.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productAdidas.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeFloral.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeZoom.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeTailwind.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productLeviJeans.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productNikeTailwind.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productOversizedWomenTShirt.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productObeyHat.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productBuildComputer.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productLenovoIdeaCentre.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productBuildComputer.Id,
                    Id = 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productDigitalStorm.Id,
                    Id = 2,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                    Id = 3,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct
                {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                    Id = 4,
                    _id = ObjectId.GenerateNewId().ToString(),
                });

            //_productRepository.Update(allProducts);
            //};
            //_relatedProductRepository.Insert(relatedProducts);

            #endregion


            #region Product Tags

            //product tags
            AddProductTag(product25GiftCard, "nice");
            AddProductTag(product25GiftCard, "gift");
            AddProductTag(productNikeTailwind, "cool");
            AddProductTag(productNikeTailwind, "apparel");
            AddProductTag(productNikeTailwind, "shirt");
            AddProductTag(productBeatsPill, "computer");
            AddProductTag(productBeatsPill, "cool");
            AddProductTag(productNikeFloral, "cool");
            AddProductTag(productNikeFloral, "shoes");
            AddProductTag(productNikeFloral, "apparel");
            AddProductTag(productAdobePhotoshop, "computer");
            AddProductTag(productAdobePhotoshop, "awesome");
            AddProductTag(productUniversalTabletCover, "computer");
            AddProductTag(productUniversalTabletCover, "cool");
            AddProductTag(productOversizedWomenTShirt, "cool");
            AddProductTag(productOversizedWomenTShirt, "apparel");
            AddProductTag(productOversizedWomenTShirt, "shirt");
            AddProductTag(productAppleMacBookPro, "compact");
            AddProductTag(productAppleMacBookPro, "awesome");
            AddProductTag(productAppleMacBookPro, "computer");
            AddProductTag(productAsusN551JK, "compact");
            AddProductTag(productAsusN551JK, "awesome");
            AddProductTag(productAsusN551JK, "computer");
            AddProductTag(productFahrenheit, "awesome");
            AddProductTag(productFahrenheit, "book");
            AddProductTag(productFahrenheit, "nice");
            AddProductTag(productHtcOne, "cell");
            AddProductTag(productHtcOne, "compact");
            AddProductTag(productHtcOne, "awesome");
            AddProductTag(productBuildComputer, "awesome");
            AddProductTag(productBuildComputer, "computer");
            AddProductTag(productNikonD5500DSLR, "cool");
            AddProductTag(productNikonD5500DSLR, "camera");
            AddProductTag(productLeica, "camera");
            AddProductTag(productLeica, "cool");
            AddProductTag(productDigitalStorm, "cool");
            AddProductTag(productDigitalStorm, "computer");
            AddProductTag(productWindows8Pro, "awesome");
            AddProductTag(productWindows8Pro, "computer");
            AddProductTag(productCustomTShirt, "cool");
            AddProductTag(productCustomTShirt, "shirt");
            AddProductTag(productCustomTShirt, "apparel");
            AddProductTag(productElegantGemstoneNecklace, "jewelry");
            AddProductTag(productElegantGemstoneNecklace, "awesome");
            AddProductTag(productFlowerGirlBracelet, "awesome");
            AddProductTag(productFlowerGirlBracelet, "jewelry");
            AddProductTag(productFirstPrizePies, "book");
            AddProductTag(productAdidas, "cool");
            AddProductTag(productAdidas, "shoes");
            AddProductTag(productAdidas, "apparel");
            AddProductTag(productLenovoIdeaCentre, "awesome");
            AddProductTag(productLenovoIdeaCentre, "computer");
            AddProductTag(productSamsungSeries, "nice");
            AddProductTag(productSamsungSeries, "computer");
            AddProductTag(productSamsungSeries, "compact");
            AddProductTag(productHpSpectre, "nice");
            AddProductTag(productHpSpectre, "computer");
            AddProductTag(productHpEnvy, "computer");
            AddProductTag(productHpEnvy, "cool");
            AddProductTag(productHpEnvy, "compact");
            AddProductTag(productObeyHat, "apparel");
            AddProductTag(productObeyHat, "cool");
            AddProductTag(productLeviJeans, "cool");
            AddProductTag(productLeviJeans, "jeans");
            AddProductTag(productLeviJeans, "apparel");
            AddProductTag(productSoundForge, "game");
            AddProductTag(productSoundForge, "computer");
            AddProductTag(productSoundForge, "cool");
            AddProductTag(productNightVision, "awesome");
            AddProductTag(productNightVision, "digital");
            AddProductTag(productSunglasses, "apparel");
            AddProductTag(productSunglasses, "cool");
            AddProductTag(productHtcOneMini, "awesome");
            AddProductTag(productHtcOneMini, "compact");
            AddProductTag(productHtcOneMini, "cell");
            AddProductTag(productIfYouWait, "digital");
            AddProductTag(productIfYouWait, "awesome");
            AddProductTag(productNokiaLumia, "awesome");
            AddProductTag(productNokiaLumia, "cool");
            AddProductTag(productNokiaLumia, "camera");
            AddProductTag(productScienceAndFaith, "digital");
            AddProductTag(productScienceAndFaith, "awesome");
            AddProductTag(productPrideAndPrejudice, "book");
            AddProductTag(productLenovoThinkpad, "awesome");
            AddProductTag(productLenovoThinkpad, "computer");
            AddProductTag(productLenovoThinkpad, "compact");
            AddProductTag(productNikeZoom, "jeans");
            AddProductTag(productNikeZoom, "cool");
            AddProductTag(productNikeZoom, "apparel");
            AddProductTag(productEngagementRing, "jewelry");
            AddProductTag(productEngagementRing, "awesome");


            #endregion

            //reviews
            var random = new Random();
            foreach (var product in allProducts)
            {
                if (product.ProductType != ProductType.SimpleProduct)
                    continue;

                //only 3 of 4 products will have reviews
                if (random.Next(4) == 3)
                    continue;

                //rating from 4 to 5
                var rating = random.Next(4, 6);
                var productReview = new ProductReview
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    CustomerId = defaultCustomer.Id,
                    ProductId = product.Id,
                    IsApproved = true,
                    StoreId = defaultStore.Id,
                    Title = "Some sample review",
                    ReviewText = string.Format("This sample review is for the {0}. I've been waiting for this product to be available. It is priced just right.", product.Name),
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    CreatedOnUtc = DateTime.UtcNow,

                };
                _productReviewRepository.Insert(productReview);

                product.ApprovedRatingSum = rating;
                product.ApprovedTotalReviews = product.ApprovedTotalReviews + 1;

            }
            _productRepository.Update(allProducts);
        }

        protected virtual void InstallForums()
        {
            var forumGroup = new ForumGroup
            {
                Name = "General",
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            _forumGroupRepository.Insert(forumGroup);

            var newProductsForum = new Forum
            {
                ForumGroupId = forumGroup.Id,
                Name = "New Products",
                Description = "Discuss new products and industry trends",
                NumTopics = 0,
                NumPosts = 0,
                LastPostCustomerId = 0,
                LastPostTime = null,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            _forumRepository.Insert(newProductsForum);

            var mobileDevicesForum = new Forum
            {
                ForumGroupId = forumGroup.Id,
                Name = "Mobile Devices Forum",
                Description = "Discuss the mobile phone market",
                NumTopics = 0,
                NumPosts = 0,
                LastPostCustomerId = 0,
                LastPostTime = null,
                DisplayOrder = 10,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            _forumRepository.Insert(mobileDevicesForum);

            var packagingShippingForum = new Forum
            {
                ForumGroupId = forumGroup.Id,
                Name = "Packaging & Shipping",
                Description = "Discuss packaging & shipping",
                NumTopics = 0,
                NumPosts = 0,
                LastPostTime = null,
                DisplayOrder = 20,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            _forumRepository.Insert(packagingShippingForum);
        }

        protected virtual void InstallDiscounts()
        {
            var discounts = new List<Discount>
                                {
                                    new Discount
                                        {
                                            Name = "Sample discount with coupon code",
                                            DiscountType = DiscountType.AssignedToSkus,
                                            DiscountLimitation = DiscountLimitationType.Unlimited,
                                            UsePercentage = false,
                                            DiscountAmount = 10,
                                            RequiresCouponCode = true,
                                            CouponCode = "123",
                                        },
                                    new Discount
                                        {
                                            Name = "'20% order total' discount",
                                            DiscountType = DiscountType.AssignedToOrderTotal,
                                            DiscountLimitation = DiscountLimitationType.Unlimited,
                                            UsePercentage = true,
                                            DiscountPercentage = 20,
                                            StartDateUtc = new DateTime(2010,1,1),
                                            EndDateUtc = new DateTime(2020,1,1),
                                            RequiresCouponCode = true,
                                            CouponCode = "456",
                                        },
                                };
            _discountRepository.Insert(discounts);
        }

        protected virtual void InstallBlogPosts()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var blogPosts = new List<BlogPost>
                                {
                                    new BlogPost
                                        {
                                             Id = 1,
                                             AllowComments = true,
                                             LanguageId = defaultLanguage.Id,
                                             Title = "How a blog can help your growing e-Commerce business",
                                             BodyOverview = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p>",
                                             Body = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p><h3>1) Blog is useful in educating your customers</h3><p>Blogging is one of the best way by which you can educate your customers about your products/services that you offer. This helps you as a business owner to bring more value to your brand. When you provide useful information to the customers about your products, they are more likely to buy products from you. You can use your blog for providing tutorials in regard to the use of your products.</p><p><strong>For example:</strong> If you have an online store that offers computer parts. You can write tutorials about how to build a computer or how to make your computer&rsquo;s performance better. While talking about these things, you can mention products in the tutorials and provide link to your products within the blog post from your website. Your potential customers might get different ideas of using your product and will likely to buy products from your online store.</p><h3>2) Blog helps your business in Search Engine Optimization (SEO)</h3><p>Blog posts create more internal links to your website which helps a lot in SEO. Blog is a great way to have quality content on your website related to your products/services which is indexed by all major search engines like Google, Bing and Yahoo. The more original content you write in your blog post, the better ranking you will get in search engines. SEO is an on-going process and posting blog posts regularly keeps your site active all the time which is beneficial when it comes to search engine optimization.</p><p><strong>For example:</strong> Let&rsquo;s say you sell &ldquo;Sony Television Model XYZ&rdquo; and you regularly publish blog posts about your product. Now, whenever someone searches for &ldquo;Sony Television Model XYZ&rdquo;, Google will crawl on your website knowing that you have something to do with this particular product. Hence, your website will show up on the search result page whenever this item is being searched.</p><h3>3) Blog helps in boosting your sales by convincing the potential customers to buy</h3><p>If you own an online business, there are so many ways you can share different stories with your audience in regard your products/services that you offer. Talk about how you started your business, share stories that educate your audience about what&rsquo;s new in your industry, share stories about how your product/service was beneficial to someone or share anything that you think your audience might find interesting (it does not have to be related to your product). This kind of blogging shows that you are an expert in your industry and interested in educating your audience. It sets you apart in the competitive market. This gives you an opportunity to showcase your expertise by educating the visitors and it can turn your audience into buyers.</p><p><strong>Fun Fact:</strong> Did you know that 92% of companies who decided to blog acquired customers through their blog?</p><p><a href=\"http://www.grandnode.com/\">grandnode</a> is great e-Commerce solution that also offers a variety of CMS features including blog. A store owner has full access for managing the blog posts and related comments.</p>",
                                             Tags = "e-commerce, blog, moey",
                                             CreatedOnUtc = DateTime.UtcNow,
                                        },
                                    new BlogPost
                                        {
                                             Id = 2,
                                             AllowComments = true,
                                             LanguageId = defaultLanguage.Id,
                                             Title = "Why your online store needs a wish list",
                                             BodyOverview = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p>",
                                             Body = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p><p>Does every e-Commerce store needs a wish list? The answer to this question in most cases is yes, because of the following reasons:</p><p><strong>Understanding the needs of your customers</strong> - A wish list is a great way to know what is in your customer&rsquo;s mind. Try to think the purchase history as a small portion of the customer&rsquo;s preferences. But, the wish list is like a wide open door that can give any online business a lot of valuable information about their customer and what they like or desire.</p><p><strong>Shoppers like to share their wish list with friends and family</strong> - Providing your customers a way to email their wish list to their friends and family is a pleasant way to make online shopping enjoyable for the shoppers. It is always a good idea to make the wish list sharable by a unique link so that it can be easily shared though different channels like email or on social media sites.</p><p><strong>Wish list can be a great marketing tool</strong> &ndash; Another way to look at wish list is a great marketing tool because it is extremely targeted and the recipients are always motivated to use it. For example: when your younger brother tells you that his wish list is on a certain e-Commerce store. What is the first thing you are going to do? You are most likely to visit the e-Commerce store, check out the wish list and end up buying something for your younger brother.</p><p>So, how a wish list is a marketing tool? The reason is quite simple, it introduce your online store to new customers just how it is explained in the above example.</p><p><strong>Encourage customers to return to the store site</strong> &ndash; Having a feature of wish list on the store site can increase the return traffic because it encourages customers to come back and buy later. Allowing the customers to save the wish list to their online accounts gives them a reason return to the store site and login to the account at any time to view or edit the wish list items.</p><p><strong>Wish list can be used for gifts for different occasions like weddings or birthdays. So, what kind of benefits a gift-giver gets from a wish list?</strong></p><ul><li>It gives them a surety that they didn&rsquo;t buy a wrong gift</li><li>It guarantees that the recipient will like the gift</li><li>It avoids any awkward moments when the recipient unwraps the gift and as a gift-giver you got something that the recipient do not want</li></ul><p><strong>Wish list is a great feature to have on a store site &ndash; So, what kind of benefits a business owner gets from a wish list</strong></p><ul><li>It is a great way to advertise an online store as many people do prefer to shop where their friend or family shop online</li><li>It allows the current customers to return to the store site and open doors for the new customers</li><li>It allows store admins to track what&rsquo;s in customers wish list and run promotions accordingly to target specific customer segments</li></ul><p><a href=\"http://www.grandnode.com/\">grandnode</a> offers the feature of wish list that allows customers to create a list of products that they desire or planning to buy in future.</p>",
                                             Tags = "e-commerce, grandnode, sample tag, money",
                                             CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                        },
                                };
            _blogPostRepository.Insert(blogPosts);

            //search engine names
            foreach (var blogPost in blogPosts)
            {
                var seName = blogPost.ValidateSeName("", blogPost.Title, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = blogPost.Id,
                    EntityName = "BlogPost",
                    LanguageId = blogPost.LanguageId,
                    IsActive = true,
                    Slug = seName
                });
                blogPost.SeName = seName;
                _blogPostRepository.Update(blogPost);

            }
        }

        protected virtual void InstallBlogPosts(string defaultUserEmail)
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var blogPosts = new List<BlogPost>
                                {
                                    new BlogPost
                                        {
                                             AllowComments = true,
                                             LanguageId = defaultLanguage.Id,
                                             Title = "How a blog can help your growing e-Commerce business",
                                             BodyOverview = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p>",
                                             Body = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p><h3>1) Blog is useful in educating your customers</h3><p>Blogging is one of the best way by which you can educate your customers about your products/services that you offer. This helps you as a business owner to bring more value to your brand. When you provide useful information to the customers about your products, they are more likely to buy products from you. You can use your blog for providing tutorials in regard to the use of your products.</p><p><strong>For example:</strong> If you have an online store that offers computer parts. You can write tutorials about how to build a computer or how to make your computer&rsquo;s performance better. While talking about these things, you can mention products in the tutorials and provide link to your products within the blog post from your website. Your potential customers might get different ideas of using your product and will likely to buy products from your online store.</p><h3>2) Blog helps your business in Search Engine Optimization (SEO)</h3><p>Blog posts create more internal links to your website which helps a lot in SEO. Blog is a great way to have quality content on your website related to your products/services which is indexed by all major search engines like Google, Bing and Yahoo. The more original content you write in your blog post, the better ranking you will get in search engines. SEO is an on-going process and posting blog posts regularly keeps your site active all the time which is beneficial when it comes to search engine optimization.</p><p><strong>For example:</strong> Let&rsquo;s say you sell &ldquo;Sony Television Model XYZ&rdquo; and you regularly publish blog posts about your product. Now, whenever someone searches for &ldquo;Sony Television Model XYZ&rdquo;, Google will crawl on your website knowing that you have something to do with this particular product. Hence, your website will show up on the search result page whenever this item is being searched.</p><h3>3) Blog helps in boosting your sales by convincing the potential customers to buy</h3><p>If you own an online business, there are so many ways you can share different stories with your audience in regard your products/services that you offer. Talk about how you started your business, share stories that educate your audience about what&rsquo;s new in your industry, share stories about how your product/service was beneficial to someone or share anything that you think your audience might find interesting (it does not have to be related to your product). This kind of blogging shows that you are an expert in your industry and interested in educating your audience. It sets you apart in the competitive market. This gives you an opportunity to showcase your expertise by educating the visitors and it can turn your audience into buyers.</p><p><strong>Fun Fact:</strong> Did you know that 92% of companies who decided to blog acquired customers through their blog?</p><p><a href=\"http://www.grandnode.com/\">Grandnode</a> is great e-Commerce solution that also offers a variety of CMS features including blog. A store owner has full access for managing the blog posts and related comments.</p>",
                                             Tags = "e-commerce, blog, moey",
                                             CreatedOnUtc = DateTime.UtcNow,
                                        },
                                    new BlogPost
                                        {
                                             AllowComments = true,
                                             LanguageId = defaultLanguage.Id,
                                             Title = "Why your online store needs a wish list",
                                             BodyOverview = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p>",
                                             Body = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p><p>Does every e-Commerce store needs a wish list? The answer to this question in most cases is yes, because of the following reasons:</p><p><strong>Understanding the needs of your customers</strong> - A wish list is a great way to know what is in your customer&rsquo;s mind. Try to think the purchase history as a small portion of the customer&rsquo;s preferences. But, the wish list is like a wide open door that can give any online business a lot of valuable information about their customer and what they like or desire.</p><p><strong>Shoppers like to share their wish list with friends and family</strong> - Providing your customers a way to email their wish list to their friends and family is a pleasant way to make online shopping enjoyable for the shoppers. It is always a good idea to make the wish list sharable by a unique link so that it can be easily shared though different channels like email or on social media sites.</p><p><strong>Wish list can be a great marketing tool</strong> &ndash; Another way to look at wish list is a great marketing tool because it is extremely targeted and the recipients are always motivated to use it. For example: when your younger brother tells you that his wish list is on a certain e-Commerce store. What is the first thing you are going to do? You are most likely to visit the e-Commerce store, check out the wish list and end up buying something for your younger brother.</p><p>So, how a wish list is a marketing tool? The reason is quite simple, it introduce your online store to new customers just how it is explained in the above example.</p><p><strong>Encourage customers to return to the store site</strong> &ndash; Having a feature of wish list on the store site can increase the return traffic because it encourages customers to come back and buy later. Allowing the customers to save the wish list to their online accounts gives them a reason return to the store site and login to the account at any time to view or edit the wish list items.</p><p><strong>Wish list can be used for gifts for different occasions like weddings or birthdays. So, what kind of benefits a gift-giver gets from a wish list?</strong></p><ul><li>It gives them a surety that they didn&rsquo;t buy a wrong gift</li><li>It guarantees that the recipient will like the gift</li><li>It avoids any awkward moments when the recipient unwraps the gift and as a gift-giver you got something that the recipient do not want</li></ul><p><strong>Wish list is a great feature to have on a store site &ndash; So, what kind of benefits a business owner gets from a wish list</strong></p><ul><li>It is a great way to advertise an online store as many people do prefer to shop where their friend or family shop online</li><li>It allows the current customers to return to the store site and open doors for the new customers</li><li>It allows store admins to track what&rsquo;s in customers wish list and run promotions accordingly to target specific customer segments</li></ul><p><a href=\"http://www.grandnode.com/\">grandnode</a> offers the feature of wish list that allows customers to create a list of products that they desire or planning to buy in future.</p>",
                                             Tags = "e-commerce, grandnode, sample tag, money",
                                             CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                        },
                                };
            _blogPostRepository.Insert(blogPosts);

            //search engine names
            foreach (var blogPost in blogPosts)
            {
                blogPost.SeName = blogPost.ValidateSeName("", blogPost.Title, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = blogPost.Id,
                    EntityName = "BlogPost",
                    LanguageId = blogPost.LanguageId,
                    IsActive = true,
                    Slug = blogPost.ValidateSeName("", blogPost.Title, true)
                });
                _blogPostRepository.Update(blogPost);
            }

        }
        protected virtual void InstallNews()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var news = new List<NewsItem>
                                {
                                    new NewsItem
                                    {
                                         Id = 1,
                                         AllowComments = true,
                                         LanguageId = defaultLanguage.Id,
                                         Title = "About Grandnode",
                                         Short = "It's stable and highly usable. From downloads to documentation, www.grandnode.com offers a comprehensive base of information, resources, and support to the grandnode community.",
                                         Full = "<p>For full feature list go to <a href=\"http://www.grandnode.com\">grandnode.com</a></p><p>Providing outstanding custom search engine optimization, web development services and e-commerce development solutions to our clients at a fair price in a professional manner.</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow,
                                    },
                                    new NewsItem
                                    {
                                         Id = 2,
                                         AllowComments = true,
                                         LanguageId = defaultLanguage.Id,
                                         Title = "Grandnode new release!",
                                         Short = "grandnode includes everything you need to begin your e-commerce online store. We have thought of everything and it's all included! grandnode is a fully customizable shopping cart",
                                         Full = "<p>Grandnode includes everything you need to begin your e-commerce online store. We have thought of everything and it's all included!</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                    },
                                    new NewsItem
                                    {
                                         Id = 3,
                                         AllowComments = true,
                                         LanguageId = defaultLanguage.Id,
                                         Title = "New online store is open!",
                                         Short = "The new grandnode store is open now! We are very excited to offer our new range of products. We will be constantly adding to our range so please register on our site.",
                                         Full = "<p>Our online store is officially up and running. Stock up for the holiday season! We have a great selection of items. We will be constantly adding to our range so please register on our site, this will enable you to keep up to date with any new products.</p><p>All shipping is worldwide and will leave the same day an order is placed! Happy Shopping and spread the word!!</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow.AddSeconds(2),
                                    },

                                };
            _newsItemRepository.Insert(news);

            //search engine names
            foreach (var newsItem in news)
            {
                newsItem.SeName = newsItem.ValidateSeName("", newsItem.Title, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = newsItem.Id,
                    EntityName = "NewsItem",
                    LanguageId = newsItem.LanguageId,
                    IsActive = true,
                    Slug = newsItem.SeName
                });
                _newsItemRepository.Update(newsItem);
            }
        }

        protected virtual void InstallPolls()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var poll1 = new Poll
            {
                Id = 1,
                LanguageId = defaultLanguage.Id,
                Name = "Do you like Grandnode for MongoDB?",
                SystemKeyword = "",
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 1,
            };
            poll1.PollAnswers.Add(new PollAnswer
            {
                Name = "Excellent",
                DisplayOrder = 1,
                PollId = 1,
                Id = 1,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            poll1.PollAnswers.Add(new PollAnswer
            {
                Name = "Good",
                DisplayOrder = 2,
                PollId = 1,
                Id = 2,
                _id = ObjectId.GenerateNewId().ToString(),
            });
            poll1.PollAnswers.Add(new PollAnswer
            {
                Name = "Poor",
                DisplayOrder = 3,
                PollId = 1,
                Id = 3,
                _id = ObjectId.GenerateNewId().ToString(),

            });
            poll1.PollAnswers.Add(new PollAnswer
            {
                Name = "Very bad",
                DisplayOrder = 4,
                PollId = 1,
                Id = 4,
                _id = ObjectId.GenerateNewId().ToString(),

            });
            _pollRepository.Insert(poll1);
        }

        protected virtual void InstallActivityLogTypes()
        {
            var activityLogTypes = new List<ActivityLogType>
                                      {
                                          //admin area activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCategory",
                                                  Enabled = true,
                                                  Name = "Add a new category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomer",
                                                  Enabled = true,
                                                  Name = "Add a new customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomerRole",
                                                  Enabled = true,
                                                  Name = "Add a new customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewDiscount",
                                                  Enabled = true,
                                                  Name = "Add a new discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewGiftCard",
                                                  Enabled = true,
                                                  Name = "Add a new gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewManufacturer",
                                                  Enabled = true,
                                                  Name = "Add a new manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProduct",
                                                  Enabled = true,
                                                  Name = "Add a new product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProductAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSetting",
                                                  Enabled = true,
                                                  Name = "Add a new setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                SystemKeyword = "AddNewTopic",
                                                Enabled = true,
                                                Name = "Add a new topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewWidget",
                                                  Enabled = true,
                                                  Name = "Add a new widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCategory",
                                                  Enabled = true,
                                                  Name = "Delete category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomer",
                                                  Enabled = true,
                                                  Name = "Delete a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomerRole",
                                                  Enabled = true,
                                                  Name = "Delete a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteDiscount",
                                                  Enabled = true,
                                                  Name = "Delete a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteGiftCard",
                                                  Enabled = true,
                                                  Name = "Delete a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteManufacturer",
                                                  Enabled = true,
                                                  Name = "Delete a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteOrder",
                                                  Enabled = true,
                                                  Name = "Delete an order"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProduct",
                                                  Enabled = true,
                                                  Name = "Delete a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProductAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteReturnRequest",
                                                  Enabled = true,
                                                  Name = "Delete a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSetting",
                                                  Enabled = true,
                                                  Name = "Delete a setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteTopic",
                                                  Enabled = true,
                                                  Name = "Delete a topic"
                                              },
                                        new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteWidget",
                                                  Enabled = true,
                                                  Name = "Delete a widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCategory",
                                                  Enabled = true,
                                                  Name = "Edit category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomer",
                                                  Enabled = true,
                                                  Name = "Edit a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomerRole",
                                                  Enabled = true,
                                                  Name = "Edit a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditDiscount",
                                                  Enabled = true,
                                                  Name = "Edit a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditGiftCard",
                                                  Enabled = true,
                                                  Name = "Edit a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditManufacturer",
                                                  Enabled = true,
                                                  Name = "Edit a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditOrder",
                                                  Enabled = true,
                                                  Name = "Edit an order"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProduct",
                                                  Enabled = true,
                                                  Name = "Edit a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProductAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditPromotionProviders",
                                                  Enabled = true,
                                                  Name = "Edit promotion providers"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditReturnRequest",
                                                  Enabled = true,
                                                  Name = "Edit a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSettings",
                                                  Enabled = true,
                                                  Name = "Edit setting(s)"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditTopic",
                                                  Enabled = true,
                                                  Name = "Edit a topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditWidget",
                                                  Enabled = true,
                                                  Name = "Edit a widget"
                                              },
                                              //public store activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewCategory",
                                                  Enabled = false,
                                                  Name = "Public store. View a category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewManufacturer",
                                                  Enabled = false,
                                                  Name = "Public store. View a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewProduct",
                                                  Enabled = false,
                                                  Name = "Public store. View a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.PlaceOrder",
                                                  Enabled = false,
                                                  Name = "Public store. Place an order"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.SendPM",
                                                  Enabled = false,
                                                  Name = "Public store. Send PM"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ContactUs",
                                                  Enabled = false,
                                                  Name = "Public store. Use contact us form"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToCompareList",
                                                  Enabled = false,
                                                  Name = "Public store. Add to compare list"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToShoppingCart",
                                                  Enabled = false,
                                                  Name = "Public store. Add to shopping cart"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToWishlist",
                                                  Enabled = false,
                                                  Name = "Public store. Add to wishlist"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Login",
                                                  Enabled = false,
                                                  Name = "Public store. Login"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Logout",
                                                  Enabled = false,
                                                  Name = "Public store. Logout"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddProductReview",
                                                  Enabled = false,
                                                  Name = "Public store. Add product review"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddNewsComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add news comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddBlogComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add blog comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum post"
                                              },
                                      };
            _activityLogTypeRepository.Insert(activityLogTypes);
        }

        protected virtual void InstallProductTemplates()
        {
            var productTemplates = new List<ProductTemplate>
                               {
                                   new ProductTemplate
                                       {
                                           Name = "Simple product",
                                           ViewPath = "ProductTemplate.Simple",
                                           DisplayOrder = 10
                                       },
                                   new ProductTemplate
                                       {
                                           Name = "Grouped product (with variants)",
                                           ViewPath = "ProductTemplate.Grouped",
                                           DisplayOrder = 100
                                       },
                               };
            _productTemplateRepository.Insert(productTemplates);
        }

        protected virtual void InstallCategoryTemplates()
        {
            var categoryTemplates = new List<CategoryTemplate>
                               {
                                   new CategoryTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "CategoryTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            _categoryTemplateRepository.Insert(categoryTemplates);
        }

        protected virtual void InstallManufacturerTemplates()
        {
            var manufacturerTemplates = new List<ManufacturerTemplate>
                               {
                                   new ManufacturerTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            _manufacturerTemplateRepository.Insert(manufacturerTemplates);
        }

        protected virtual void InstallTopicTemplates()
        {
            var topicTemplates = new List<TopicTemplate>
                               {
                                   new TopicTemplate
                                       {
                                           Name = "Default template",
                                           ViewPath = "TopicDetails",
                                           DisplayOrder = 1
                                       },
                               };
            _topicTemplateRepository.Insert(topicTemplates);
        }

        protected virtual void InstallScheduleTasks()
        {
            var tasks = new List<ScheduleTask>
            {
                new ScheduleTask
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "Nop.Services.Messages.QueuedMessagesSendTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "Nop.Services.Common.KeepAliveTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "Nop.Services.Customers.DeleteGuestsTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "Nop.Services.Caching.ClearCacheTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear log",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Nop.Services.Logging.ClearLogTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Update currency exchange rates",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Nop.Services.Directory.UpdateExchangeRateTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
            };

            _scheduleTaskRepository.Insert(tasks);
        }

        protected virtual void InstallReturnRequestReasons()
        {
            var returnRequestReasons = new List<ReturnRequestReason>
                                {
                                    new ReturnRequestReason
                                        {
                                            Name = "Received Wrong Product",
                                            DisplayOrder = 1
                                        },
                                    new ReturnRequestReason
                                        {
                                            Name = "Wrong Product Ordered",
                                            DisplayOrder = 2
                                        },
                                    new ReturnRequestReason
                                        {
                                            Name = "There Was A Problem With The Product",
                                            DisplayOrder = 3
                                        }
                                };
            _returnRequestReasonRepository.Insert(returnRequestReasons);
        }
        protected virtual void InstallReturnRequestActions()
        {
            var returnRequestActions = new List<ReturnRequestAction>
                                {
                                    new ReturnRequestAction
                                        {
                                            Name = "Repair",
                                            DisplayOrder = 1
                                        },
                                    new ReturnRequestAction
                                        {
                                            Name = "Replacement",
                                            DisplayOrder = 2
                                        },
                                    new ReturnRequestAction
                                        {
                                            Name = "Store Credit",
                                            DisplayOrder = 3
                                        }
                                };
            _returnRequestActionRepository.Insert(returnRequestActions);
        }

        protected virtual void InstallWarehouses()
        {
            var warehouse1address = new Address
            {
                Address1 = "21 West 52nd Street",
                City = "New York",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            _addressRepository.Insert(warehouse1address);
            var warehouse2address = new Address
            {
                Address1 = "300 South Spring Stree",
                City = "Los Angeles",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "California").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "90013",
                CreatedOnUtc = DateTime.UtcNow,
            };
            _addressRepository.Insert(warehouse2address);
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    Name = "Warehouse 1 (New York)",
                    AddressId = warehouse1address.Id
                },
                new Warehouse
                {
                    Name = "Warehouse 2 (Los Angeles)",
                    AddressId = warehouse2address.Id
                }
            };

            _warehouseRepository.Insert(warehouses);
        }

        protected virtual void InstallVendors()
        {
            var vendors = new List<Vendor>
            {
                new Vendor
                {
                    Id = 1,
                    Name = "Vendor 1",
                    Email = "vendor1email@gmail.com",
                    Description = "Some description...",
                    AdminComment = "",
                    PictureId = 0,
                    Active = true,
                    DisplayOrder = 1,
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = "6, 3, 9, 18",
                },
                new Vendor
                {
                    Id = 2,
                    Name = "Vendor 2",
                    Email = "vendor2email@gmail.com",
                    Description = "Some description...",
                    AdminComment = "",
                    PictureId = 0,
                    Active = true,
                    DisplayOrder = 2,
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = "6, 3, 9, 18",
                }
            };

            _vendorRepository.Insert(vendors);

            //search engine names
            foreach (var vendor in vendors)
            {
                var seName = vendor.ValidateSeName(vendor.SeName, vendor.Name, true);
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = vendor.Id,
                    EntityName = "Vendor",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = seName
                });
                vendor.SeName = seName;
                _vendorRepository.Update(vendor);
            }


        }

        protected virtual void InstallAffiliates()
        {
            var affiliateAddress = new Address
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "affiliate_email@gmail.com",
                Company = "Company name here...",
                City = "New York",
                Address1 = "21 West 52nd Street",
                ZipPostalCode = "10021",
                PhoneNumber = "123456789",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                CreatedOnUtc = DateTime.UtcNow,
            };
            _addressRepository.Insert(affiliateAddress);
            var affilate = new Affiliate
            {
                Active = true,
                Address = affiliateAddress
            };
            _affiliateRepository.Insert(affilate);
        }

        private void AddProductTag(Product product, string tag)
        {
            var productTag = _productTagRepository.Table.FirstOrDefault(pt => pt.Name == tag);
            if (productTag == null)
            {
                productTag = new ProductTag
                {
                    Name = tag,
                };

                _productTagRepository.Insert(productTag);
            }
            productTag.Count = productTag.Count + 1;
            _productTagRepository.Update(productTag);
            productTag.ProductId = product.Id;
            product.ProductTags.Add(productTag);
            _productRepository.Update(product);
        }

        private void CreateIndexes()
        {
            _versionRepository.Collection.Indexes.CreateOneAsync(Builders<GrandNodeVersion>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            //Store
            _storeRepository.Collection.Indexes.CreateOneAsync(Builders<Store>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _storeRepository.Collection.Indexes.CreateOneAsync(Builders<Store>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            //measure
            _measureDimensionRepository.Collection.Indexes.CreateOneAsync(Builders<MeasureDimension>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _measureWeightRepository.Collection.Indexes.CreateOneAsync(Builders<MeasureWeight>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //TaxCategory
            _taxCategoryRepository.Collection.Indexes.CreateOneAsync(Builders<TaxCategory>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //Language
            _languageRepository.Collection.Indexes.CreateOneAsync(Builders<Language>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _lsrRepository.Collection.Indexes.CreateOneAsync(Builders<LocaleStringResource>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _lsrRepository.Collection.Indexes.CreateOneAsync(Builders<LocaleStringResource>.IndexKeys.Ascending(x => x.LanguageId), new CreateIndexOptions() { Name = "Language" });

            _currencyRepository.Collection.Indexes.CreateOneAsync(Builders<Currency>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //customer
            _customerRepository.Collection.Indexes.CreateOneAsync(Builders<Customer>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _customerRepository.Collection.Indexes.CreateOneAsync(Builders<Customer>.IndexKeys.Ascending(x => x.Email), new CreateIndexOptions() { Name = "Email" });
            _customerRepository.Collection.Indexes.CreateOneAsync(Builders<Customer>.IndexKeys.Ascending(x => x.Username), new CreateIndexOptions() { Name = "Username" });

            _customerRoleRepository.Collection.Indexes.CreateOneAsync(Builders<CustomerRole>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _customerRoleProductRepository.Collection.Indexes.CreateOneAsync(Builders<CustomerRoleProduct>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _customerRoleProductRepository.Collection.Indexes.CreateOneAsync(Builders<CustomerRoleProduct>.IndexKeys.Ascending(x => x.Id).Ascending(x=>x.DisplayOrder), new CreateIndexOptions() { Name = "CustomerRoleId_DisplayOrder", Unique = false });

            _addressRepository.Collection.Indexes.CreateOneAsync(Builders<Address>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //specificationAttribute
            _specificationAttributeRepository.Collection.Indexes.CreateOneAsync(Builders<SpecificationAttribute>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _specificationAttributeRepository.Collection.Indexes.CreateOneAsync(Builders<SpecificationAttribute>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            //checkoutAttribute
            _checkoutAttributeRepository.Collection.Indexes.CreateOneAsync(Builders<CheckoutAttribute>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _checkoutAttributeRepository.Collection.Indexes.CreateOneAsync(Builders<CheckoutAttribute>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            _productAttributeRepository.Collection.Indexes.CreateOneAsync(Builders<ProductAttribute>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //category
            _categoryRepository.Collection.Indexes.CreateOneAsync(Builders<Category>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _categoryRepository.Collection.Indexes.CreateOneAsync(Builders<Category>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            //manufacturer
            _manufacturerRepository.Collection.Indexes.CreateOneAsync(Builders<Manufacturer>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _manufacturerRepository.Collection.Indexes.CreateOneAsync(Builders<Manufacturer>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            //Product
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.Sku), new CreateIndexOptions() { Name = "Sku", Unique = false });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.ShowOnHomePage), new CreateIndexOptions() { Name = "ShowOnHomePage" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.Name), new CreateIndexOptions() { Name = "Name_asc" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Descending(x => x.Name), new CreateIndexOptions() { Name = "Name_desc" });

            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.CreatedOnUtc), new CreateIndexOptions() { Name = "CreatedOnUtc_asc" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Descending(x => x.CreatedOnUtc), new CreateIndexOptions() { Name = "CreatedOnUtc_desc" });

            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending(x => x.Price), new CreateIndexOptions() { Name = "Price_asc" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Descending(x => x.Price), new CreateIndexOptions() { Name = "Price_desc" });

            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending("ProductTags.Id").Ascending("Published").Ascending("VisibleIndividually"), new CreateIndexOptions() { Name = "Tags" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending("Published").Ascending("VisibleIndividually"), new CreateIndexOptions() { Name = "ProductCategories" });
            _productRepository.Collection.Indexes.CreateOneAsync(Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending("Published").Ascending("VisibleIndividually"), new CreateIndexOptions() { Name = "ProductManufacturers" });

            _productReviewRepository.Collection.Indexes.CreateOneAsync(Builders<ProductReview>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _productReviewRepository.Collection.Indexes.CreateOneAsync(Builders<ProductReview>.IndexKeys.Ascending(x => x.ProductId), new CreateIndexOptions() { Name = "ProductId", Unique = false });

            _pictureRepository.Collection.Indexes.CreateOneAsync(Builders<Picture>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _productTagRepository.Collection.Indexes.CreateOneAsync(Builders<ProductTag>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _productTemplateRepository.Collection.Indexes.CreateOneAsync(Builders<ProductTemplate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //Product also purchased
            _productalsopurchasedRepository.Collection.Indexes.CreateOneAsync(Builders<ProductAlsoPurchased>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _productalsopurchasedRepository.Collection.Indexes.CreateOneAsync(Builders<ProductAlsoPurchased>.IndexKeys.Ascending(x => x.ProductId), new CreateIndexOptions() { Name = "ProductId", Unique = true });

            _urlRecordRepository.Collection.Indexes.CreateOneAsync(Builders<UrlRecord>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _urlRecordRepository.Collection.Indexes.CreateOneAsync(Builders<UrlRecord>.IndexKeys.Ascending(x => x.Slug), new CreateIndexOptions() { Name = "Slug" });

            _emailAccountRepository.Collection.Indexes.CreateOneAsync(Builders<EmailAccount>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _messageTemplateRepository.Collection.Indexes.CreateOneAsync(Builders<MessageTemplate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            //forum
            _forumGroupRepository.Collection.Indexes.CreateOneAsync(Builders<ForumGroup>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _forumRepository.Collection.Indexes.CreateOneAsync(Builders<Forum>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _forumpostRepository.Collection.Indexes.CreateOneAsync(Builders<ForumPost>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _forumtopicRepository.Collection.Indexes.CreateOneAsync(Builders<ForumTopic>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _forumsubscriptionRepository.Collection.Indexes.CreateOneAsync(Builders<ForumSubscription>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            // Country and Stateprovince
            _countryRepository.Collection.Indexes.CreateOneAsync(Builders<Country>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _countryRepository.Collection.Indexes.CreateOneAsync(Builders<Country>.IndexKeys.Ascending(x => x.DisplayOrder), new CreateIndexOptions() { Name = "DisplayOrder" });

            _stateProvinceRepository.Collection.Indexes.CreateOneAsync(Builders<StateProvince>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _stateProvinceRepository.Collection.Indexes.CreateOneAsync(Builders<StateProvince>.IndexKeys.Ascending(x => x.CountryId).Ascending(x => x.DisplayOrder).Ascending(x => x.Id), new CreateIndexOptions() { Name = "Country" });

            _discountRepository.Collection.Indexes.CreateOneAsync(Builders<Discount>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _blogPostRepository.Collection.Indexes.CreateOneAsync(Builders<BlogPost>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _topicRepository.Collection.Indexes.CreateOneAsync(Builders<Topic>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _newsItemRepository.Collection.Indexes.CreateOneAsync(Builders<NewsItem>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _newslettersubscriptionRepository.Collection.Indexes.CreateOneAsync(Builders<NewsLetterSubscription>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _pollRepository.Collection.Indexes.CreateOneAsync(Builders<Poll>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _shippingMethodRepository.Collection.Indexes.CreateOneAsync(Builders<ShippingMethod>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _deliveryDateRepository.Collection.Indexes.CreateOneAsync(Builders<DeliveryDate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            // Activity log
            _activityLogTypeRepository.Collection.Indexes.CreateOneAsync(Builders<ActivityLogType>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _activityLogRepository.Collection.Indexes.CreateOneAsync(Builders<ActivityLog>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _affiliateRepository.Collection.Indexes.CreateOneAsync(Builders<Affiliate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _blogcommentRepository.Collection.Indexes.CreateOneAsync(Builders<BlogComment>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _blogpostRepository.Collection.Indexes.CreateOneAsync(Builders<BlogPost>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _campaignRepository.Collection.Indexes.CreateOneAsync(Builders<Campaign>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _downloadRepository.Collection.Indexes.CreateOneAsync(Builders<Download>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _giftcardRepository.Collection.Indexes.CreateOneAsync(Builders<GiftCard>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _logRepository.Collection.Indexes.CreateOneAsync(Builders<Log>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _privatemessageRepository.Collection.Indexes.CreateOneAsync(Builders<PrivateMessage>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _categoryTemplateRepository.Collection.Indexes.CreateOneAsync(Builders<CategoryTemplate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _manufacturerTemplateRepository.Collection.Indexes.CreateOneAsync(Builders<ManufacturerTemplate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _topicTemplateRepository.Collection.Indexes.CreateOneAsync(Builders<TopicTemplate>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _scheduleTaskRepository.Collection.Indexes.CreateOneAsync(Builders<ScheduleTask>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _queuedemailRepository.Collection.Indexes.CreateOneAsync(Builders<QueuedEmail>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _returnrequestRepository.Collection.Indexes.CreateOneAsync(Builders<ReturnRequest>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _rewardpointshistoryRepository.Collection.Indexes.CreateOneAsync(Builders<RewardPointsHistory>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _rewardpointshistoryRepository.Collection.Indexes.CreateOneAsync(Builders<RewardPointsHistory>.IndexKeys.Ascending(x => x.CustomerId), new CreateIndexOptions() { Name = "CustomerId" });

            _searchtermRepository.Collection.Indexes.CreateOneAsync(Builders<SearchTerm>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _settingRepository.Collection.Indexes.CreateOneAsync(Builders<Setting>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _shipmentRepository.Collection.Indexes.CreateOneAsync(Builders<Shipment>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _shipmentRepository.Collection.Indexes.CreateOneAsync(Builders<Shipment>.IndexKeys.Ascending(x => x.OrderId), new CreateIndexOptions() { Name = "OrderId" });

            _warehouseRepository.Collection.Indexes.CreateOneAsync(Builders<Warehouse>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _orderRepository.Collection.Indexes.CreateOneAsync(Builders<Order>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _orderRepository.Collection.Indexes.CreateOneAsync(Builders<Order>.IndexKeys.Ascending("OrderItem.ProductId").Ascending(x => x.Deleted), new CreateIndexOptions() { Name = "OrderItemProductId" });

            _permissionRepository.Collection.Indexes.CreateOneAsync(Builders<PermissionRecord>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _vendorRepository.Collection.Indexes.CreateOneAsync(Builders<Vendor>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _externalAuthenticationRepository.Collection.Indexes.CreateOneAsync(Builders<ExternalAuthenticationRecord>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _externalAuthenticationRepository.Collection.Indexes.CreateOneAsync(Builders<ExternalAuthenticationRecord>.IndexKeys.Ascending(x => x.CustomerId), new CreateIndexOptions() { Name = "CustomerId" });

            _discountusageRepository.Collection.Indexes.CreateOneAsync(Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _discountusageRepository.Collection.Indexes.CreateOneAsync(Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.CustomerId), new CreateIndexOptions() { Name = "CustomerId" });
            _discountusageRepository.Collection.Indexes.CreateOneAsync(Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.DiscountId), new CreateIndexOptions() { Name = "DiscountId" });
            _discountusageRepository.Collection.Indexes.CreateOneAsync(Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.OrderId), new CreateIndexOptions() { Name = "OrderId" });

            _returnRequestActionRepository.Collection.Indexes.CreateOneAsync(Builders<ReturnRequestAction>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _returnRequestReasonRepository.Collection.Indexes.CreateOneAsync(Builders<ReturnRequestReason>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

            _contactUsRepository.Collection.Indexes.CreateOneAsync(Builders<ContactUs>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _contactUsRepository.Collection.Indexes.CreateOneAsync(Builders<ContactUs>.IndexKeys.Ascending(x => x.Email), new CreateIndexOptions() { Name = "Email", Unique = false });
            _contactUsRepository.Collection.Indexes.CreateOneAsync(Builders<ContactUs>.IndexKeys.Descending(x => x.CreatedOnUtc), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false });

            //customer action
            _customerActionType.Collection.Indexes.CreateOneAsync(Builders<CustomerActionType>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _customerActionConditionType.Collection.Indexes.CreateOneAsync(Builders<CustomerActionConditionType>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
            _customerAction.Collection.Indexes.CreateOneAsync(Builders<CustomerAction>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

        }

        #endregion

        #region Methods


        public virtual void InstallData(string defaultUserEmail,
            string defaultUserPassword, bool installSampleData = true)
        {
            CreateIndexes();
            InstallVersion();
            InstallStores();
            InstallMeasures();
            InstallTaxCategories();
            InstallLanguages();
            InstallCurrencies();
            InstallCountriesAndStates();
            InstallShippingMethods();
            InstallDeliveryDates();
            InstallCustomersAndUsers(defaultUserEmail, defaultUserPassword);
            InstallEmailAccounts();
            InstallMessageTemplates();
            InstallCustomerAction();
            InstallSettings();
            InstallTopicTemplates();
            InstallTopics();
            InstallLocaleResources();
            InstallActivityLogTypes();
            HashDefaultCustomerPassword(defaultUserEmail, defaultUserPassword);
            InstallProductTemplates();
            InstallCategoryTemplates();
            InstallManufacturerTemplates();
            InstallScheduleTasks();
            InstallReturnRequestReasons();
            InstallReturnRequestActions();
            if (installSampleData)
            {
                InstallCheckoutAttributes();
                InstallSpecificationAttributes();
                InstallProductAttributes();
                InstallCategories();
                InstallManufacturers();
                InstallProducts(defaultUserEmail);
                InstallForums();
                InstallDiscounts();
                InstallBlogPosts();
                InstallNews();
                InstallPolls();
                InstallWarehouses();
                InstallVendors();
                InstallAffiliates();
            }
        }


        #endregion

    }
}