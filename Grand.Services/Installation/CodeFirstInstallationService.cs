using Grand.Core;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.AdminSearch;
using Grand.Domain.Affiliates;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Cms;
using Grand.Domain.Common;
using Grand.Domain.Configuration;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Logging;
using Grand.Domain.Media;
using Grand.Domain.Messages;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Polls;
using Grand.Domain.PushNotifications;
using Grand.Domain.Security;
using Grand.Domain.Seo;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Tasks;
using Grand.Domain.Tax;
using Grand.Domain.Topics;
using Grand.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grand.Core.Data;

namespace Grand.Services.Installation
{
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<GrandNodeVersion> _versionRepository;
        private readonly IRepository<Bid> _bidRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<CampaignHistory> _campaignHistoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<ReturnRequest> _returnRequestRepository;
        private readonly IRepository<ReturnRequestNote> _returnRequestNoteRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<MeasureUnit> _measureUnitRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IRepository<Log> _logRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<CustomerProduct> _customerProductRepository;
        private readonly IRepository<CustomerProductPrice> _customerProductPriceRepository;
        private readonly IRepository<CustomerTagProduct> _customerTagProductRepository;
        private readonly IRepository<CustomerHistoryPassword> _customerHistoryPasswordRepository;
        private readonly IRepository<CustomerNote> _customerNoteRepository;
        private readonly IRepository<UserApi> _userapiRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReservation> _productReservationRepository;
        private readonly IRepository<ProductAlsoPurchased> _productalsopurchasedRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumPostVote> _forumPostVote;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountCoupon> _discountCouponRepository;
        private readonly IRepository<DiscountUsageHistory> _discountusageRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<NewsLetterSubscription> _newslettersubscriptionRepository;
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IRepository<TopicTemplate> _topicTemplateRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IRepository<RewardPointsHistory> _rewardpointshistoryRepository;
        private readonly IRepository<SearchTerm> _searchtermRepository;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<PickupPoint> _pickupPointsRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<PermissionAction> _permissionAction;
        private readonly IRepository<ExternalAuthenticationRecord> _externalAuthenticationRepository;
        private readonly IRepository<ReturnRequestReason> _returnRequestReasonRepository;
        private readonly IRepository<ReturnRequestAction> _returnRequestActionRepository;
        private readonly IRepository<ContactUs> _contactUsRepository;
        private readonly IRepository<CustomerAction> _customerAction;
        private readonly IRepository<CustomerActionType> _customerActionType;
        private readonly IRepository<CustomerActionHistory> _customerActionHistory;
        private readonly IRepository<PopupArchive> _popupArchive;
        private readonly IRepository<CustomerReminderHistory> _customerReminderHistoryRepository;
        private readonly IRepository<RecentlyViewedProduct> _recentlyViewedProductRepository;
        private readonly IRepository<KnowledgebaseArticle> _knowledgebaseArticleRepository;
        private readonly IRepository<KnowledgebaseCategory> _knowledgebaseCategoryRepository;
        private readonly IRepository<OrderTag> _orderTagRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;
        
        #endregion

        #region Ctor

        public CodeFirstInstallationService(IServiceProvider serviceProvider)
        {
            _versionRepository = serviceProvider.GetRequiredService<IRepository<GrandNodeVersion>>();
            _bidRepository = serviceProvider.GetRequiredService<IRepository<Bid>>();
            _addressRepository = serviceProvider.GetRequiredService<IRepository<Address>>();
            _affiliateRepository = serviceProvider.GetRequiredService<IRepository<Affiliate>>();
            _campaignHistoryRepository = serviceProvider.GetRequiredService<IRepository<CampaignHistory>>();
            _orderRepository = serviceProvider.GetRequiredService<IRepository<Order>>();
            _orderNoteRepository = serviceProvider.GetRequiredService<IRepository<OrderNote>>();
            _storeRepository = serviceProvider.GetRequiredService<IRepository<Store>>();
            _measureDimensionRepository = serviceProvider.GetRequiredService<IRepository<MeasureDimension>>();
            _measureWeightRepository = serviceProvider.GetRequiredService<IRepository<MeasureWeight>>();
            _measureUnitRepository = serviceProvider.GetRequiredService<IRepository<MeasureUnit>>();
            _taxCategoryRepository = serviceProvider.GetRequiredService<IRepository<TaxCategory>>();
            _languageRepository = serviceProvider.GetRequiredService<IRepository<Language>>();
            _lsrRepository = serviceProvider.GetRequiredService<IRepository<LocaleStringResource>>();
            _logRepository = serviceProvider.GetRequiredService<IRepository<Log>>();
            _currencyRepository = serviceProvider.GetRequiredService<IRepository<Currency>>();
            _customerRepository = serviceProvider.GetRequiredService<IRepository<Customer>>();
            _customerRoleRepository = serviceProvider.GetRequiredService<IRepository<CustomerRole>>();
            _customerProductRepository = serviceProvider.GetRequiredService<IRepository<CustomerProduct>>();
            _customerProductPriceRepository = serviceProvider.GetRequiredService<IRepository<CustomerProductPrice>>();
            _customerRoleProductRepository = serviceProvider.GetRequiredService<IRepository<CustomerRoleProduct>>();
            _customerTagProductRepository = serviceProvider.GetRequiredService<IRepository<CustomerTagProduct>>();
            _customerHistoryPasswordRepository = serviceProvider.GetRequiredService<IRepository<CustomerHistoryPassword>>();
            _customerNoteRepository = serviceProvider.GetRequiredService<IRepository<CustomerNote>>();
            _userapiRepository = serviceProvider.GetRequiredService<IRepository<UserApi>>();
            _specificationAttributeRepository = serviceProvider.GetRequiredService<IRepository<SpecificationAttribute>>();
            _checkoutAttributeRepository = serviceProvider.GetRequiredService<IRepository<CheckoutAttribute>>();
            _productAttributeRepository = serviceProvider.GetRequiredService<IRepository<ProductAttribute>>();
            _categoryRepository = serviceProvider.GetRequiredService<IRepository<Category>>();
            _manufacturerRepository = serviceProvider.GetRequiredService<IRepository<Manufacturer>>();
            _productRepository = serviceProvider.GetRequiredService<IRepository<Product>>();
            _productReservationRepository = serviceProvider.GetRequiredService<IRepository<ProductReservation>>();
            _productalsopurchasedRepository = serviceProvider.GetRequiredService<IRepository<ProductAlsoPurchased>>();
            _urlRecordRepository = serviceProvider.GetRequiredService<IRepository<UrlRecord>>();
            _emailAccountRepository = serviceProvider.GetRequiredService<IRepository<EmailAccount>>();
            _messageTemplateRepository = serviceProvider.GetRequiredService<IRepository<MessageTemplate>>();
            _forumGroupRepository = serviceProvider.GetRequiredService<IRepository<ForumGroup>>();
            _forumRepository = serviceProvider.GetRequiredService<IRepository<Forum>>();
            _forumPostVote = serviceProvider.GetRequiredService<IRepository<ForumPostVote>>();
            _countryRepository = serviceProvider.GetRequiredService<IRepository<Country>>();
            _stateProvinceRepository = serviceProvider.GetRequiredService<IRepository<StateProvince>>();
            _discountRepository = serviceProvider.GetRequiredService<IRepository<Discount>>();
            _discountCouponRepository = serviceProvider.GetRequiredService<IRepository<DiscountCoupon>>();
            _blogPostRepository = serviceProvider.GetRequiredService<IRepository<BlogPost>>();
            _topicRepository = serviceProvider.GetRequiredService<IRepository<Topic>>();
            _productReviewRepository = serviceProvider.GetRequiredService<IRepository<ProductReview>>();
            _newsItemRepository = serviceProvider.GetRequiredService<IRepository<NewsItem>>();
            _newslettersubscriptionRepository = serviceProvider.GetRequiredService<IRepository<NewsLetterSubscription>>();
            _pollRepository = serviceProvider.GetRequiredService<IRepository<Poll>>();
            _shippingMethodRepository = serviceProvider.GetRequiredService<IRepository<ShippingMethod>>();
            _deliveryDateRepository = serviceProvider.GetRequiredService<IRepository<DeliveryDate>>();
            _activityLogTypeRepository = serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            _productTagRepository = serviceProvider.GetRequiredService<IRepository<ProductTag>>();
            _productTemplateRepository = serviceProvider.GetRequiredService<IRepository<ProductTemplate>>();
            _recentlyViewedProductRepository = serviceProvider.GetRequiredService<IRepository<RecentlyViewedProduct>>();
            _categoryTemplateRepository = serviceProvider.GetRequiredService<IRepository<CategoryTemplate>>();
            _manufacturerTemplateRepository = serviceProvider.GetRequiredService<IRepository<ManufacturerTemplate>>();
            _topicTemplateRepository = serviceProvider.GetRequiredService<IRepository<TopicTemplate>>();
            _scheduleTaskRepository = serviceProvider.GetRequiredService<IRepository<ScheduleTask>>();
            _returnRequestRepository = serviceProvider.GetRequiredService<IRepository<ReturnRequest>>();
            _returnRequestNoteRepository = serviceProvider.GetRequiredService<IRepository<ReturnRequestNote>>();
            _rewardpointshistoryRepository = serviceProvider.GetRequiredService<IRepository<RewardPointsHistory>>();
            _searchtermRepository = serviceProvider.GetRequiredService<IRepository<SearchTerm>>();
            _settingRepository = serviceProvider.GetRequiredService<IRepository<Setting>>();
            _shipmentRepository = serviceProvider.GetRequiredService<IRepository<Shipment>>();
            _warehouseRepository = serviceProvider.GetRequiredService<IRepository<Warehouse>>();
            _pickupPointsRepository = serviceProvider.GetRequiredService<IRepository<PickupPoint>>();
            _permissionRepository = serviceProvider.GetRequiredService<IRepository<PermissionRecord>>();
            _permissionAction = serviceProvider.GetRequiredService<IRepository<PermissionAction>>();
            _vendorRepository = serviceProvider.GetRequiredService<IRepository<Vendor>>();
            _externalAuthenticationRepository = serviceProvider.GetRequiredService<IRepository<ExternalAuthenticationRecord>>();
            _discountusageRepository = serviceProvider.GetRequiredService<IRepository<DiscountUsageHistory>>();
            _returnRequestReasonRepository = serviceProvider.GetRequiredService<IRepository<ReturnRequestReason>>();
            _contactUsRepository = serviceProvider.GetRequiredService<IRepository<ContactUs>>();
            _returnRequestActionRepository = serviceProvider.GetRequiredService<IRepository<ReturnRequestAction>>();
            _customerAction = serviceProvider.GetRequiredService<IRepository<CustomerAction>>();
            _customerActionType = serviceProvider.GetRequiredService<IRepository<CustomerActionType>>();
            _customerActionHistory = serviceProvider.GetRequiredService<IRepository<CustomerActionHistory>>();
            _customerReminderHistoryRepository = serviceProvider.GetRequiredService<IRepository<CustomerReminderHistory>>();
            _knowledgebaseArticleRepository = serviceProvider.GetRequiredService<IRepository<KnowledgebaseArticle>>();
            _knowledgebaseCategoryRepository = serviceProvider.GetRequiredService<IRepository<KnowledgebaseCategory>>();
            _popupArchive = serviceProvider.GetRequiredService<IRepository<PopupArchive>>();
            _orderTagRepository = serviceProvider.GetRequiredService<IRepository<OrderTag>>();
            _genericAttributeService = serviceProvider.GetRequiredService<IGenericAttributeService>();
            _webHelper = serviceProvider.GetRequiredService<IWebHelper>();
            _hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Utilities

        protected virtual string GetSamplesPath()
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, "content/samples/");
        }


        protected virtual async Task InstallVersion()
        {
            var version = new GrandNodeVersion {
                DataBaseVersion = GrandVersion.SupportedDBVersion
            };
            await _versionRepository.InsertAsync(version);
        }

        protected virtual async Task InstallStores()
        {
            //var storeUrl = "http://www.yourStore.com/";
            var storeUrl = _webHelper.GetStoreLocation(false);
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "Your store name",
                    Shortcut = "Store",
                    Url = storeUrl,
                    SslEnabled = false,
                    Hosts = "yourstore.com,www.yourstore.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "Your company name",
                    CompanyAddress = "21 West 52nd Street",
                    CompanyPhoneNumber = "(123) 456-78901",
                    CompanyVat = null,
                    CompanyEmail = "company@email.com",
                    CompanyHours = "Monday - Sunday / 8:00AM - 6:00PM"
                },
            };

            await _storeRepository.InsertAsync(stores);
        }

        protected virtual async Task InstallMeasures()
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

            await _measureDimensionRepository.InsertAsync(measureDimensions);

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

            await _measureWeightRepository.InsertAsync(measureWeights);

            var measureUnits = new List<MeasureUnit>
            {
                new MeasureUnit
                {
                    Name = "pcs.",
                    DisplayOrder = 1,
                },
                new MeasureUnit
                {
                    Name = "pair",
                    DisplayOrder = 2,
                },
                new MeasureUnit
                {
                    Name = "set",
                    DisplayOrder = 3,
                }
            };

            await _measureUnitRepository.InsertAsync(measureUnits);

        }

        protected virtual async Task InstallTaxCategories()
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
            await _taxCategoryRepository.InsertAsync(taxCategories);

        }

        protected virtual async Task InstallLanguages()
        {
            var language = new Language {
                Name = "English",
                LanguageCulture = "en-US",
                UniqueSeoCode = "en",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 1
            };
            await _languageRepository.InsertAsync(language);
        }

        protected virtual async Task InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/"), "*.grandres.xml", SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
                await localizationService.ImportResourcesFromXmlInstall(language, localesXml);
            }

        }

        protected virtual async Task InstallCurrencies()
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
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.ToEven,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Euro",
                    CurrencyCode = "EUR",
                    Rate = 0.95M,
                    DisplayLocale = "",
                    CustomFormatting = string.Format("{0}0.00", "\u20ac"),
                    Published = true,
                    DisplayOrder = 2,
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.AwayFromZero,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "British Pound",
                    CurrencyCode = "GBP",
                    Rate = 0.82M,
                    DisplayLocale = "en-GB",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 3,
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.AwayFromZero,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Chinese Yuan Renminbi",
                    CurrencyCode = "CNY",
                    Rate = 6.93M,
                    DisplayLocale = "zh-CN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 4,
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.ToEven,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Indian Rupee",
                    CurrencyCode = "INR",
                    Rate = 68.17M,
                    DisplayLocale = "en-IN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 5,
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.ToEven,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
                new Currency
                {
                    Name = "Złoty",
                    CurrencyCode = "PLN",
                    Rate = 3.97M,
                    DisplayLocale = "pl-PL",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 6,
                    RoundingType = RoundingType.Rounding001,
                    MidpointRound = MidpointRounding.AwayFromZero,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                },
            };
            await _currencyRepository.InsertAsync(currencies);
        }

        protected virtual async Task InstallCountriesAndStates()
        {
            var cUsa = new Country {
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
            await _countryRepository.InsertAsync(cUsa);

            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "AA (Armed Forces Americas)",
                Abbreviation = "AA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "AE (Armed Forces Europe)",
                Abbreviation = "AE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Alabama",
                Abbreviation = "AL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Alaska",
                Abbreviation = "AK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "American Samoa",
                Abbreviation = "AS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "AP (Armed Forces Pacific)",
                Abbreviation = "AP",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Arizona",
                Abbreviation = "AZ",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Arkansas",
                Abbreviation = "AR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "California",
                Abbreviation = "CA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Colorado",
                Abbreviation = "CO",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Connecticut",
                Abbreviation = "CT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Delaware",
                Abbreviation = "DE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "District of Columbia",
                Abbreviation = "DC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Federated States of Micronesia",
                Abbreviation = "FM",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Florida",
                Abbreviation = "FL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Georgia",
                Abbreviation = "GA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Guam",
                Abbreviation = "GU",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Hawaii",
                Abbreviation = "HI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Idaho",
                Abbreviation = "ID",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Illinois",
                Abbreviation = "IL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Indiana",
                Abbreviation = "IN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Iowa",
                Abbreviation = "IA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Kansas",
                Abbreviation = "KS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Kentucky",
                Abbreviation = "KY",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Louisiana",
                Abbreviation = "LA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Maine",
                Abbreviation = "ME",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Marshall Islands",
                Abbreviation = "MH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Maryland",
                Abbreviation = "MD",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Massachusetts",
                Abbreviation = "MA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Michigan",
                Abbreviation = "MI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Minnesota",
                Abbreviation = "MN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Mississippi",
                Abbreviation = "MS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Missouri",
                Abbreviation = "MO",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Montana",
                Abbreviation = "MT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Nebraska",
                Abbreviation = "NE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Nevada",
                Abbreviation = "NV",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "New Hampshire",
                Abbreviation = "NH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "New Jersey",
                Abbreviation = "NJ",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "New Mexico",
                Abbreviation = "NM",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "New York",
                Abbreviation = "NY",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "North Carolina",
                Abbreviation = "NC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "North Dakota",
                Abbreviation = "ND",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Northern Mariana Islands",
                Abbreviation = "MP",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Ohio",
                Abbreviation = "OH",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Oklahoma",
                Abbreviation = "OK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Oregon",
                Abbreviation = "OR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Palau",
                Abbreviation = "PW",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Pennsylvania",
                Abbreviation = "PA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Puerto Rico",
                Abbreviation = "PR",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Rhode Island",
                Abbreviation = "RI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "South Carolina",
                Abbreviation = "SC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "South Dakota",
                Abbreviation = "SD",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Tennessee",
                Abbreviation = "TN",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Texas",
                Abbreviation = "TX",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Utah",
                Abbreviation = "UT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Vermont",
                Abbreviation = "VT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Virgin Islands",
                Abbreviation = "VI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Virginia",
                Abbreviation = "VA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Washington",
                Abbreviation = "WA",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "West Virginia",
                Abbreviation = "WV",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Wisconsin",
                Abbreviation = "WI",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cUsa.Id,
                Name = "Wyoming",
                Abbreviation = "WY",
                Published = true,
                DisplayOrder = 1,
            });
            var cCanada = new Country {
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
            await _countryRepository.InsertAsync(cCanada);

            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Alberta",
                Abbreviation = "AB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "British Columbia",
                Abbreviation = "BC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Manitoba",
                Abbreviation = "MB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "New Brunswick",
                Abbreviation = "NB",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Newfoundland and Labrador",
                Abbreviation = "NL",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Northwest Territories",
                Abbreviation = "NT",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Nova Scotia",
                Abbreviation = "NS",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Nunavut",
                Abbreviation = "NU",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Ontario",
                Abbreviation = "ON",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Prince Edward Island",
                Abbreviation = "PE",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Quebec",
                Abbreviation = "QC",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Saskatchewan",
                Abbreviation = "SK",
                Published = true,
                DisplayOrder = 1,
            });
            states.Add(new StateProvince {
                CountryId = cCanada.Id,
                Name = "Yukon Territory",
                Abbreviation = "YT",
                Published = true,
                DisplayOrder = 1,
            });

            await _stateProvinceRepository.InsertAsync(states);

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
            await _countryRepository.InsertAsync(countries);
        }

        protected virtual async Task InstallShippingMethods()
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
            await _shippingMethodRepository.InsertAsync(shippingMethods);
        }

        protected virtual async Task InstallDeliveryDates()
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
            await _deliveryDateRepository.InsertAsync(deliveryDates);
        }

        protected virtual async Task InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new CustomerRole {
                Name = "Administrators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            await _customerRoleRepository.InsertAsync(crAdministrators);

            var crForumModerators = new CustomerRole {
                Name = "Forum Moderators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.ForumModerators,
            };
            await _customerRoleRepository.InsertAsync(crForumModerators);

            var crRegistered = new CustomerRole {
                Name = "Registered",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            await _customerRoleRepository.InsertAsync(crRegistered);

            var crGuests = new CustomerRole {
                Name = "Guests",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };
            await _customerRoleRepository.InsertAsync(crGuests);

            var crVendors = new CustomerRole {
                Name = "Vendors",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Vendors,
            };
            await _customerRoleRepository.InsertAsync(crVendors);

            var crStaff = new CustomerRole {
                Name = "Staff",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Staff,
            };
            await _customerRoleRepository.InsertAsync(crStaff);

            //admin user
            var adminUser = new Customer {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Password = defaultUserPassword,
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = "",
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                PasswordChangeDateUtc = DateTime.UtcNow,
            };
            var defaultAdminUserAddress = new Address {
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = "admin@yourstore.com",
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
            await _customerRepository.InsertAsync(adminUser);

            //set default customer name
            await _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, "John");
            await _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, "Smith");


            //search engine (crawler) built-in user
            var searchEngineUser = new Customer {
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
            await _customerRepository.InsertAsync(searchEngineUser);


            //built-in user for background tasks
            var backgroundTaskUser = new Customer {
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
            await _customerRepository.InsertAsync(backgroundTaskUser);

        }

        protected virtual async Task HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
            var customerRegistrationService = _serviceProvider.GetRequiredService<ICustomerRegistrationService>();
            await customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false, PasswordFormat.Hashed, defaultUserPassword));
        }

        protected virtual async Task InstallCustomerAction()
        {
            var customerActionType = new List<CustomerActionType>()
            {
                new CustomerActionType()
                {
                    Name = "Add to cart",
                    SystemKeyword = "AddToCart",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13 }
                },
                new CustomerActionType()
                {
                    Name = "Add order",
                    SystemKeyword = "AddOrder",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13 }
                },
                new CustomerActionType()
                {
                    Name = "Paid order",
                    SystemKeyword = "PaidOrder",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13 }
                },
                new CustomerActionType()
                {
                    Name = "Viewed",
                    SystemKeyword = "Viewed",
                    Enabled = false,
                    ConditionType = {1, 2, 3, 7, 8, 9, 10, 13}
                },
                new CustomerActionType()
                {
                    Name = "Url",
                    SystemKeyword = "Url",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10, 11, 12, 13}
                },
                new CustomerActionType()
                {
                    Name = "Customer Registration",
                    SystemKeyword = "Registration",
                    Enabled = false,
                    ConditionType = {7, 8, 9, 10, 13}
                }
            };
            await _customerActionType.InsertAsync(customerActionType);

        }

        protected virtual async Task InstallEmailAccounts()
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
                                           SecureSocketOptionsId = 1,
                                           UseServerCertificateValidation = true
                                       },
                               };
            await _emailAccountRepository.InsertAsync(emailAccounts);
        }

        protected virtual async Task InstallMessageTemplates()
        {
            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");

            var OrderProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Order.Products.txt"));
            var OrderVendorProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Order.VendorProducts.txt"));
            var ShipmentProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Shipment.Products.txt"));

            var messageTemplates = new List<MessageTemplate>
                               {
                                    new MessageTemplate
                                       {
                                           Name = "AuctionEnded.CustomerNotificationWin",
                                           Subject = "{{Store.Name}}. Auction ended.",
                                           Body = "<p>Hello, {{Customer.FullName}}!</p><p></p><p>At {{Auctions.EndTime}} you have won <a href=\"{{Store.URL}}{{Auctions.ProductSeName}}\">{{Auctions.ProductName}}</a> for {{Auctions.Price}}. Visit  <a href=\"{{Store.URL}}/cart\">cart</a> to finish checkout process. </p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                            {
                                                Name = "AuctionEnded.CustomerNotificationLost",
                                                Subject = "{{Store.Name}}. Auction ended.",
                                                Body = "<p>Hello, {{Customer.FullName}}!</p><p></p><p>Unfortunately you did not win the bid {{Auctions.ProductName}}</p> <p>End price:  {{Auctions.Price}} </p> <p>End date auction {{Auctions.EndTime}} </p>",
                                                IsActive = true,
                                                EmailAccountId = eaGeneral.Id,
                                            },
                                    new MessageTemplate
                                            {
                                                Name = "AuctionEnded.CustomerNotificationBin",
                                                Subject = "{{Store.Name}}. Auction ended.",
                                                Body = "<p>Hello, {{Customer.FullName}}!</p><p></p><p>Unfortunately you did not win the bid {{Product.Name}}</p> <p>Product was bought by option Buy it now for price: {{Product.Price}} </p>",
                                                IsActive = true,
                                                EmailAccountId = eaGeneral.Id,
                                            },
                                    new MessageTemplate
                                       {
                                           Name = "AuctionEnded.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Auction ended.",
                                           Body = "<p>At {{Auctions.EndTime}} {{Customer.FullName}} have won <a href=\"{{Store.URL}}{{Auctions.ProductSeName}}\">{{Auctions.ProductName}}</a> for {{Auctions.Price}}.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "AuctionExpired.StoreOwnerNotification",
                                           Subject = "Your auction to product {{Product.Name}}  has expired.",
                                           Body = "Hello, <br> Your auction to product {{Product.Name}} has expired without bid.",
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "BidUp.CustomerNotification",
                                           Subject = "{{Store.Name}}. Your offer has been outbid.",
                                           Body = "<p>Hi {{Customer.FullName}}!</p><p>Your offer for product <a href=\"{{Store.URL}}{{Auctions.ProductSeName}}\">{{Auctions.ProductName}}</a> has been outbid. Your price was {{Auctions.Price}}.<br />\r\nRaise a price by raising one's offer. Auction will be ended on {{Auctions.EndTime}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "Blog.BlogComment",
                                           Subject = "{{Store.Name}}. New blog comment.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new blog comment has been created for blog post \"{{BlogComment.BlogPostTitle}}\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Knowledgebase.ArticleComment",
                                           Subject = "{{Store.Name}}. New article comment.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new article comment has been created for article \"{{Knowledgebase.ArticleCommentTitle}}\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.BackInStock",
                                           Subject = "{{Store.Name}}. Back in stock notification",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Customer.FullName}}, <br />\r\nProduct <a target=\"_blank\" href=\"{{BackInStockSubscription.ProductUrl}}\">{{BackInStockSubscription.ProductName}}</a> is in stock.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "CustomerDelete.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Customer has been deleted.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> ,<br />\r\n{{Customer.FullName}} ({{Customer.Email}}) has just deleted from your database. </p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.EmailTokenValidationMessage",
                                           Subject = "{{Store.Name}} - Email Verification Code",
                                           Body = "Hello {{Customer.FullName}}, <br /><br />\r\n Enter this 6 digit code on the sign in page to confirm your identity:<br /><br /> \r\n <b>{{Customer.Token}}</b><br /><br />\r\n Yours securely, <br /> \r\n Team",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.EmailValidationMessage",
                                           Subject = "{{Store.Name}}. Email validation",
                                           Body = "<a href=\"{{Store.URL}}\">{{Store.Name}}</a>  <br />\r\n  <br />\r\n  To activate your account <a href=\"{{Customer.AccountActivationURL}}\">click here</a>.     <br />\r\n  <br />\r\n  {{Store.Name}}",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewPM",
                                           Subject = "{{Store.Name}}. You have received a new private message",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nYou have received a new private message.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.PasswordRecovery",
                                           Subject = "{{Store.Name}}. Password recovery",
                                           Body = "<a href=\"{{Store.URL}}\">{{Store.Name}}</a>  <br />\r\n  <br />\r\n  To change your password <a href=\"{{Customer.PasswordRecoveryURL}}\">click here</a>.     <br />\r\n  <br />\r\n  {{Store.Name}}",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.WelcomeMessage",
                                           Subject = "Welcome to {{Store.Name}}",
                                           Body = "We welcome you to <a href=\"{{Store.URL}}\"> {{Store.Name}}</a>.<br />\r\n<br />\r\nYou can now take part in the various services we have to offer you. Some of these services include:<br />\r\n<br />\r\nPermanent Cart - Any products added to your online cart remain there until you remove them, or check them out.<br />\r\nAddress Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.<br />\r\nOrder History - View your history of purchases that you have made with us.<br />\r\nProducts Reviews - Share your opinions on products with our other customers.<br />\r\n<br />\r\nFor help with any of our online services, please email the store-owner: <a href=\"mailto:{{Store.Email}}\">{{Store.Email}}</a>.<br />\r\n<br />\r\nNote: This email address was provided on our registration page. If you own the email and did not register on our site, please send an email to <a href=\"mailto:{{Store.Email}}\">{{Store.Email}}</a>.",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumPost",
                                           Subject = "{{Store.Name}}. New Post Notification.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new post has been created in the topic <a href=\"{{Forums.TopicURL}}\">\"{{Forums.TopicName}}\"</a> at <a href=\"{{Forums.ForumURL}}\">\"{{Forums.ForumName}}\"</a> forum.<br />\r\n<br />\r\nClick <a href=\"{{Forums.TopicURL}}\">here</a> for more info.<br />\r\n<br />\r\nPost author: {{Forums.PostAuthor}}<br />\r\nPost body: {{Forums.PostBody}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumTopic",
                                           Subject = "{{Store.Name}}. New Topic Notification.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new topic <a href=\"{{Forums.TopicURL}}\">\"{{Forums.TopicName}}\"</a> has been created at <a href=\"{{Forums.ForumURL}}\">\"{{Forums.ForumName}}\"</a> forum.<br />\r\n<br />\r\nClick <a href=\"{{Forums.TopicURL}}\">here</a> for more info.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "GiftCard.Notification",
                                           Subject = "{{GiftCard.SenderName}} has sent you a gift card for {{Store.Name}}",
                                           Body = "<p>You have received a gift card for {{Store.Name}}</p><p>Dear {{GiftCard.RecipientName}}, <br />\r\n<br />\r\n{{GiftCard.SenderName}} ({{GiftCard.SenderEmail}}) has sent you a {{GiftCard.Amount}} gift cart for <a href=\"{{Store.URL}}\"> {{Store.Name}}</a></p><p>You gift card code is {{GiftCard.CouponCode}}</p><p>{{GiftCard.Message}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewCustomer.Notification",
                                           Subject = "{{Store.Name}}. New customer registration",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new customer registered with your store. Below are the customer's details:<br />\r\nFull name: {{Customer.FullName}}<br />\r\nEmail: {{Customer.Email}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewReturnRequest.CustomerNotification",
                                           Subject = "{{Store.Name}}. New return request.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Customer.FullName}}!<br />\r\n You have just submitted a new return request. Details are below:<br />\r\nRequest ID: {{ReturnRequest.Id}}<br />\r\nProducts:<br />\r\n{{ReturnRequest.Products}}<br />\r\nCustomer comments: {{ReturnRequest.CustomerComment}}<br />\r\n<br />\r\nPickup date: {{ReturnRequest.PickupDate}}<br />\r\n<br />\r\nPickup address:<br />\r\n{{ReturnRequest.PickupAddressFirstName}} {{ReturnRequest.PickupAddressLastName}}<br />\r\n{{ReturnRequest.PickupAddressAddress1}}<br />\r\n{{ReturnRequest.PickupAddressCity}} {{ReturnRequest.PickupAddressZipPostalCode}}<br />\r\n{{ReturnRequest.PickupAddressStateProvince}} {{ReturnRequest.PickupAddressCountry}}<br />\r\n</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewReturnRequest.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. New return request.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Customer.FullName}} has just submitted a new return request. Details are below:<br />\r\nRequest ID: {{ReturnRequest.Id}}<br />\r\nProducts:<br />\r\n{{ReturnRequest.Products}}<br />\r\nCustomer comments: {{ReturnRequest.CustomerComment}}<br />\r\n<br />\r\nPickup date: {{ReturnRequest.PickupDate}}<br />\r\n<br />\r\nPickup address:<br />\r\n{{ReturnRequest.PickupAddressFirstName}} {{ReturnRequest.PickupAddressLastName}}<br />\r\n{{ReturnRequest.PickupAddressAddress1}}<br />\r\n{{ReturnRequest.PickupAddressCity}} {{ReturnRequest.PickupAddressZipPostalCode}}<br />\r\n{{ReturnRequest.PickupAddressStateProvince}} {{ReturnRequest.PickupAddressCountry}}<br />\r\n</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "News.NewsComment",
                                           Subject = "{{Store.Name}}. New news comment.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new news comment has been created for news \"{{NewsComment.NewsTitle}}\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.ActivationMessage",
                                           Subject = "{{Store.Name}}. Subscription activation message.",
                                           Body = "<p><a href=\"{{NewsLetterSubscription.ActivationUrl}}\">Click here to confirm your subscription to our list.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.DeactivationMessage",
                                           Subject = "{{Store.Name}}. Subscription deactivation message.",
                                           Body = "<p><a href=\"{{NewsLetterSubscription.DeactivationUrl}}\">Click here to unsubscribe from our newsletter.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewVATSubmitted.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. New VAT number is submitted.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Customer.FullName}} ({{Customer.Email}}) has just submitted a new VAT number. Details are below:<br />\r\nVAT number: {{Customer.VatNumber}}<br />\r\nVAT number status: {{Customer.VatNumberStatus}}<br />\r\nReceived name: {{VatValidationResult.Name}}<br />\r\nReceived address: {{VatValidationResult.Address}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                  new MessageTemplate
                                       {
                                           Name = "OrderCancelled.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Customer cancelled an order",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n<br />\r\nCustomer cancelled an order. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a target=\"_blank\" href=\"{{Order.OrderURLForCustomer}}\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "OrderCancelled.CustomerNotification",
                                           Subject = "{{Store.Name}}. Your order cancelled",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}, <br />\r\nYour order has been cancelled. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a target=\"_blank\" href=\"{{Order.OrderURLForCustomer}}\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "OrderCancelled.VendorNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} cancelled",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br /><br />Order #{{Order.OrderNumber}} has been cancelled. <br /><br />Order Number: {{Order.OrderNumber}} <br />   Date Ordered: {{Order.CreatedOn}} <br /><br /> ",
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "OrderCompleted.CustomerNotification",
                                           Subject = "{{Store.Name}}. Your order completed",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}, <br />\r\nYour order has been completed. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a target=\"_blank\" href=\"{{Order.OrderURLForCustomer}}\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentDelivered.CustomerNotification",
                                           Subject = "Your order from {{Store.Name}} has been delivered.",
                                           Body = "<p><a href=\"{{Store.URL}}\"> {{Store.Name}}</a> <br />\r\n <br />\r\n Hello {{Order.CustomerFullName}}, <br />\r\n Good news! You order has been delivered. <br />\r\n Order Number: {{Order.OrderNumber}}<br />\r\n Order Details: <a href=\"{{Order.OrderURLForCustomer}}\" target=\"_blank\">{{Order.OrderURLForCustomer}}</a><br />\r\n Date Ordered: {{Order.CreatedOn}}<br />\r\n <br />\r\n <br />\r\n <br />\r\n Billing Address<br />\r\n {{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n {{Order.BillingAddress1}}<br />\r\n {{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n {{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n <br />\r\n <br />\r\n <br />\r\n Shipping Address<br />\r\n {{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n {{Order.ShippingAddress1}}<br />\r\n {{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n {{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n <br />\r\n Shipping Method: {{Order.ShippingMethod}} <br />\r\n <br />\r\n Delivered Products: <br />\r\n <br />\r\n" + ShipmentProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.CustomerNotification",
                                           Subject = "Order receipt from {{Store.Name}}.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}, <br />\r\nThanks for buying from <a href=\"{{Store.URL}}\">{{Store.Name}}</a>. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a target=\"_blank\" href=\"{{Order.OrderURLForCustomer}}\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Purchase Receipt for Order #{{Order.OrderNumber}}",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Order.CustomerFullName}} ({{Order.CustomerEmail}}) has just placed an order from your store. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentSent.CustomerNotification",
                                           Subject = "Your order from {{Store.Name}} has been shipped.",
                                           Body = "<p><a href=\"{{Store.URL}}\"> {{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}!, <br />\r\nGood news! You order has been shipped. <br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a href=\"{{Order.OrderURLForCustomer}}\" target=\"_blank\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}} <br />\r\n <br />\r\n Shipped Products: <br />\r\n <br />\r\n" + ShipmentProducts + "</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Product.ProductReview",
                                           Subject = "{{Store.Name}}. New product review.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new product review has been written for product \"{{ProductReview.ProductName}}\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "QuantityBelow.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Quantity below notification. {{Product.Name}}",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Product.Name}} (ID: {{Product.Id}}) low quantity. <br />\r\n<br />\r\nQuantity: {{Product.StockQuantity}}<br />\r\n</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "QuantityBelow.AttributeCombination.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Quantity below notification. {{Product.Name}}",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Product.Name}} (ID: {{Product.Id}}) low quantity. <br />\r\n{{AttributeCombination.Formatted}}<br />\r\nQuantity: {{AttributeCombination.StockQuantity}}<br />\r\n</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ReturnRequestStatusChanged.CustomerNotification",
                                           Subject = "{{Store.Name}}. Return request status was changed.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Customer.FullName}},<br />\r\nYour return request #{{ReturnRequest.Id}} status has been changed.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.EmailAFriend",
                                           Subject = "{{Store.Name}}. Referred Item",
                                           Body = "<p><a href=\"{{Store.URL}}\"> {{Store.Name}}</a> <br />\r\n<br />\r\n{{EmailAFriend.Email}} was shopping on {{Store.Name}} and wanted to share the following item with you. <br />\r\n<br />\r\n<b><a target=\"_blank\" href=\"{{Product.ProductURLForCustomer}}\">{{Product.Name}}</a></b> <br />\r\n{{Product.ShortDescription}} <br />\r\n<br />\r\nFor more info click <a target=\"_blank\" href=\"{{Product.ProductURLForCustomer}}\">here</a> <br />\r\n<br />\r\n<br />\r\n{{EmailAFriend.PersonalMessage}}<br />\r\n<br />\r\n{{Store.Name}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.AskQuestion",
                                           Subject = "{{Store.Name}}. Question about a product",
                                           Body = "<p><a href=\"{{Store.URL}}\"> {{Store.Name}}</a> <br />\r\n<br />\r\n{{AskQuestion.Email}} wanted to ask question about a product {{Product.Name}}. <br />\r\n<br />\r\n<b><a target=\"_blank\" href=\"{{Product.ProductURLForCustomer}}\">{{Product.Name}}</a></b> <br />\r\n{{Product.ShortDescription}} <br />\r\n{{AskQuestion.Message}}<br />\r\n {{AskQuestion.Email}} <br />\r\n {{AskQuestion.FullName}} <br />\r\n {{AskQuestion.Phone}} <br />\r\n{{Store.Name}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.ContactUs",
                                           Subject = "{{Store.Name}}. Contact us",
                                           Body = "<p>From {{ContactUs.SenderName}} - {{ContactUs.SenderEmail}}<br /><br />{{ContactUs.Body}}<br />{{ContactUs.AttributeDescription}}</p><br />",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.ContactVendor",
                                           Subject = "{{Store.Name}}. Contact us",
                                           Body = "<p>From {{ContactUs.SenderName}} - {{ContactUs.SenderEmail}}<br /><br />{{ContactUs.Body}}</p><br />",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },

                                   new MessageTemplate
                                       {
                                           Name = "Wishlist.EmailAFriend",
                                           Subject = "{{Store.Name}}. Wishlist",
                                           Body = "<p><a href=\"{{Store.URL}}\"> {{Store.Name}}</a> <br />\r\n<br />\r\n{{ShoppingCart.WishlistEmail}} was shopping on {{Store.Name}} and wanted to share a wishlist with you. <br />\r\n<br />\r\n<br />\r\nFor more info click <a target=\"_blank\" href=\"{{ShoppingCart.WishlistURLForCustomer}}\">here</a> <br />\r\n<br />\r\n<br />\r\n{{ShoppingCart.WishlistPersonalMessage}}<br />\r\n<br />\r\n{{Store.Name}}</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewOrderNote",
                                           Subject = "{{Store.Name}}. New order note has been added",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Customer.FullName}}, <br />\r\nNew order note has been added to your account:<br />\r\n\"{{Order.NewNoteText}}\".<br />\r\n<a target=\"_blank\" href=\"{{Order.OrderURLForCustomer}}\">{{Order.OrderURLForCustomer}}</a></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewCustomerNote",
                                           Subject = "New customer note has been added",
                                           Body = "<p><br />\r\nHello {{Customer.FullName}}, <br />\r\nNew customer note has been added to your account:<br />\r\n\"{{Customer.NewTitleText}}\".<br />\r\n</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                     new MessageTemplate
                                       {
                                           Name = "Customer.NewReturnRequestNote",
                                           Subject = "{{Store.Name}}. New return request note has been added",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Customer.FullName}},<br />\r\nYour return request #{{ReturnRequest.ReturnNumber}} has a new note.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "RecurringPaymentCancelled.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Recurring payment cancelled",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Customer.FullName}} ({{Customer.Email}}) has just cancelled a recurring payment ID={{RecurringPayment.ID}}.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.VendorNotification",
                                           Subject = "{{Store.Name}}. Order placed",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Customer.FullName}} ({{Customer.Email}}) has just placed an order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n" + OrderVendorProducts + "</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} paid",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nOrder #{{Order.OrderNumber}} has been just paid<br />\r\nDate Ordered: {{Order.CreatedOn}}</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.CustomerNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} paid",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}, <br />\r\nThanks for buying from <a href=\"{{Store.URL}}\">{{Store.Name}}</a>. Order #{{Order.OrderNumber}} has been just paid. Below is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a href=\"{{Order.OrderURLForCustomer}}\" target=\"_blank\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPaid.VendorNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} paid",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nOrder #{{Order.OrderNumber}} has been just paid. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n" + OrderVendorProducts + "</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                        {
                                           Name = "OrderRefunded.CustomerNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} refunded",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nHello {{Order.CustomerFullName}}, <br />\r\nThanks for buying from <a href=\"{{Store.URL}}\">{{Store.Name}}</a>. Order #{{Order.OrderNumber}} has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.<br />\r\n<br />\r\nAmount refunded: {{Order.AmountRefunded}}<br />\r\n<br />\r\nBelow is the summary of the order. <br />\r\n<br />\r\nOrder Number: {{Order.OrderNumber}}<br />\r\nOrder Details: <a href=\"{{Order.OrderURLForCustomer}}\" target=\"_blank\">{{Order.OrderURLForCustomer}}</a><br />\r\nDate Ordered: {{Order.CreatedOn}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nBilling Address<br />\r\n{{Order.BillingFirstName}} {{Order.BillingLastName}}<br />\r\n{{Order.BillingAddress1}}<br />\r\n{{Order.BillingCity}} {{Order.BillingZipPostalCode}}<br />\r\n{{Order.BillingStateProvince}} {{Order.BillingCountry}}<br />\r\n<br />\r\n<br />\r\n<br />\r\nShipping Address<br />\r\n{{Order.ShippingFirstName}} {{Order.ShippingLastName}}<br />\r\n{{Order.ShippingAddress1}}<br />\r\n{{Order.ShippingCity}} {{Order.ShippingZipPostalCode}}<br />\r\n{{Order.ShippingStateProvince}} {{Order.ShippingCountry}}<br />\r\n<br />\r\nShipping Method: {{Order.ShippingMethod}}<br />\r\n<br />\r\n" + OrderProducts + "</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                        {
                                           Name = "OrderRefunded.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} refunded",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nOrder #{{Order.OrderNumber}} has been just refunded<br />\r\n<br />\r\nAmount refunded: {{Order.AmountRefunded}}<br />\r\n<br />\r\nDate Ordered: {{Order.CreatedOn}}</p>",
                                           //this template is disabled by default
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "VendorAccountApply.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. New vendor account submitted.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Customer.FullName}} ({{Customer.Email}}) has just submitted for a vendor account. Details are below:<br />\r\nVendor name: {{Vendor.Name}}<br />\r\nVendor email: {{Vendor.Email}}<br />\r\n<br />\r\nYou can activate it in admin area.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       {
                                           Name = "Vendor.VendorReview",
                                           Subject = "{{Store.Name}}. New vendor review.",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\nA new vendor review has been written.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                    new MessageTemplate
                                       { 
                                           Name = "VendorInformationChange.StoreOwnerNotification",
                                           Subject = "{{Store.Name}}. Vendor {{Vendor.Name}} changed provided information",
                                           Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br />\r\n<br />\r\n{{Vendor.Name}} changed provided information.</p>",
                                           IsActive = false,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                               };
            await _messageTemplateRepository.InsertAsync(messageTemplates);
        }

        protected virtual async Task InstallTopics()
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
                                           IncludeInFooterRow1 = true,
                                           DisplayOrder = 20,
                                           Title = "About us",
                                           Body = "<p>Put your &quot;About Us&quot; information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "CheckoutAsGuestOrRegister",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p><strong>Register and save time!</strong><br />Register with us for future convenience:</p><ul><li>Fast and easy check out</li><li>Easy access to your order history and status</li></ul>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "ConditionsOfUse",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterRow1 = true,
                                           DisplayOrder = 15,
                                           Title = "Conditions of Use",
                                           Body = "<p>Put your conditions of use information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "ContactUs",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Put your contact information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "ForumWelcomeMessage",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "Forums",
                                           Body = "<p>Put your welcome message here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "HomePageText",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "Welcome to our store",
                                           Body = "<p>Online shopping is the process consumers go through to purchase products or services over the Internet. You can edit this in the admin site.</p><p>If you have questions, see the <a href=\"http://www.grandnode.com/\">Documentation</a>, or post in the <a href=\"http://www.grandnode.com/boards/\">Forums</a> at <a href=\"http://www.grandnode.com\">grandnode.com</a></p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "LoginRegistrationInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "About login / registration",
                                           Body = "<p>Put your login / registration information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "PrivacyInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterRow1 = true,
                                           DisplayOrder = 10,
                                           Title = "Privacy notice",
                                           Body = "<p>Put your privacy policy information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "PageNotFound",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p><strong>The page you requested was not found, and we have a fine guess why.</strong></p><ul><li>If you typed the URL directly, please make sure the spelling is correct.</li><li>The page no longer exists. In this case, we profusely apologize for the inconvenience and for any damage this may cause.</li></ul>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "ShippingInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           IncludeInFooterRow1 = true,
                                           DisplayOrder = 5,
                                           Title = "Shipping & returns",
                                           Body = "<p>Put your shipping &amp; returns information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "ApplyVendor",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Put your apply vendor instructions here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "VendorTermsOfService",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Put your terms of service information here. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                                   new Topic
                                       {
                                           SystemName = "KnowledgebaseHomePage",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p>Knowledgebase homepage. You can edit this in the admin site.</p>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true,
                                       },
                               };
            await _topicRepository.InsertAsync(topics);

            var ltopics = from p in _topicRepository.Table
                          select p;
            //search engine names
            foreach (var topic in ltopics)
            {
                var seName = topic.SystemName.ToLowerInvariant();
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = topic.Id,
                    EntityName = "Topic",
                    LanguageId = "",
                    IsActive = true,
                    Slug = seName
                });
                topic.SeName = seName;
                await _topicRepository.UpdateAsync(topic);
            }

        }

        protected virtual async Task InstallSettings(bool installSampleData)
        {
            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();

            await _settingService.SaveSetting(new MenuItemSettings {
                DisplayHomePageMenu = !installSampleData,
                DisplayNewProductsMenu = !installSampleData,
                DisplaySearchMenu = !installSampleData,
                DisplayCustomerMenu = !installSampleData,
                DisplayBlogMenu = !installSampleData,
                DisplayForumsMenu = !installSampleData,
                DisplayContactUsMenu = !installSampleData
            });

            await _settingService.SaveSetting(new PdfSettings {
                LogoPictureId = "",
                InvoiceHeaderText = null,
                InvoiceFooterText = null,
            });

            await _settingService.SaveSetting(new CommonSettings {
                StoreInDatabaseContactUsForm = true,
                UseSystemEmailForContactUsForm = true,
                UseStoredProceduresIfSupported = true,
                SitemapEnabled = true,
                SitemapIncludeCategories = true,
                SitemapIncludeManufacturers = true,
                SitemapIncludeProducts = false,
                DisplayJavaScriptDisabledWarning = false,
                UseFullTextSearch = false,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                DeleteGuestTaskOlderThanMinutes = 1440,
                PopupForTermsOfServiceLinks = true,
                AllowToSelectStore = false,
            });
            await _settingService.SaveSetting(new SecuritySettings {
                EncryptionKey = CommonHelper.GenerateRandomDigitCode(24),
                AdminAreaAllowedIpAddresses = null,
                EnableXsrfProtectionForAdminArea = true,
                EnableXsrfProtectionForPublicStore = true,
                HoneypotEnabled = false,
                HoneypotInputName = "hpinput",
                AllowNonAsciiCharInHeaders = true,
            });
            await _settingService.SaveSetting(new MediaSettings {
                AvatarPictureSize = 120,
                BlogThumbPictureSize = 450,
                ProductThumbPictureSize = 415,
                ProductDetailsPictureSize = 550,
                ProductThumbPictureSizeOnProductDetailsPage = 100,
                AssociatedProductPictureSize = 220,
                CategoryThumbPictureSize = 450,
                ManufacturerThumbPictureSize = 420,
                VendorThumbPictureSize = 450,
                CourseThumbPictureSize = 200,
                LessonThumbPictureSize = 64,
                CartThumbPictureSize = 80,
                MiniCartThumbPictureSize = 100,
                AddToCartThumbPictureSize = 200,
                AutoCompleteSearchThumbPictureSize = 50,
                ImageSquarePictureSize = 32,
                MaximumImageSize = 1980,
                DefaultPictureZoomEnabled = true,
                DefaultImageQuality = 80,
                MultipleThumbDirectories = false,
                StoreLocation = "/",
                StoreInDb = true
            });

            await _settingService.SaveSetting(new SeoSettings {
                PageTitleSeparator = ". ",
                PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                DefaultTitle = "Your store",
                DefaultMetaKeywords = "",
                DefaultMetaDescription = "",
                GenerateProductMetaDescription = true,
                ConvertNonWesternChars = false,
                SeoCharConversion = "ą:a;ę:e;ó:o;ć:c;ł:l;ś:s;ź:z;ż:z",
                AllowUnicodeCharsInUrls = true,
                CanonicalUrlsEnabled = false,
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
                        "knowledgebase",
                        "boards",
                        "inboxupdate",
                        "sentupdate",
                        "news",
                        "sitemap",
                        "search",
                        "config",
                        "eucookielawaccept",
                        "page-not-found",
                        //system names are not allowed (anyway they will cause a runtime error),
                        "con",
                        "lpt1",
                        "lpt2",
                        "lpt3",
                        "lpt4",
                        "lpt5",
                        "lpt6",
                        "lpt7",
                        "lpt8",
                        "lpt9",
                        "com1",
                        "com2",
                        "com3",
                        "com4",
                        "com5",
                        "com6",
                        "com7",
                        "com8",
                        "com9",
                        "null",
                        "prn",
                        "aux"
                    },
            });

            await _settingService.SaveSetting(new AdminAreaSettings {
                DefaultGridPageSize = 15,
                GridPageSizes = "10, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false,
                UseIsoDateTimeConverterInJson = true,
            });

            await _settingService.SaveSetting(new CatalogSettings {
                AllowViewUnpublishedProductPage = true,
                DisplayDiscontinuedMessageForUnpublishedProducts = true,
                PublishBackProductWhenCancellingOrders = false,
                ShowSkuOnProductDetailsPage = false,
                ShowSkuOnCatalogPages = false,
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
                ShowShareButton = false,
                PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_inline_share_toolbox\"></div><script type=\"text/javascript\" src=\"//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-5bbf4b026e74abf6\"></script><!-- AddThis Button END -->",
                ProductReviewsMustBeApproved = false,
                DefaultProductRatingValue = 5,
                AllowAnonymousUsersToReviewProduct = false,
                ProductReviewPossibleOnlyAfterPurchasing = false,
                NotifyStoreOwnerAboutNewProductReviews = false,
                EmailAFriendEnabled = true,
                AskQuestionEnabled = false,
                AskQuestionOnProduct = true,
                AllowAnonymousUsersToEmailAFriend = false,
                RecentlyViewedProductsNumber = 3,
                RecentlyViewedProductsEnabled = true,
                RecommendedProductsEnabled = false,
                SuggestedProductsEnabled = false,
                SuggestedProductsNumber = 6,
                PersonalizedProductsEnabled = false,
                PersonalizedProductsNumber = 6,
                NewProductsNumber = 6,
                NewProductsEnabled = true,
                NewProductsOnHomePage = false,
                NewProductsNumberOnHomePage = 6,
                CompareProductsEnabled = true,
                CompareProductsNumber = 4,
                ProductSearchAutoCompleteEnabled = true,
                ProductSearchAutoCompleteNumberOfProducts = 10,
                ProductSearchTermMinimumLength = 3,
                ShowProductImagesInSearchAutoComplete = true,
                ShowBestsellersOnHomepage = false,
                NumberOfBestsellersOnHomepage = 4,
                PeriodBestsellers = 6,
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
                CustomerProductPrice = false,
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
                DefaultManufacturerPageSize = 6,
                LimitOfFeaturedProducts = 30,
            });

            await _settingService.SaveSetting(new LocalizationSettings {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.Name == "English").Id,
                UseImagesForLanguageSelection = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                IgnoreRtlPropertyForAdminArea = false,
            });

            await _settingService.SaveSetting(new CustomerSettings {
                UsernamesEnabled = false,
                CheckUsernameAvailabilityEnabled = false,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = "SHA1",
                PasswordMinLength = 6,
                PasswordRecoveryLinkDaysValid = 7,
                PasswordLifetime = 90,
                FailedPasswordAllowedAttempts = 0,
                FailedPasswordLockoutMinutes = 30,
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
                HideAuctionsTab = true,
                HideNotesTab = true,
                HideDocumentsTab = true,
                DownloadableProductsValidateUser = true,
                CustomerNameFormat = CustomerNameFormat.ShowFirstName,
                GenderEnabled = false,
                DateOfBirthEnabled = false,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = 0,
                CompanyEnabled = false,
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
                OnlineShoppingCartMinutes = 60,
                StoreLastVisitedPage = false,
                SaveVisitedPage = false,
                SuffixDeletedCustomers = true,
                AllowUsersToDeleteAccount = false,
                AllowUsersToExportData = false,
                HideReviewsTab = false,
                HideCoursesTab = true,
                HideSubAccountsTab = true,
                TwoFactorAuthenticationEnabled = false,
            });

            await _settingService.SaveSetting(new AddressSettings {
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
                FaxEnabled = false,
            });

            await _settingService.SaveSetting(new StoreInformationSettings {
                StoreClosed = false,
                DefaultStoreTheme = "DefaultClean",
                AllowCustomerToSelectTheme = false,
                DisplayEuCookieLawWarning = false,
                FacebookLink = "https://www.facebook.com/grandnodecom",
                TwitterLink = "https://twitter.com/grandnode",
                YoutubeLink = "http://www.youtube.com/user/grandnode",
                InstagramLink = "https://www.instagram.com/grandnode/",
                LinkedInLink = "https://www.linkedin.com/company/grandnode.com/",
                PinterestLink = "",
                HidePoweredByGrandNode = false
            });

            await _settingService.SaveSetting(new ExternalAuthenticationSettings {
                AutoRegisterEnabled = true,
                RequireEmailValidation = false
            });

            await _settingService.SaveSetting(new RewardPointsSettings {
                Enabled = true,
                ExchangeRate = 1,
                PointsForRegistration = 0,
                PointsForPurchases_Amount = 10,
                PointsForPurchases_Points = 1,
                PointsForPurchases_Awarded = OrderStatus.Complete,
                ReduceRewardPointsAfterCancelOrder = true,
                DisplayHowMuchWillBeEarned = true,
                PointsAccumulatedForAllStores = true,
            });

            await _settingService.SaveSetting(new CurrencySettings {
                DisplayCurrencyLabel = false,
                PrimaryStoreCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                AutoUpdateEnabled = false
            });

            await _settingService.SaveSetting(new MeasureSettings {
                BaseDimensionId = _measureDimensionRepository.Table.Single(m => m.SystemKeyword == "inches").Id,
                BaseWeightId = _measureWeightRepository.Table.Single(m => m.SystemKeyword == "lb").Id,
            });

            await _settingService.SaveSetting(new MessageTemplatesSettings {
                CaseInvariantReplacement = false,
                Color1 = "#b9babe",
                Color2 = "#ebecee",
                Color3 = "#dde2e6",
                PictureSize = 50,
            });

            await _settingService.SaveSetting(new ShoppingCartSettings {
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

            await _settingService.SaveSetting(new OrderSettings {
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
                ReturnRequests_AllowToSpecifyPickupAddress = false,
                ReturnRequests_AllowToSpecifyPickupDate = false,
                ReturnRequests_PickupDateRequired = false,
                NumberOfDaysReturnRequestAvailable = 365,
                MinimumOrderPlacementInterval = 30,
                DeactivateGiftCardsAfterDeletingOrder = false,
                CompleteOrderWhenDelivered = true,
                UserCanCancelUnpaidOrder = false,
                LengthCode = 8
            });

            await _settingService.SaveSetting(new ShippingSettings {
                ActiveShippingRateComputationMethodSystemNames = new List<string> { "Shipping.FixedRate" },
                ShipToSameAddress = false,
                AllowPickUpInStore = true,
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

            await _settingService.SaveSetting(new PaymentSettings {
                ActivePaymentMethodSystemNames = new List<string>
                    {
                        "Payments.CheckMoneyOrder",
                        "Payments.CashOnDelivery",
                        "Payments.PayInStore",
                    },
                AllowRePostingPayments = true,
                BypassPaymentMethodSelectionIfOnlyOne = true,
                ShowPaymentMethodDescriptions = true,
                SkipPaymentInfoStepForRedirectionPaymentMethods = false,
            });

            await _settingService.SaveSetting(new TaxSettings {
                TaxBasedOn = TaxBasedOn.BillingAddress,
                TaxDisplayType = TaxDisplayType.ExcludingTax,
                ActiveTaxProviderSystemName = "Tax.FixedRate",
                DefaultTaxAddressId = "",
                DisplayTaxSuffix = false,
                DisplayTaxRates = false,
                PricesIncludeTax = false,
                AllowCustomersToSelectTaxDisplayType = false,
                ForceTaxExclusionFromOrderSubtotal = false,
                HideZeroTax = false,
                HideTaxInOrderSummary = false,
                DefaultTaxCategoryId = "",
                ShippingIsTaxable = false,
                ShippingPriceIncludesTax = false,
                ShippingTaxClassId = "",
                PaymentMethodAdditionalFeeIsTaxable = false,
                PaymentMethodAdditionalFeeIncludesTax = false,
                PaymentMethodAdditionalFeeTaxClassId = "",
                EuVatEnabled = false,
                EuVatShopCountryId = "",
                EuVatAllowVatExemption = true,
                EuVatUseWebService = false,
                EuVatAssumeValid = false,
                EuVatEmailAdminWhenNewVatSubmitted = false
            });

            await _settingService.SaveSetting(new DateTimeSettings {
                DefaultStoreTimeZoneId = "",
                AllowCustomersToSetTimeZone = false
            });

            await _settingService.SaveSetting(new BlogSettings {
                Enabled = true,
                PostsPageSize = 10,
                AllowNotRegisteredUsersToLeaveComments = false,
                NotifyAboutNewBlogComments = false,
                NumberOfTags = 15,
                ShowHeaderRssUrl = false,
                ShowBlogOnHomePage = false,
                HomePageBlogCount = 3,
                MaxTextSizeHomePage = 200
            });

            await _settingService.SaveSetting(new KnowledgebaseSettings {
                Enabled = false,
                AllowNotRegisteredUsersToLeaveComments = false,
                NotifyAboutNewArticleComments = false
            });

            await _settingService.SaveSetting(new PushNotificationsSettings {
                Enabled = false,
                AllowGuestNotifications = true
            });

            await _settingService.SaveSetting(new AdminSearchSettings {
                BlogsDisplayOrder = 0,
                CategoriesDisplayOrder = 0,
                CustomersDisplayOrder = 0,
                ManufacturersDisplayOrder = 0,
                MaxSearchResultsCount = 10,
                MinSearchTermLength = 3,
                NewsDisplayOrder = 0,
                OrdersDisplayOrder = 0,
                ProductsDisplayOrder = 0,
                SearchInBlogs = true,
                SearchInCategories = true,
                SearchInCustomers = true,
                SearchInManufacturers = true,
                SearchInNews = true,
                SearchInOrders = true,
                SearchInProducts = true,
                SearchInTopics = true,
                TopicsDisplayOrder = 0,
                SearchInMenu = true,
                MenuDisplayOrder = -1
            });

            await _settingService.SaveSetting(new NewsSettings {
                Enabled = true,
                AllowNotRegisteredUsersToLeaveComments = false,
                NotifyAboutNewNewsComments = false,
                ShowNewsOnMainPage = true,
                MainPageNewsCount = 3,
                NewsArchivePageSize = 10,
                ShowHeaderRssUrl = false,
            });

            await _settingService.SaveSetting(new ForumSettings {
                ForumsEnabled = false,
                RelativeDateTimeFormattingEnabled = true,
                AllowCustomersToDeletePosts = false,
                AllowCustomersToEditPosts = false,
                AllowCustomersToManageSubscriptions = false,
                AllowGuestsToCreatePosts = false,
                AllowGuestsToCreateTopics = false,
                AllowPostVoting = true,
                MaxVotesPerDay = 30,
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

            await _settingService.SaveSetting(new VendorSettings {
                DefaultVendorPageSizeOptions = "6, 3, 9",
                VendorsBlockItemsToDisplay = 0,
                ShowVendorOnProductDetailsPage = true,
                AllowCustomersToContactVendors = true,
                AllowCustomersToApplyForVendorAccount = false,
                AllowAnonymousUsersToReviewVendor = false,
                DefaultVendorRatingValue = 5,
                VendorReviewsMustBeApproved = true,
                VendorReviewPossibleOnlyAfterPurchasing = true,
                NotifyVendorAboutNewVendorReviews = true,
            });

            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            await _settingService.SaveSetting(new EmailAccountSettings {
                DefaultEmailAccountId = eaGeneral.Id
            });

            await _settingService.SaveSetting(new WidgetSettings {
                ActiveWidgetSystemNames = new List<string> { "Widgets.Slider" },
            });

            await _settingService.SaveSetting(new GoogleAnalyticsSettings() {
                gaprivateKey = "",
                gaserviceAccountEmail = "",
                gaviewID = ""
            });
        }

        protected virtual async Task InstallCheckoutAttributes()
        {
            var ca1 = new CheckoutAttribute {
                Name = "Gift wrapping",
                IsRequired = true,
                ShippableProductRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1,
            };
            await _checkoutAttributeRepository.InsertAsync(ca1);
            ca1.CheckoutAttributeValues.Add(new CheckoutAttributeValue {
                Name = "No",
                PriceAdjustment = 0,
                DisplayOrder = 1,
                IsPreSelected = true,
                CheckoutAttributeId = ca1.Id,
            });

            ca1.CheckoutAttributeValues.Add(new CheckoutAttributeValue {
                Name = "Yes",
                PriceAdjustment = 10,
                DisplayOrder = 2,
                CheckoutAttributeId = ca1.Id,
            });
            await _checkoutAttributeRepository.UpdateAsync(ca1);
        }

        protected virtual async Task InstallSpecificationAttributes()
        {
            var sa1 = new SpecificationAttribute {
                Name = "Screensize",
                DisplayOrder = 1,
                SeName = SeoExtensions.GetSeName("Screensize", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa1);

            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "13.0''",
                DisplayOrder = 2,
                SeName = SeoExtensions.GetSeName("13.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "13.3''",
                DisplayOrder = 3,
                SeName = SeoExtensions.GetSeName("13.3''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "14.0''",
                DisplayOrder = 4,
                SeName = SeoExtensions.GetSeName("14.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "15.0''",
                DisplayOrder = 4,
                SeName = SeoExtensions.GetSeName("15.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "15.6''",
                DisplayOrder = 5,
                SeName = SeoExtensions.GetSeName("15.6''", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa1);

            var sa2 = new SpecificationAttribute {
                Name = "CPU Type",
                DisplayOrder = 2,
                SeName = SeoExtensions.GetSeName("CPU Type", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa2);

            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "Intel Core i5",
                DisplayOrder = 1,
                SeName = SeoExtensions.GetSeName("Intel Core i5", false, false),
            });

            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "Intel Core i7",
                DisplayOrder = 2,
                SeName = SeoExtensions.GetSeName("Intel Core i7", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa2);

            var sa3 = new SpecificationAttribute {
                Name = "Memory",
                DisplayOrder = 3,
                SeName = SeoExtensions.GetSeName("Memory", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa3);

            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "4 GB",
                DisplayOrder = 1,
                SeName = SeoExtensions.GetSeName("4 GB", false, false),
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "8 GB",
                DisplayOrder = 2,
                SeName = SeoExtensions.GetSeName("8 GB", false, false),
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "16 GB",
                DisplayOrder = 3,
                SeName = SeoExtensions.GetSeName("16 GB", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa3);

            var sa4 = new SpecificationAttribute {
                Name = "Hardrive",
                DisplayOrder = 5,
                SeName = SeoExtensions.GetSeName("Hardrive", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa4);

            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "128 GB",
                DisplayOrder = 7,
                SeName = SeoExtensions.GetSeName("128 GB", false, false),
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "500 GB",
                DisplayOrder = 4,
                SeName = SeoExtensions.GetSeName("500 GB", false, false),
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "1 TB",
                DisplayOrder = 3,
                SeName = SeoExtensions.GetSeName("1 TB", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa4);
        }

        protected virtual async Task InstallProductAttributes()
        {
            var productAttributes = new List<ProductAttribute>
            {
                new ProductAttribute
                {
                    Name = "Color",
                    SeName = "color"
                },
                new ProductAttribute
                {
                    Name = "Custom Text",
                    SeName = "custom-text"
                },
                new ProductAttribute
                {
                    Name = "HDD",
                    SeName = "hdd"
                },
                new ProductAttribute
                {
                    Name = "OS",
                    SeName = "os"
                },
                new ProductAttribute
                {
                    Name = "Processor",
                    SeName  = "processor"
                },
                new ProductAttribute
                {
                    Name = "RAM",
                    SeName = "ram"
                },
                new ProductAttribute
                {
                    Name = "Size",
                    SeName = "size"
                },
                new ProductAttribute
                {
                    Name = "Software",
                    SeName = "software"
                },
            };
            await _productAttributeRepository.InsertAsync(productAttributes);
        }

        protected virtual async Task InstallCategories()
        {
            var pictureService = _serviceProvider.GetRequiredService<IPictureService>();

            //sample pictures
            var sampleImagesPath = GetSamplesPath();

            var categoryTemplateInGridAndLines = _categoryTemplateRepository
                .Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
            if (categoryTemplateInGridAndLines == null)
                throw new Exception("Category template cannot be loaded");


            //categories
            var allCategories = new List<Category>();
            var categoryComputers = new Category {
                Name = "Computers",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = "",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_computers.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Computers"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                Flag = "New",
                FlagStyle = "badge-danger",
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryComputers);

            var categoryDesktops = new Category {
                Name = "Desktops",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_desktops.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Desktops"))).Id,
                PriceRanges = "-1000;1000-1200;1200-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryDesktops);

            var categoryNotebooks = new Category {
                Name = "Notebooks",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_notebooks.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Notebooks"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryNotebooks);

            var categorySoftware = new Category {
                Name = "Software",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_software.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Software"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categorySoftware);

            var categoryElectronics = new Category {
                Name = "Electronics",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_electronics.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Electronics"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryElectronics);

            var categoryCameraPhoto = new Category {
                Name = "Camera & photo",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_camera_photo.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Camera, photo"))).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryCameraPhoto);

            var categoryCellPhones = new Category {
                Name = "Cell phones",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_cell_phones.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Cell phones"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryCellPhones);

            var categoryOthers = new Category {
                Name = "Others",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_accessories.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Accessories"))).Id,
                IncludeInTopMenu = true,
                PriceRanges = "-100;100-;",
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryOthers);

            var categoryApparel = new Category {
                Name = "Apparel",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_apparel.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Apparel"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryApparel);

            var categoryShoes = new Category {
                Name = "Shoes",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_shoes.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Shoes"))).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryShoes);

            var categoryClothing = new Category {
                Name = "Clothing",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_clothing.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Clothing"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryClothing);

            var categoryAccessories = new Category {
                Name = "Accessories",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryApparel.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_apparel_accessories.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Apparel Accessories"))).Id,
                IncludeInTopMenu = true,
                PriceRanges = "-100;100-;",
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryAccessories);

            var categoryDigitalDownloads = new Category {
                Name = "Digital downloads",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_digital_downloads.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Digital downloads"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = false,
                DisplayOrder = 4,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryDigitalDownloads);

            var categoryBooks = new Category {
                Name = "Books",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MetaKeywords = "Books, Dictionary, Textbooks",
                MetaDescription = "Books category description",
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_book.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Book"))).Id,
                PriceRanges = "-25;25-50;50-;",
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 5,
                Flag = "Promo!",
                FlagStyle = "bg-success",
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryBooks);

            var categoryJewelry = new Category {
                Name = "Jewelry",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_jewelry.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Jewelry"))).Id,
                PriceRanges = "0-500;500-700;700-3000;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 6,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryJewelry);

            var categoryGiftCards = new Category {
                Name = "Gift Cards",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_gift_cards.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Gift Cards"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 7,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryGiftCards);

            await _categoryRepository.InsertAsync(allCategories);
            //search engine names
            foreach (var category in allCategories)
            {
                category.SeName = SeoExtensions.GetSeName(category.Name, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = category.Id,
                    EntityName = "Category",
                    LanguageId = "",
                    IsActive = true,
                    Slug = category.SeName,
                });
                await _categoryRepository.UpdateAsync(category);
            }
        }

        // Install order's tags
        protected virtual async Task InstallOrderTags()
        {
            var coolTag = new OrderTag {
                Name = "cool",
                Count = 0

            };
             await _orderTagRepository.InsertAsync(coolTag);

            var newTag = new OrderTag {
                Name = "new",
                Count = 0

            };
            await _orderTagRepository.InsertAsync(newTag);

            var oldTag = new OrderTag {
                Name = "old",
                Count = 0

            };
            await _orderTagRepository.InsertAsync(oldTag);

        }

        protected virtual async Task InstallManufacturers()
        {
            var pictureService = _serviceProvider.GetRequiredService<IPictureService>();
            var downloadService = _serviceProvider.GetRequiredService<IDownloadService>();

            var sampleImagesPath = GetSamplesPath();

            var manufacturerTemplateInGridAndLines =
                _manufacturerTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
            if (manufacturerTemplateInGridAndLines == null)
                throw new Exception("Manufacturer template cannot be loaded");

            var allManufacturers = new List<Manufacturer>();
            var manufacturerAsus = new Manufacturer {
                Name = "Apple",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_apple.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Apple"))).Id,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerAsus);
            allManufacturers.Add(manufacturerAsus);


            var manufacturerHp = new Manufacturer {
                Name = "HP",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_hp.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Hp"))).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerHp);
            allManufacturers.Add(manufacturerHp);


            var manufacturerNike = new Manufacturer {
                Name = "Nike",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_nike.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Nike"))).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerNike);
            allManufacturers.Add(manufacturerNike);

            //search engine names
            foreach (var manufacturer in allManufacturers)
            {
                manufacturer.SeName = SeoExtensions.GetSeName(manufacturer.Name, false, true);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = manufacturer.Id,
                    EntityName = "Manufacturer",
                    LanguageId = "",
                    IsActive = true,
                    Slug = manufacturer.SeName
                });
                await _manufacturerRepository.UpdateAsync(manufacturer);
            }
        }

        protected virtual async Task InstallProducts(string defaultUserEmail)
        {
            var pictureService = _serviceProvider.GetRequiredService<IPictureService>();
            var downloadService = _serviceProvider.GetRequiredService<IDownloadService>();

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
            var sampleImagesPath = GetSamplesPath();

            //downloads
            var sampleDownloadsPath = GetSamplesPath();

            //default store
            var defaultStore = _storeRepository.Table.FirstOrDefault();
            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            //products
            var allProducts = new List<Product>();

            #region Desktops


            var productBuildComputer = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Build your own computer",
                ShortDescription = "Build it",
                FullDescription = "<p>Fight back against cluttered workspaces with the stylish IBM zBC12 All-in-One desktop PC, featuring powerful computing resources and a stunning 20.1-inch widescreen display with stunning XBRITE-HiColor LCD technology. The black IBM zBC12 has a built-in microphone and MOTION EYE camera with face-tracking technology that allows for easy communication with friends and family. And it has a built-in DVD burner and Sony's Movie Store software so you can create a digital entertainment library for personal viewing at your convenience. Easy to setup and even easier to use, this JS-series All-in-One includes an elegantly designed keyboard and a USB mouse.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Processor").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2.2 GHz Intel Pentium Dual-Core E2200",
                                DisplayOrder = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2.5 GHz Intel Pentium Dual-Core E2200",
                                IsPreSelected = true,
                                PriceAdjustment = 15,
                                DisplayOrder = 2,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "RAM").Id,
                        AttributeControlType = AttributeControlType.DropdownList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2 GB",
                                DisplayOrder = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "4GB",
                                PriceAdjustment = 20,
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "8GB",
                                PriceAdjustment = 60,
                                DisplayOrder = 3,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "HDD").Id,
                        AttributeControlType = AttributeControlType.RadioList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "320 GB",
                                DisplayOrder = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "400 GB",
                                PriceAdjustment = 100,
                                DisplayOrder = 2,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "OS").Id,
                        AttributeControlType = AttributeControlType.RadioList,
                        IsRequired = true,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Vista Home",
                                PriceAdjustment = 50,
                                IsPreSelected = true,
                                DisplayOrder = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Vista Premium",
                                PriceAdjustment = 60,
                                DisplayOrder = 2,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
                        ProductAttributeId = _productAttributeRepository.Table.Single(x => x.Name == "Software").Id,
                        AttributeControlType = AttributeControlType.Checkboxes,
                        ProductAttributeValues =
                        {
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Microsoft Office",
                                PriceAdjustment = 50,
                                IsPreSelected = true,
                                DisplayOrder = 1,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Acrobat Reader",
                                PriceAdjustment = 10,
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Total Commander",
                                PriceAdjustment = 5,
                                DisplayOrder = 2,
                            }
                        }
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Desktops").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productBuildComputer);

            var Picture1 = await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name));
            var Picture2 = await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name));

            await _productRepository.InsertAsync(productBuildComputer);

            var productpicture1 = new ProductPicture {
                ProductId = productBuildComputer.Id,
                PictureId = Picture1.Id,
                DisplayOrder = 1
            };
            var productpicture2 = new ProductPicture {
                ProductId = productBuildComputer.Id,
                PictureId = Picture2.Id,
                DisplayOrder = 1
            };
            productBuildComputer.ProductPictures.Add(productpicture1);
            productBuildComputer.ProductPictures.Add(productpicture2);
            await _productRepository.UpdateAsync(productBuildComputer);

            var productDigitalStorm = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Digital Storm VANQUISH 3 Custom Performance PC",
                ShortDescription = "Digital Storm Vanquish 3 Desktop PC",
                FullDescription = "<p>Blow the doors off today?s most demanding games with maximum detail, speed, and power for an immersive gaming experience without breaking the bank.</p><p>Stay ahead of the competition, VANQUISH 3 is fully equipped to easily handle future upgrades, keeping your system on the cutting edge for years to come.</p><p>Each system is put through an extensive stress test, ensuring you experience zero bottlenecks and get the maximum performance from your hardware.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productDigitalStorm);
            productDigitalStorm.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DigitalStorm.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productDigitalStorm.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDigitalStorm);

            var productLenovoIdeaCentre = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo IdeaCentre 600 All-in-One PC",
                ShortDescription = "",
                FullDescription = "<p>The A600 features a 21.5in screen, DVD or optional Blu-Ray drive, support for the full beans 1920 x 1080 HD, Dolby Home Cinema certification and an optional hybrid analogue/digital TV tuner.</p><p>Connectivity is handled by 802.11a/b/g - 802.11n is optional - and an ethernet port. You also get four USB ports, a Firewire slot, a six-in-one card reader and a 1.3- or two-megapixel webcam.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Desktops").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLenovoIdeaCentre);
            productLenovoIdeaCentre.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LenovoIdeaCentre.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLenovoIdeaCentre.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLenovoIdeaCentre);

            #endregion

            #region Notebooks

            var productAppleMacBookPro = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Apple MacBook Pro 13-inch",
                ShortDescription = "A groundbreaking Retina display. A new force-sensing trackpad. All-flash architecture. Powerful dual-core and quad-core Intel processors. Together, these features take the notebook to a new level of performance. And they will do the same for you in everything you create.",
                FullDescription = "<p>With fifth-generation Intel Core processors, the latest graphics, and faster flash storage, the incredibly advanced MacBook Pro with Retina display moves even further ahead in performance and battery life.* *Compared with the previous generation.</p><p>Retina display with 2560-by-1600 resolution</p><p>Fifth-generation dual-core Intel Core i5 processor</p><p>Intel Iris Graphics</p><p>Up to 9 hours of battery life1</p><p>Faster flash storage2</p><p>802.11ac Wi-Fi</p><p>Two Thunderbolt 2 ports for connecting high-performance devices and transferring data at lightning speed</p><p>Two USB 3 ports (compatible with USB 2 devices) and HDMI</p><p>FaceTime HD camera</p><p>Pages, Numbers, Keynote, iPhoto, iMovie, GarageBand included</p><p>OS X, the world's most advanced desktop operating system</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 1800M,
                OldPrice = 2000M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Apple").Id,
                        DisplayOrder = 2,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "13.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "4 GB").Id
                    }
                }
            };
            allProducts.Add(productAppleMacBookPro);
            productAppleMacBookPro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_macbook_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleMacBookPro.Name))).Id,
                DisplayOrder = 1,
            });
            productAppleMacBookPro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_macbook_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleMacBookPro.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productAppleMacBookPro);


            var productAsusN551JK = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Asus N551JK-XO076H Laptop",
                ShortDescription = "Laptop Asus N551JK Intel Core i7-4710HQ 2.5 GHz, RAM 16GB, HDD 1TB, Video NVidia GTX 850M 4GB, BluRay, 15.6, Full HD, Win 8.1",
                FullDescription = "<p>The ASUS N550JX combines cutting-edge audio and visual technology to deliver an unsurpassed multimedia experience. A full HD wide-view IPS panel is tailor-made for watching movies and the intuitive touchscreen makes for easy, seamless navigation. ASUS has paired the N550JX?s impressive display with SonicMaster Premium, co-developed with Bang & Olufsen ICEpower® audio experts, for true surround sound. A quad-speaker array and external subwoofer combine for distinct vocals and a low bass that you can feel.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                    }
                },
                ProductSpecificationAttributes =
                {
                     new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.6''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "16 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "1 TB").Id
                    }
                }
            };
            allProducts.Add(productAsusN551JK);
            productAsusN551JK.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asuspc_N551JK.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAsusN551JK.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAsusN551JK);


            var productSamsungSeries = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Samsung Series 9 NP900X4C Premium Ultrabook",
                ShortDescription = "Samsung Series 9 NP900X4C-A06US 15-Inch Ultrabook (1.70 GHz Intel Core i5-3317U Processor, 8GB DDR3, 128GB SSD, Windows 8) Ash Black",
                FullDescription = "<p>Designed with mobility in mind, Samsung's durable, ultra premium, lightweight Series 9 laptop (model NP900X4C-A01US) offers mobile professionals and power users a sophisticated laptop equally suited for work and entertainment. Featuring a minimalist look that is both simple and sophisticated, its polished aluminum uni-body design offers an iconic look and feel that pushes the envelope with an edge just 0.58 inches thin. This Series 9 laptop also includes a brilliant 15-inch SuperBright Plus display with HD+ technology, 128 GB Solid State Drive (SSD), 8 GB of system memory, and up to 10 hours of battery life.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Notebooks").Id,
                        DisplayOrder = 1,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "8 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "128 GB").Id
                    }
                }
            };
            allProducts.Add(productSamsungSeries);
            productSamsungSeries.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SamsungNP900X4C.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSamsungSeries.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productSamsungSeries);

            var productHpSpectre = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HP Spectre XT Pro UltraBook",
                ShortDescription = "HP Spectre XT Pro UltraBook / Intel Core i5-2467M / 13.3 / 4GB / 128GB / Windows 7 Professional / Laptop",
                FullDescription = "<p>Introducing HP ENVY Spectre XT, the Ultrabook designed for those who want style without sacrificing substance. It's sleek. It's thin. And with Intel. Corer i5 processor and premium materials, it's designed to go anywhere from the bistro to the boardroom, it's unlike anything you've ever seen from HP.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "HP").Id,
                        DisplayOrder = 3,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "13.3''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i5").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "4 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "128 GB").Id
                    }
                }
            };
            allProducts.Add(productHpSpectre);
            productHpSpectre.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HPSpectreXT_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpSpectre.Name))).Id,
                DisplayOrder = 1,
            });
            productHpSpectre.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HPSpectreXT_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpSpectre.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productHpSpectre);


            var productHpEnvy = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HP Envy 6-1180ca 15.6-Inch Sleekbook",
                ShortDescription = "HP ENVY 6-1202ea Ultrabook Beats Audio, 3rd generation Intel® CoreTM i7-3517U processor, 8GB RAM, 500GB HDD, Microsoft Windows 8, AMD Radeon HD 8750M (2 GB DDR3 dedicated)",
                FullDescription = "The UltrabookTM that's up for anything. Thin and light, the HP ENVY is the large screen UltrabookTM with Beats AudioTM. With a soft-touch base that makes it easy to grab and go, it's a laptop that's up for anything.<br><br><b>Features</b><br><br>- Windows 8 or other operating systems available<br><br><b>Top performance. Stylish design. Take notice.</b><br><br>- At just 19.8 mm (0.78 in) thin, the HP ENVY UltrabookTM is slim and light enough to take anywhere. It's the laptop that gets you noticed with the power to get it done.<br>- With an eye-catching metal design, it's a laptop that you want to carry with you. The soft-touch, slip-resistant base gives you the confidence to carry it with ease.<br><br><b>More entertaining. More gaming. More fun.</b><br><br>- Own the UltrabookTM with Beats AudioTM, dual speakers, a subwoofer, and an awesome display. Your music, movies and photo slideshows will always look and sound their best.<br>- Tons of video memory let you experience incredible gaming and multimedia without slowing down. Create and edit videos in a flash. And enjoy more of what you love to the fullest.<br>- The HP ENVY UltrabookTM is loaded with the ports you'd expect on a world-class laptop, but on a Sleekbook instead. Like HDMI, USB, RJ-45, and a headphone jack. You get all the right connections without compromising size.<br><br><b>Only from HP.</b><br><br>- Life heats up. That's why there's HP CoolSense technology, which automatically adjusts your notebook's temperature based on usage and conditions. It stays cool. You stay comfortable.<br>- With HP ProtectSmart, your notebook's data stays safe from accidental bumps and bruises. It senses motion and plans ahead, stopping your hard drive and protecting your entire digital life.<br>- Keep playing even in dimly lit rooms or on red eye flights. The optional backlit keyboard[1] is full-size so you don't compromise comfort. Backlit keyboard. Another bright idea.<br><br>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "HP").Id,
                        DisplayOrder = 4,
                    }
                },
                ProductSpecificationAttributes =
                {
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "15.6''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 3,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Memory").SpecificationAttributeOptions.Single(sao => sao.Name == "8 GB").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 4,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Hardrive").SpecificationAttributeOptions.Single(sao => sao.Name == "500 GB").Id
                    }
                }
            };
            allProducts.Add(productHpEnvy);
            productHpEnvy.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpEnvy6.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpEnvy.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productHpEnvy);


            var productLenovoThinkpad = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo Thinkpad X1 Carbon Laptop",
                ShortDescription = "Lenovo Thinkpad X1 Carbon Touch Intel Core i7 14 Ultrabook",
                FullDescription = "<p>The X1 Carbon brings a new level of quality to the ThinkPad legacy of high standards and innovation. It starts with the durable, carbon fiber-reinforced roll cage, making for the best Ultrabook construction available, and adds a host of other new features on top of the old favorites. Because for 20 years, we haven't stopped innovating. And you shouldn't stop benefiting from that.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductSpecificationAttributes =
                {
                   new ProductSpecificationAttribute
                    {
                        AllowFiltering = false,
                        ShowOnProductPage = true,
                        DisplayOrder = 1,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "Screensize").SpecificationAttributeOptions.Single(sao => sao.Name == "14.0''").Id
                    },
                    new ProductSpecificationAttribute
                    {
                        AllowFiltering = true,
                        ShowOnProductPage = true,
                        DisplayOrder = 2,
                        SpecificationAttributeId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").Id,
                        SpecificationAttributeOptionId = _specificationAttributeRepository.Table.Single(sa => sa.Name == "CPU Type").SpecificationAttributeOptions.Single(sao => sao.Name == "Intel Core i7").Id
                    }
                }
            };
            allProducts.Add(productLenovoThinkpad);
            productLenovoThinkpad.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LenovoThinkpad.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLenovoThinkpad.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLenovoThinkpad);

            #endregion

            #region Software


            var productAdobePhotoshop = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adobe Photoshop CS4",
                ShortDescription = "Easily find and view all your photos",
                FullDescription = "<p>Adobe Photoshop CS4 software combines power and simplicity so you can make ordinary photos extraordinary; tell engaging stories in beautiful, personalized creations for print and web; and easily find and view all your photos. New Photoshop.com membership* works with Photoshop CS4 so you can protect your photos with automatic online backup and 2 GB of storage; view your photos anywhere you are; and share your photos in fun, interactive ways with invitation-only Online Albums.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productAdobePhotoshop);
            productAdobePhotoshop.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AdobePhotoshop.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAdobePhotoshop.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAdobePhotoshop);


            var productWindows8Pro = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Windows 8 Pro",
                ShortDescription = "Windows 8 is a Microsoft operating system that was released in 2012 as part of the company's Windows NT OS family. ",
                FullDescription = "<p>Windows 8 Pro is comparable to Windows 7 Professional and Ultimate and is targeted towards enthusiasts and business users; it includes all the features of Windows 8. Additional features include the ability to receive Remote Desktop connections, the ability to participate in a Windows Server domain, Encrypting File System, Hyper-V, and Virtual Hard Disk Booting, Group Policy as well as BitLocker and BitLocker To Go. Windows Media Center functionality is available only for Windows 8 Pro as a separate software package.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productWindows8Pro);
            productWindows8Pro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Windows8.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productWindows8Pro.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productWindows8Pro);


            var productSoundForge = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Sound Forge Pro 11",
                ShortDescription = "Advanced audio waveform editor.",
                FullDescription = "<p>Sound Forge? Pro is the application of choice for a generation of creative and prolific artists, producers, and editors. Record audio quickly on a rock-solid platform, address sophisticated audio processing tasks with surgical precision, and render top-notch master files with ease. New features include one-touch recording, metering for the new critical standards, more repair and restoration tools, and exclusive round-trip interoperability with SpectraLayers Pro. Taken together, these enhancements make this edition of Sound Forge Pro the deepest and most advanced audio editing platform available.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productSoundForge);
            productSoundForge.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SoundForge.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSoundForge.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productSoundForge);


            #endregion

            #region Camera, Photo


            //this one is a grouped product with two associated ones
            var productNikonD5500DSLR = new Product {
                ProductType = ProductType.GroupedProduct,
                VisibleIndividually = true,
                Name = "Nikon D5500 DSLR",
                ShortDescription = "Slim, lightweight Nikon D5500 packs a vari-angle touchscreen",
                FullDescription = "Nikon has announced its latest DSLR, the D5500. A lightweight, compact DX-format camera with a 24.2MP sensor, it?s the first of its type to offer a vari-angle touchscreen. The D5500 replaces the D5300 in Nikon?s range, and while it offers much the same features the company says it?s a much slimmer and lighter prospect. There?s a deep grip for easier handling and built-in Wi-Fi that lets you transfer and share shots via your phone or tablet.",
                ProductTemplateId = productTemplateGrouped.Id,
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
                    }
                }
            };
            allProducts.Add(productNikonD5500DSLR);
            productNikonD5500DSLR.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNikonD5500DSLR.Name))).Id,
                DisplayOrder = 1,
            });
            productNikonD5500DSLR.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNikonD5500DSLR.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productNikonD5500DSLR);
            var productNikonD5500DSLR_associated_1 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false, //hide this products
                ParentGroupedProductId = productNikonD5500DSLR.Id,
                Name = "Nikon D5500 DSLR - Black",
                ProductTemplateId = productTemplateSimple.Id,
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
            productNikonD5500DSLR_associated_1.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_black.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital SLR Camera - Black"))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNikonD5500DSLR_associated_1);
            var productNikonD5500DSLR_associated_2 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false,
                ParentGroupedProductId = productNikonD5500DSLR.Id,
                Name = "Nikon D5500 DSLR - Red",
                ProductTemplateId = productTemplateSimple.Id,
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
            productNikonD5500DSLR_associated_2.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikonCamera_red.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital SLR Camera - Silver"))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNikonD5500DSLR_associated_2);

            var productLeica = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Leica T Mirrorless Digital Camera",
                ShortDescription = "Leica T (Typ 701) Silver",
                FullDescription = "<p>The new Leica T offers a minimalist design that's crafted from a single block of aluminum.  Made in Germany and assembled by hand, this 16.3 effective mega pixel camera is easy to use.  With a massive 3.7 TFT LCD intuitive touch screen control, the user is able to configure and save their own menu system.  The Leica T has outstanding image quality and also has 16GB of built in memory.  This is Leica's first system camera to use Wi-Fi.  Add the T-App to your portable iOS device and be able to transfer and share your images (free download from the Apple App Store)</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productLeica);
            productLeica.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeicaT.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productLeica.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLeica);


            var productAppleICam = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Apple iCam",
                ShortDescription = "Photography becomes smart",
                FullDescription = "<p>A few months ago we featured the amazing WVIL camera, by many considered the future of digital photography. This is another very good looking concept, iCam is the vision of Italian designer Antonio DeRosa, the idea is to have a device that attaches to the iPhone 5, which then allows the user to have a camera with interchangeable lenses. The device would also feature a front-touch screen and a projector. Would be great if apple picked up on this and made it reality.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Apple").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productAppleICam);
            productAppleICam.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_iCam.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAppleICam.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAppleICam);

            #endregion

            #region Cell Phone

            var productHtcOne = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HTC One M8 Android L 5.0 Lollipop",
                ShortDescription = "HTC - One (M8) 4G LTE Cell Phone with 32GB Memory - Gunmetal (Sprint)",
                FullDescription = "<p><b>HTC One (M8) Cell Phone for Sprint:</b> With its brushed-metal design and wrap-around unibody frame, the HTC One (M8) is designed to fit beautifully in your hand. It's fun to use with amped up sound and a large Full HD touch screen, and intuitive gesture controls make it seem like your phone almost knows what you need before you do. <br><br>Sprint Easy Pay option available in store.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                Flag = "New",
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
                    }
                }
            };
            allProducts.Add(productHtcOne);
            productHtcOne.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_M8.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOne.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productHtcOne);


            var productHtcOneMini = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "HTC One Mini Blue",
                ShortDescription = "HTC One and HTC One Mini now available in bright blue hue",
                FullDescription = "<p>HTC One mini smartphone with 4.30-inch 720x1280 display powered by 1.4GHz processor alongside 1GB RAM and 4-Ultrapixel rear camera.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productHtcOneMini);
            productHtcOneMini.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_Mini_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOneMini.Name))).Id,
                DisplayOrder = 1,
            });
            productHtcOneMini.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HTC_One_Mini_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHtcOneMini.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productHtcOneMini);


            var productNokiaLumia = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nokia Lumia 1020",
                ShortDescription = "Nokia Lumia 1020 4G Cell Phone (Unlocked)",
                FullDescription = "<p>Capture special moments for friends and family with this Nokia Lumia 1020 32GB WHITE cell phone that features an easy-to-use 41.0MP rear-facing camera and a 1.2MP front-facing camera. The AMOLED touch screen offers 768 x 1280 resolution for crisp visuals.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productNokiaLumia);
            productNokiaLumia.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Lumia1020.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNokiaLumia.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNokiaLumia);


            #endregion

            #region Others



            var productBeatsPill = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Beats Pill 2.0 Wireless Speaker",
                ShortDescription = "<b>Pill 2.0 Portable Bluetooth Speaker (1-Piece):</b> Watch your favorite movies and listen to music with striking sound quality. This lightweight, portable speaker is easy to take with you as you travel to any destination, keeping you entertained wherever you are. ",
                FullDescription = "<p><ul><li>Pair and play with your Bluetooth® device with 30 foot range</li><li>Built-in speakerphone</li><li>7 hour rechargeable battery</li><li>Power your other devices with USB charge out</li><li>Tap two Beats Pills? together for twice the sound with Beats Bond?</li></ul></p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                        StartDateTimeUtc = DateTime.UtcNow,
                        EndDateTimeUtc = null
                    },
                    new TierPrice
                    {
                        Quantity = 5,
                        Price = 17,
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 15,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Others").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productBeatsPill);
            productBeatsPill.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PillBeats_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBeatsPill.Name))).Id,
                DisplayOrder = 1,
            });
            productBeatsPill.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PillBeats_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBeatsPill.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productBeatsPill);


            var productUniversalTabletCover = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Universal 7-8 Inch Tablet Cover",
                ShortDescription = "Universal protection for 7-inch & 8-inch tablets",
                FullDescription = "<p>Made of durable polyurethane, our Universal Cover is slim, lightweight, and strong, with protective corners that stretch to hold most 7 and 8-inch tablets securely. This tough case helps protects your tablet from bumps, scuffs, and dings.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productUniversalTabletCover);
            productUniversalTabletCover.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_TabletCover.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productUniversalTabletCover.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productUniversalTabletCover);


            var productPortableSoundSpeakers = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Portable Sound Speakers",
                ShortDescription = "Universall portable sound speakers",
                FullDescription = "<p>Your phone cut the cord, now it's time for you to set your music free and buy a Bluetooth speaker. Thankfully, there's one suited for everyone out there.</p><p>Some Bluetooth speakers excel at packing in as much functionality as the unit can handle while keeping the price down. Other speakers shuck excess functionality in favor of premium build materials instead. Whatever path you choose to go down, you'll be greeted with many options to suit your personal tastes.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productPortableSoundSpeakers);
            productPortableSoundSpeakers.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Speakers.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPortableSoundSpeakers.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productPortableSoundSpeakers);


            #endregion

            #region Shoes


            var productNikeFloral = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Floral Roshe Customized Running Shoes",
                ShortDescription = "When you ran across these shoes, you will immediately fell in love and needed a pair of these customized beauties.",
                FullDescription = "<p>Each Rosh Run is personalized and exclusive, handmade in our workshop Custom. Run Your Rosh creations born from the hand of an artist specialized in sneakers, more than 10 years of experience.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "9",
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "10",
                                DisplayOrder = 3,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "11",
                                DisplayOrder = 4,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "White/Black",
                                DisplayOrder = 2,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productNikeFloral);
            productNikeFloral.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeFloralShoe_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeFloral.Name))).Id,
                DisplayOrder = 1,
            });
            productNikeFloral.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeFloralShoe_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeFloral.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productNikeFloral);


            var productAdidas = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "adidas Consortium Campus 80s Running Shoes",
                ShortDescription = "adidas Consortium Campus 80s Primeknit Light Maroon/Running Shoes",
                FullDescription = "<p>One of three colorways of the adidas Consortium Campus 80s Primeknit set to drop alongside each other. This pair comes in light maroon and running white. Featuring a maroon-based primeknit upper with white accents. A limited release, look out for these at select adidas Consortium accounts worldwide.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductAttributeMappings =
                {
                    new ProductAttributeMapping
                    {
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "9",
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "10",
                                DisplayOrder = 3,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "11",
                                DisplayOrder = 4,
                            }
                        }
                    },
                    new ProductAttributeMapping
                    {
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Blue",
                                ColorSquaresRgb = "#363656",
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Silver",
                                ColorSquaresRgb = "#c5c5d5",
                                DisplayOrder = 3,
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
                    }
                }
            };
            allProducts.Add(productAdidas);
            productAdidas.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name))).Id,
                DisplayOrder = 1,
            });
            productAdidas.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name))).Id,
                DisplayOrder = 2,
            });
            productAdidas.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_3.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidas.Name))).Id,
                DisplayOrder = 3,
            });


            await _productRepository.InsertAsync(productAdidas);

            var productAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Color").FirstOrDefault();

            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Red").First().PictureId = productAdidas.ProductPictures.ElementAt(0).PictureId;
            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Blue").First().PictureId = productAdidas.ProductPictures.ElementAt(1).PictureId;
            productAdidas.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Silver").First().PictureId = productAdidas.ProductPictures.ElementAt(2).PictureId;
            await _productRepository.UpdateAsync(productAdidas);


            var productNikeZoom = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike SB Zoom Stefan Janoski \"Medium Mint\"",
                ShortDescription = "Nike SB Zoom Stefan Janoski Dark Grey Medium Mint Teal ...",
                FullDescription = "The newly Nike SB Zoom Stefan Janoski gets hit with a \"Medium Mint\" accents that sits atop a Dark Grey suede. Expected to drop in October.",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productNikeZoom);
            productNikeZoom.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeZoom.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeZoom.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNikeZoom);


            #endregion

            #region Clothing

            var productNikeTailwind = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Tailwind Loose Short-Sleeve Running Shirt",
                ShortDescription = "",
                FullDescription = "<p>Boost your adrenaline with the Nike® Women's Tailwind Running Shirt. The lightweight, slouchy fit is great for layering, and moisture-wicking fabrics keep you feeling at your best. This tee has a notched hem for an enhanced range of motion, while flat seams with reinforcement tape lessen discomfort and irritation over longer distances. Put your keys and card in the side zip pocket and take off in your Nike® running t-shirt.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "1X",
                                DisplayOrder = 2,
                            },

                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "2X",
                                DisplayOrder = 3,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "3X",
                                DisplayOrder = 4,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "4X",
                                DisplayOrder = 5,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "5X",
                                DisplayOrder = 6,
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
                    }
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Nike").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productNikeTailwind);
            productNikeTailwind.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NikeShirt.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productNikeTailwind.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNikeTailwind);

            var productOversizedWomenTShirt = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Oversized Women T-Shirt",
                ShortDescription = "",
                FullDescription = "<p>This oversized women t-Shirt needs minimum ironing. It is a great product at a great value!</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    },
                    new TierPrice
                    {
                        Quantity = 7,
                        Price = 19,
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 16,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productOversizedWomenTShirt);
            productOversizedWomenTShirt.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_WomenTShirt.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productOversizedWomenTShirt.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productOversizedWomenTShirt);


            var productCustomTShirt = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Custom T-Shirt",
                ShortDescription = "T-Shirt - Add Your Content",
                FullDescription = "<p>Comfort comes in all shapes and forms, yet this tee out does it all. Rising above the rest, our classic cotton crew provides the simple practicality you need to make it through the day. Tag-free, relaxed fit wears well under dress shirts or stands alone in laid-back style. Reinforced collar and lightweight feel give way to long-lasting shape and breathability. One less thing to worry about, rely on this tee to provide comfort and ease with every wear.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productCustomTShirt);
            productCustomTShirt.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CustomTShirt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCustomTShirt.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productCustomTShirt);


            var productLeviJeans = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Levi's 511 Jeans",
                ShortDescription = "Levi's Faded Black 511 Jeans ",
                FullDescription = "<p>Between a skinny and straight fit, our 511&trade; slim fit jeans are cut close without being too restricting. Slim throughout the thigh and leg opening for a long and lean look.</p><ul><li>Slouch1y at top; sits below the waist</li><li>Slim through the leg, close at the thigh and straight to the ankle</li><li>Stretch for added comfort</li><li>Classic five-pocket styling</li><li>99% Cotton, 1% Spandex, 11.2 oz. - Imported</li></ul>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    },
                    new TierPrice
                    {
                        Quantity = 6,
                        Price = 38,
                    },
                    new TierPrice
                    {
                        Quantity = 10,
                        Price = 35,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Clothing").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLeviJeans);

            productLeviJeans.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name))).Id,
                DisplayOrder = 1,
            });
            productLeviJeans.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productLeviJeans);


            #endregion

            #region Accessories


            var productObeyHat = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Obey Propaganda Hat",
                ShortDescription = "",
                FullDescription = "<p>Printed poplin 5 panel camp hat with debossed leather patch and web closure</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Medium",
                                DisplayOrder = 2,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "Large",
                                DisplayOrder = 3,
                            },
                            new ProductAttributeValue
                            {
                                AttributeValueType = AttributeValueType.Simple,
                                Name = "X-Large",
                                DisplayOrder = 4,
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
                    }
                }
            };
            allProducts.Add(productObeyHat);
            productObeyHat.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_hat.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productObeyHat.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productObeyHat);



            var productBelt = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Reversible Horseferry Check Belt",
                ShortDescription = "Reversible belt in Horseferry check with smooth leather trim",
                FullDescription = "<p>Reversible belt in Horseferry check with smooth leather trim</p><p>Leather lining, polished metal buckle</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productBelt);
            productBelt.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Belt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBelt.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productBelt);



            var productSunglasses = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Ray Ban Aviator Sunglasses",
                ShortDescription = "Aviator sunglasses are one of the first widely popularized styles of modern day sunwear.",
                FullDescription = "<p>Since 1937, Ray-Ban can genuinely claim the title as the world's leading sunglasses and optical eyewear brand. Combining the best of fashion and sports performance, the Ray-Ban line of Sunglasses delivers a truly classic style that will have you looking great today and for years to come.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productSunglasses);
            productSunglasses.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Sunglasses.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productSunglasses.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productSunglasses);

            #endregion

            #region Digital Downloads


            var downloadNightVision1 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_NightVision_1.zip"),
                Extension = ".zip",
                Filename = "Night_Vision_1",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadNightVision1);
            var downloadNightVision2 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_NightVision_2.txt"),
                Extension = ".txt",
                Filename = "Night_Vision_1",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadNightVision2);
            var productNightVision = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Night Visions",
                ShortDescription = "Night Visions is the debut studio album by American rock band Imagine Dragons.",
                FullDescription = "<p>Original Release Date: September 4, 2012</p><p>Release Date: September 4, 2012</p><p>Genre - Alternative rock, indie rock, electronic rock</p><p>Label - Interscope/KIDinaKORNER</p><p>Copyright: (C) 2011 Interscope Records</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productNightVision);
            productNightVision.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_NightVisions.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productNightVision.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNightVision);



            var downloadIfYouWait1 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_IfYouWait_1.zip"),
                Extension = ".zip",
                Filename = "If_You_Wait_1",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadIfYouWait1);
            var downloadIfYouWait2 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_IfYouWait_2.txt"),
                Extension = ".txt",
                Filename = "If_You_Wait_1",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadIfYouWait2);
            var productIfYouWait = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "If You Wait",
                ShortDescription = "If You Wait is the debut studio album by English indie pop band London Grammar",
                FullDescription = "<p>Original Release Date: September 6, 2013</p><p>Genre - Electronica, dream pop downtempo, pop</p><p>Label - Metal & Dust/Ministry of Sound</p><p>Producer - Tim Bran, Roy Kerr London, Grammar</p><p>Length - 43:22</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productIfYouWait);

            productIfYouWait.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_IfYouWait.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productIfYouWait.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productIfYouWait);


            var downloadScienceAndFaith = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_ScienceAndFaith_1.zip"),
                Extension = ".zip",
                Filename = "Science_And_Faith",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadScienceAndFaith);
            var productScienceAndFaith = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Science & Faith",
                ShortDescription = "Science & Faith is the second studio album by Irish pop rock band The Script.",
                FullDescription = "<p># Original Release Date: September 10, 2010<br /># Label: RCA, Epic/Phonogenic(America)<br /># Copyright: 2010 RCA Records.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productScienceAndFaith);
            productScienceAndFaith.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ScienceAndFaith.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productScienceAndFaith.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productScienceAndFaith);



            #endregion

            #region Books

            var productFahrenheit = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Fahrenheit 451 by Ray Bradbury",
                ShortDescription = "Fahrenheit 451 is a dystopian novel by Ray Bradbury published in 1953. It is regarded as one of his best works.",
                FullDescription = "<p>The novel presents a future American society where books are outlawed and firemen burn any that are found. The title refers to the temperature that Bradbury understood to be the autoignition point of paper.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productFahrenheit);
            productFahrenheit.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Fahrenheit451.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productFahrenheit.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productFahrenheit);



            var productFirstPrizePies = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "First Prize Pies",
                ShortDescription = "Allison Kave made pies as a hobby, until one day her boyfriend convinced her to enter a Brooklyn pie-making contest. She won. In fact, her pies were such a hit that she turned pro.",
                FullDescription = "<p>First Prize Pies, a boutique, made-to-order pie business that originated on New York's Lower East Side, has become synonymous with tempting and unusual confections. For the home baker who is passionate about seasonal ingredients and loves a creative approach to recipes, First Prize Pies serves up 52 weeks of seasonal and eclectic pastries in an interesting pie-a-week format. Clear instructions, technical tips and creative encouragement guide novice bakers as well as pie mavens. With its nostalgia-evoking photos of homemade pies fresh out of the oven, First Prize Pies will be as giftable as it is practical.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productFirstPrizePies);
            productFirstPrizePies.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_FirstPrizePies.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productFirstPrizePies.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productFirstPrizePies);

            var productPrideAndPrejudice = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Pride and Prejudice",
                ShortDescription = "Pride and Prejudice is a novel of manners by Jane Austen, first published in 1813.",
                FullDescription = "<p>Set in England in the early 19th century, Pride and Prejudice tells the story of Mr and Mrs Bennet's five unmarried daughters after the rich and eligible Mr Bingley and his status-conscious friend, Mr Darcy, have moved into their neighbourhood. While Bingley takes an immediate liking to the eldest Bennet daughter, Jane, Darcy has difficulty adapting to local society and repeatedly clashes with the second-eldest Bennet daughter, Elizabeth.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productPrideAndPrejudice);
            productPrideAndPrejudice.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PrideAndPrejudice.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPrideAndPrejudice.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productPrideAndPrejudice);



            #endregion

            #region Jewelry

            var productElegantGemstoneNecklace = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Elegant Gemstone Necklace",
                ShortDescription = "Classic and elegant gemstone necklace now available in our store",
                FullDescription = "<p>For those who like jewelry, creating their ownelegant jewelry from gemstone beads provides an economical way to incorporate genuine gemstones into your jewelry wardrobe. Manufacturers create beads from all kinds of precious gemstones and semi-precious gemstones, which are available in bead shops, craft stores, and online marketplaces.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productElegantGemstoneNecklace);
            productElegantGemstoneNecklace.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_GemstoneNecklaces.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productElegantGemstoneNecklace.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productElegantGemstoneNecklace);


            var productFlowerGirlBracelet = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Flower Girl Bracelet",
                ShortDescription = "Personalised Flower Braceled",
                FullDescription = "<p>This is a great gift for your flower girl to wear on your wedding day. A delicate bracelet that is made with silver plated soldered cable chain, gives this bracelet a dainty look for young wrist. A Swarovski heart, shown in Rose, hangs off a silver plated flower. Hanging alongside the heart is a silver plated heart charm with Flower Girl engraved on both sides. This is a great style for the younger flower girl.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productFlowerGirlBracelet);
            productFlowerGirlBracelet.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_FlowerBracelet.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productFlowerGirlBracelet.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productFlowerGirlBracelet);


            var productEngagementRing = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Vintage Style Engagement Ring",
                ShortDescription = "1.24 Carat (ctw) in 14K White Gold (Certified)",
                FullDescription = "<p>Dazzle her with this gleaming 14 karat white gold vintage proposal. A ravishing collection of 11 decadent diamonds come together to invigorate a superbly ornate gold shank. Total diamond weight on this antique style engagement ring equals 1 1/4 carat (ctw). Item includes diamond certificate.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(productEngagementRing);
            productEngagementRing.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_EngagementRing_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productEngagementRing.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productEngagementRing);



            #endregion

            #region Gift Cards


            var product25GiftCard = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$25 Virtual Gift Card",
                ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(product25GiftCard);
            product25GiftCard.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_25giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product25GiftCard.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(product25GiftCard);


            var product50GiftCard = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$50 Physical Gift Card",
                ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(product50GiftCard);
            product50GiftCard.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_50giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product50GiftCard.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(product50GiftCard);


            var product100GiftCard = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$100 Physical Gift Card",
                ShortDescription = "$100 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSimple.Id,
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
                    }
                }
            };
            allProducts.Add(product100GiftCard);
            product100GiftCard.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_100giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product100GiftCard.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(product100GiftCard);

            #endregion

            //search engine names
            foreach (var product in allProducts)
            {
                product.SeName = SeoExtensions.GetSeName(product.Name, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = product.Id,
                    EntityName = "Product",
                    LanguageId = "",
                    IsActive = true,
                    Slug = product.SeName,
                });

                await _productRepository.UpdateAsync(product);
            }


            #region Related Products

            //related products

            productFlowerGirlBracelet.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFlowerGirlBracelet.Id,
                    ProductId2 = productEngagementRing.Id,
                });

            productFlowerGirlBracelet.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFlowerGirlBracelet.Id,
                    ProductId2 = productElegantGemstoneNecklace.Id,
                });

            productEngagementRing.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productEngagementRing.Id,
                    ProductId2 = productFlowerGirlBracelet.Id,
                });

            productEngagementRing.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productEngagementRing.Id,
                    ProductId2 = productElegantGemstoneNecklace.Id,
                });

            productElegantGemstoneNecklace.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productElegantGemstoneNecklace.Id,
                    ProductId2 = productFlowerGirlBracelet.Id,
                });

            productElegantGemstoneNecklace.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productElegantGemstoneNecklace.Id,
                    ProductId2 = productEngagementRing.Id,
                });

            productIfYouWait.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productIfYouWait.Id,
                    ProductId2 = productNightVision.Id,
                });

            productIfYouWait.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productIfYouWait.Id,
                    ProductId2 = productScienceAndFaith.Id,
                });

            productNightVision.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNightVision.Id,
                    ProductId2 = productIfYouWait.Id,
                });

            productNightVision.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNightVision.Id,
                    ProductId2 = productScienceAndFaith.Id,
                });

            productPrideAndPrejudice.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPrideAndPrejudice.Id,
                    ProductId2 = productFirstPrizePies.Id,
                });

            productPrideAndPrejudice.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPrideAndPrejudice.Id,
                    ProductId2 = productFahrenheit.Id,
                });

            productFirstPrizePies.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFirstPrizePies.Id,
                    ProductId2 = productPrideAndPrejudice.Id,
                });

            productFirstPrizePies.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFirstPrizePies.Id,
                    ProductId2 = productFahrenheit.Id,
                });

            productFahrenheit.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFahrenheit.Id,
                    ProductId2 = productFirstPrizePies.Id,
                });

            productFahrenheit.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productFahrenheit.Id,
                    ProductId2 = productPrideAndPrejudice.Id,
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productSamsungSeries.Id,
                });

            productAsusN551JK.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAsusN551JK.Id,
                    ProductId2 = productHpSpectre.Id,
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productAsusN551JK.Id,
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });

            productLenovoThinkpad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoThinkpad.Id,
                    ProductId2 = productSamsungSeries.Id,
                });

            productLenovoThinkpad.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productLenovoThinkpad.Id,
                     ProductId2 = productHpEnvy.Id,
                 });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productSamsungSeries.Id,
                });

            productAppleMacBookPro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAppleMacBookPro.Id,
                    ProductId2 = productAsusN551JK.Id,
                });

            productAppleMacBookPro.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productAppleMacBookPro.Id,
                     ProductId2 = productHpSpectre.Id,
                 });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productSamsungSeries.Id,
                });

            productHpSpectre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpSpectre.Id,
                    ProductId2 = productAsusN551JK.Id,
                });

            productHpSpectre.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productHpSpectre.Id,
                     ProductId2 = productHpEnvy.Id,
                 });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productAsusN551JK.Id,
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productHpSpectre.Id,
                });

            productHpEnvy.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHpEnvy.Id,
                    ProductId2 = productSamsungSeries.Id,
                });
            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productAsusN551JK.Id,
                });
            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });

            productSamsungSeries.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSamsungSeries.Id,
                    ProductId2 = productHpEnvy.Id,
                });
            productSamsungSeries.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productSamsungSeries.Id,
                     ProductId2 = productHpSpectre.Id,
                 });
            productLeica.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productHtcOneMini.Id,
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productNikonD5500DSLR.Id,
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productAppleICam.Id,
                });

            productLeica.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeica.Id,
                    ProductId2 = productNokiaLumia.Id,
                });
            productHtcOne.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productHtcOneMini.Id,
                });

            productHtcOne.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productNokiaLumia.Id,
                });
            productHtcOne.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productBeatsPill.Id,
                });

            productHtcOne.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOne.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                });

            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productHtcOne.Id,
                });
            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productNokiaLumia.Id,
                });

            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productBeatsPill.Id,
                });
            productHtcOneMini.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productHtcOneMini.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productHtcOne.Id,
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productHtcOneMini.Id,
                });

            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productBeatsPill.Id,
                });
            productNokiaLumia.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productNokiaLumia.Id,
                    ProductId2 = productPortableSoundSpeakers.Id,
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productLeviJeans.Id,
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeFloral.Id,
                });

            productAdidas.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeZoom.Id,
                });
            productAdidas.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidas.Id,
                    ProductId2 = productNikeTailwind.Id,
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productAdidas.Id,
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeFloral.Id,
                });

            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeZoom.Id,
                });
            productLeviJeans.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLeviJeans.Id,
                    ProductId2 = productNikeTailwind.Id,
                });

            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productLeviJeans.Id,
                });

            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productNikeTailwind.Id,
                });
            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productOversizedWomenTShirt.Id,
                });
            productCustomTShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCustomTShirt.Id,
                    ProductId2 = productObeyHat.Id,
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productBuildComputer.Id,
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productLenovoIdeaCentre.Id,
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                });
            productDigitalStorm.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDigitalStorm.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productBuildComputer.Id,
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productDigitalStorm.Id,
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productLenovoThinkpad.Id,
                });

            productLenovoIdeaCentre.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaCentre.Id,
                    ProductId2 = productAppleMacBookPro.Id,
                });


            #endregion

            #region Product Tags

            //product tags
            await AddProductTag(product25GiftCard, "nice");
            await AddProductTag(product25GiftCard, "gift");
            await AddProductTag(productNikeTailwind, "cool");
            await AddProductTag(productNikeTailwind, "apparel");
            await AddProductTag(productNikeTailwind, "shirt");
            await AddProductTag(productBeatsPill, "computer");
            await AddProductTag(productBeatsPill, "cool");
            await AddProductTag(productNikeFloral, "cool");
            await AddProductTag(productNikeFloral, "shoes");
            await AddProductTag(productNikeFloral, "apparel");
            await AddProductTag(productAdobePhotoshop, "computer");
            await AddProductTag(productAdobePhotoshop, "awesome");
            await AddProductTag(productUniversalTabletCover, "computer");
            await AddProductTag(productUniversalTabletCover, "cool");
            await AddProductTag(productOversizedWomenTShirt, "cool");
            await AddProductTag(productOversizedWomenTShirt, "apparel");
            await AddProductTag(productOversizedWomenTShirt, "shirt");
            await AddProductTag(productAppleMacBookPro, "compact");
            await AddProductTag(productAppleMacBookPro, "awesome");
            await AddProductTag(productAppleMacBookPro, "computer");
            await AddProductTag(productAsusN551JK, "compact");
            await AddProductTag(productAsusN551JK, "awesome");
            await AddProductTag(productAsusN551JK, "computer");
            await AddProductTag(productFahrenheit, "awesome");
            await AddProductTag(productFahrenheit, "book");
            await AddProductTag(productFahrenheit, "nice");
            await AddProductTag(productHtcOne, "cell");
            await AddProductTag(productHtcOne, "compact");
            await AddProductTag(productHtcOne, "awesome");
            await AddProductTag(productBuildComputer, "awesome");
            await AddProductTag(productBuildComputer, "computer");
            await AddProductTag(productNikonD5500DSLR, "cool");
            await AddProductTag(productNikonD5500DSLR, "camera");
            await AddProductTag(productLeica, "camera");
            await AddProductTag(productLeica, "cool");
            await AddProductTag(productDigitalStorm, "cool");
            await AddProductTag(productDigitalStorm, "computer");
            await AddProductTag(productWindows8Pro, "awesome");
            await AddProductTag(productWindows8Pro, "computer");
            await AddProductTag(productCustomTShirt, "cool");
            await AddProductTag(productCustomTShirt, "shirt");
            await AddProductTag(productCustomTShirt, "apparel");
            await AddProductTag(productElegantGemstoneNecklace, "jewelry");
            await AddProductTag(productElegantGemstoneNecklace, "awesome");
            await AddProductTag(productFlowerGirlBracelet, "awesome");
            await AddProductTag(productFlowerGirlBracelet, "jewelry");
            await AddProductTag(productFirstPrizePies, "book");
            await AddProductTag(productAdidas, "cool");
            await AddProductTag(productAdidas, "shoes");
            await AddProductTag(productAdidas, "apparel");
            await AddProductTag(productLenovoIdeaCentre, "awesome");
            await AddProductTag(productLenovoIdeaCentre, "computer");
            await AddProductTag(productSamsungSeries, "nice");
            await AddProductTag(productSamsungSeries, "computer");
            await AddProductTag(productSamsungSeries, "compact");
            await AddProductTag(productHpSpectre, "nice");
            await AddProductTag(productHpSpectre, "computer");
            await AddProductTag(productHpEnvy, "computer");
            await AddProductTag(productHpEnvy, "cool");
            await AddProductTag(productHpEnvy, "compact");
            await AddProductTag(productObeyHat, "apparel");
            await AddProductTag(productObeyHat, "cool");
            await AddProductTag(productLeviJeans, "cool");
            await AddProductTag(productLeviJeans, "jeans");
            await AddProductTag(productLeviJeans, "apparel");
            await AddProductTag(productSoundForge, "game");
            await AddProductTag(productSoundForge, "computer");
            await AddProductTag(productSoundForge, "cool");
            await AddProductTag(productNightVision, "awesome");
            await AddProductTag(productNightVision, "digital");
            await AddProductTag(productSunglasses, "apparel");
            await AddProductTag(productSunglasses, "cool");
            await AddProductTag(productHtcOneMini, "awesome");
            await AddProductTag(productHtcOneMini, "compact");
            await AddProductTag(productHtcOneMini, "cell");
            await AddProductTag(productIfYouWait, "digital");
            await AddProductTag(productIfYouWait, "awesome");
            await AddProductTag(productNokiaLumia, "awesome");
            await AddProductTag(productNokiaLumia, "cool");
            await AddProductTag(productNokiaLumia, "camera");
            await AddProductTag(productScienceAndFaith, "digital");
            await AddProductTag(productScienceAndFaith, "awesome");
            await AddProductTag(productPrideAndPrejudice, "book");
            await AddProductTag(productLenovoThinkpad, "awesome");
            await AddProductTag(productLenovoThinkpad, "computer");
            await AddProductTag(productLenovoThinkpad, "compact");
            await AddProductTag(productNikeZoom, "jeans");
            await AddProductTag(productNikeZoom, "cool");
            await AddProductTag(productNikeZoom, "apparel");
            await AddProductTag(productEngagementRing, "jewelry");
            await AddProductTag(productEngagementRing, "awesome");


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
                var productReview = new ProductReview {
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
                await _productReviewRepository.InsertAsync(productReview);

                product.ApprovedRatingSum = rating;
                product.ApprovedTotalReviews = product.ApprovedTotalReviews + 1;

            }
            await _productRepository.UpdateAsync(allProducts);
        }

        protected virtual async Task InstallForums()
        {
            var forumGroup = new ForumGroup {
                Name = "General",
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            await _forumGroupRepository.InsertAsync(forumGroup);

            var newProductsForum = new Forum {
                ForumGroupId = forumGroup.Id,
                Name = "New Products",
                Description = "Discuss new products and industry trends",
                NumTopics = 0,
                NumPosts = 0,
                LastPostCustomerId = "",
                LastPostTime = null,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _forumRepository.InsertAsync(newProductsForum);

            var mobileDevicesForum = new Forum {
                ForumGroupId = forumGroup.Id,
                Name = "Mobile Devices Forum",
                Description = "Discuss the mobile phone market",
                NumTopics = 0,
                NumPosts = 0,
                LastPostCustomerId = "",
                LastPostTime = null,
                DisplayOrder = 10,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _forumRepository.InsertAsync(mobileDevicesForum);

            var packagingShippingForum = new Forum {
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
            await _forumRepository.InsertAsync(packagingShippingForum);
        }

        protected virtual async Task InstallDiscounts()
        {
            var discounts = new List<Discount>
                                {
                                    new Discount
                                        {
                                            Name = "Sample discount with coupon code",
                                            DiscountType = DiscountType.AssignedToOrderTotal,
                                            DiscountLimitation = DiscountLimitationType.Unlimited,
                                            UsePercentage = false,
                                            DiscountAmount = 10,
                                            RequiresCouponCode = true,
                                            IsEnabled = true,
                                        },
                                    new Discount
                                        {
                                            Name = "'20% order total' discount",
                                            DiscountType = DiscountType.AssignedToOrderTotal,
                                            DiscountLimitation = DiscountLimitationType.Unlimited,
                                            UsePercentage = true,
                                            DiscountPercentage = 20,
                                            StartDateUtc = new DateTime(2010,1,1),
                                            EndDateUtc = new DateTime(2030,1,1),
                                            RequiresCouponCode = true,
                                            IsEnabled = true
                                        },
                                };
            await _discountRepository.InsertAsync(discounts);
            var coupon1 = new DiscountCoupon {
                CouponCode = "123",
                DiscountId = _discountRepository.Table.Where(x => x.Name == "Sample discount with coupon code").FirstOrDefault().Id
            };
            await _discountCouponRepository.InsertAsync(coupon1);

            var coupon2 = new DiscountCoupon {
                CouponCode = "456",
                DiscountId = _discountRepository.Table.Where(x => x.Name == "'20% order total' discount").FirstOrDefault().Id
            };
            await _discountCouponRepository.InsertAsync(coupon2);

        }

        protected virtual async Task InstallBlogPosts()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var blogPosts = new List<BlogPost>
                                {
                                    new BlogPost
                                        {
                                             AllowComments = false,
                                             Title = "How a blog can help your growing e-Commerce business",
                                             BodyOverview = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p>",
                                             Body = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p><h3>1) Blog is useful in educating your customers</h3><p>Blogging is one of the best way by which you can educate your customers about your products/services that you offer. This helps you as a business owner to bring more value to your brand. When you provide useful information to the customers about your products, they are more likely to buy products from you. You can use your blog for providing tutorials in regard to the use of your products.</p><p><strong>For example:</strong> If you have an online store that offers computer parts. You can write tutorials about how to build a computer or how to make your computer&rsquo;s performance better. While talking about these things, you can mention products in the tutorials and provide link to your products within the blog post from your website. Your potential customers might get different ideas of using your product and will likely to buy products from your online store.</p><h3>2) Blog helps your business in Search Engine Optimization (SEO)</h3><p>Blog posts create more internal links to your website which helps a lot in SEO. Blog is a great way to have quality content on your website related to your products/services which is indexed by all major search engines like Google, Bing and Yahoo. The more original content you write in your blog post, the better ranking you will get in search engines. SEO is an on-going process and posting blog posts regularly keeps your site active all the time which is beneficial when it comes to search engine optimization.</p><p><strong>For example:</strong> Let&rsquo;s say you sell &ldquo;Sony Television Model XYZ&rdquo; and you regularly publish blog posts about your product. Now, whenever someone searches for &ldquo;Sony Television Model XYZ&rdquo;, Google will crawl on your website knowing that you have something to do with this particular product. Hence, your website will show up on the search result page whenever this item is being searched.</p><h3>3) Blog helps in boosting your sales by convincing the potential customers to buy</h3><p>If you own an online business, there are so many ways you can share different stories with your audience in regard your products/services that you offer. Talk about how you started your business, share stories that educate your audience about what&rsquo;s new in your industry, share stories about how your product/service was beneficial to someone or share anything that you think your audience might find interesting (it does not have to be related to your product). This kind of blogging shows that you are an expert in your industry and interested in educating your audience. It sets you apart in the competitive market. This gives you an opportunity to showcase your expertise by educating the visitors and it can turn your audience into buyers.</p><p><strong>Fun Fact:</strong> Did you know that 92% of companies who decided to blog acquired customers through their blog?</p><p><a href=\"http://www.grandnode.com/\">grandnode</a> is great e-Commerce solution that also offers a variety of CMS features including blog. A store owner has full access for managing the blog posts and related comments.</p>",
                                             Tags = "e-commerce, blog, moey",
                                             CreatedOnUtc = DateTime.UtcNow,
                                        },
                                    new BlogPost
                                        {
                                             AllowComments = false,
                                             Title = "Why your online store needs a wish list",
                                             BodyOverview = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p>",
                                             Body = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p><p>Does every e-Commerce store needs a wish list? The answer to this question in most cases is yes, because of the following reasons:</p><p><strong>Understanding the needs of your customers</strong> - A wish list is a great way to know what is in your customer&rsquo;s mind. Try to think the purchase history as a small portion of the customer&rsquo;s preferences. But, the wish list is like a wide open door that can give any online business a lot of valuable information about their customer and what they like or desire.</p><p><strong>Shoppers like to share their wish list with friends and family</strong> - Providing your customers a way to email their wish list to their friends and family is a pleasant way to make online shopping enjoyable for the shoppers. It is always a good idea to make the wish list sharable by a unique link so that it can be easily shared though different channels like email or on social media sites.</p><p><strong>Wish list can be a great marketing tool</strong> &ndash; Another way to look at wish list is a great marketing tool because it is extremely targeted and the recipients are always motivated to use it. For example: when your younger brother tells you that his wish list is on a certain e-Commerce store. What is the first thing you are going to do? You are most likely to visit the e-Commerce store, check out the wish list and end up buying something for your younger brother.</p><p>So, how a wish list is a marketing tool? The reason is quite simple, it introduce your online store to new customers just how it is explained in the above example.</p><p><strong>Encourage customers to return to the store site</strong> &ndash; Having a feature of wish list on the store site can increase the return traffic because it encourages customers to come back and buy later. Allowing the customers to save the wish list to their online accounts gives them a reason return to the store site and login to the account at any time to view or edit the wish list items.</p><p><strong>Wish list can be used for gifts for different occasions like weddings or birthdays. So, what kind of benefits a gift-giver gets from a wish list?</strong></p><ul><li>It gives them a surety that they didn&rsquo;t buy a wrong gift</li><li>It guarantees that the recipient will like the gift</li><li>It avoids any awkward moments when the recipient unwraps the gift and as a gift-giver you got something that the recipient do not want</li></ul><p><strong>Wish list is a great feature to have on a store site &ndash; So, what kind of benefits a business owner gets from a wish list</strong></p><ul><li>It is a great way to advertise an online store as many people do prefer to shop where their friend or family shop online</li><li>It allows the current customers to return to the store site and open doors for the new customers</li><li>It allows store admins to track what&rsquo;s in customers wish list and run promotions accordingly to target specific customer segments</li></ul><p><a href=\"http://www.grandnode.com/\">grandnode</a> offers the feature of wish list that allows customers to create a list of products that they desire or planning to buy in future.</p>",
                                             Tags = "e-commerce, grandnode, sample tag, money",
                                             CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                        },
                                };
            await _blogPostRepository.InsertAsync(blogPosts);

            //search engine names
            foreach (var blogPost in blogPosts)
            {
                var seName = SeoExtensions.GetSeName(blogPost.Title, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = blogPost.Id,
                    EntityName = "BlogPost",
                    LanguageId = "",
                    IsActive = true,
                    Slug = seName
                });
                blogPost.SeName = seName;
                await _blogPostRepository.UpdateAsync(blogPost);

            }
        }

        protected virtual async Task InstallBlogPosts(string defaultUserEmail)
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var blogPosts = new List<BlogPost>
                                {
                                    new BlogPost
                                        {
                                             AllowComments = false,
                                             Title = "How a blog can help your growing e-Commerce business",
                                             BodyOverview = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p>",
                                             Body = "<p>When you start an online business, your main aim is to sell the products, right? As a business owner, you want to showcase your store to more audience. So, you decide to go on social media, why? Because everyone is doing it, then why shouldn&rsquo;t you? It is tempting as everyone is aware of the hype that it is the best way to market your brand.</p><p>Do you know having a blog for your online store can be very helpful? Many businesses do not understand the importance of having a blog because they don&rsquo;t have time to post quality content.</p><p>Today, we will talk about how a blog can play an important role for the growth of your e-Commerce business. Later, we will also discuss some tips that will be helpful to you for writing business related blog posts.</p><h3>1) Blog is useful in educating your customers</h3><p>Blogging is one of the best way by which you can educate your customers about your products/services that you offer. This helps you as a business owner to bring more value to your brand. When you provide useful information to the customers about your products, they are more likely to buy products from you. You can use your blog for providing tutorials in regard to the use of your products.</p><p><strong>For example:</strong> If you have an online store that offers computer parts. You can write tutorials about how to build a computer or how to make your computer&rsquo;s performance better. While talking about these things, you can mention products in the tutorials and provide link to your products within the blog post from your website. Your potential customers might get different ideas of using your product and will likely to buy products from your online store.</p><h3>2) Blog helps your business in Search Engine Optimization (SEO)</h3><p>Blog posts create more internal links to your website which helps a lot in SEO. Blog is a great way to have quality content on your website related to your products/services which is indexed by all major search engines like Google, Bing and Yahoo. The more original content you write in your blog post, the better ranking you will get in search engines. SEO is an on-going process and posting blog posts regularly keeps your site active all the time which is beneficial when it comes to search engine optimization.</p><p><strong>For example:</strong> Let&rsquo;s say you sell &ldquo;Sony Television Model XYZ&rdquo; and you regularly publish blog posts about your product. Now, whenever someone searches for &ldquo;Sony Television Model XYZ&rdquo;, Google will crawl on your website knowing that you have something to do with this particular product. Hence, your website will show up on the search result page whenever this item is being searched.</p><h3>3) Blog helps in boosting your sales by convincing the potential customers to buy</h3><p>If you own an online business, there are so many ways you can share different stories with your audience in regard your products/services that you offer. Talk about how you started your business, share stories that educate your audience about what&rsquo;s new in your industry, share stories about how your product/service was beneficial to someone or share anything that you think your audience might find interesting (it does not have to be related to your product). This kind of blogging shows that you are an expert in your industry and interested in educating your audience. It sets you apart in the competitive market. This gives you an opportunity to showcase your expertise by educating the visitors and it can turn your audience into buyers.</p><p><strong>Fun Fact:</strong> Did you know that 92% of companies who decided to blog acquired customers through their blog?</p><p><a href=\"http://www.grandnode.com/\">Grandnode</a> is great e-Commerce solution that also offers a variety of CMS features including blog. A store owner has full access for managing the blog posts and related comments.</p>",
                                             Tags = "e-commerce, blog, moey",
                                             CreatedOnUtc = DateTime.UtcNow,
                                        },
                                    new BlogPost
                                        {
                                             AllowComments = false,
                                             Title = "Why your online store needs a wish list",
                                             BodyOverview = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p>",
                                             Body = "<p>What comes to your mind, when you hear the term&rdquo; wish list&rdquo;? The application of this feature is exactly how it sounds like: a list of things that you wish to get. As an online store owner, would you like your customers to be able to save products in a wish list so that they review or buy them later? Would you like your customers to be able to share their wish list with friends and family for gift giving?</p><p>Offering your customers a feature of wish list as part of shopping cart is a great way to build loyalty to your store site. Having the feature of wish list on a store site allows online businesses to engage with their customers in a smart way as it allows the shoppers to create a list of what they desire and their preferences for future purchase.</p><p>Does every e-Commerce store needs a wish list? The answer to this question in most cases is yes, because of the following reasons:</p><p><strong>Understanding the needs of your customers</strong> - A wish list is a great way to know what is in your customer&rsquo;s mind. Try to think the purchase history as a small portion of the customer&rsquo;s preferences. But, the wish list is like a wide open door that can give any online business a lot of valuable information about their customer and what they like or desire.</p><p><strong>Shoppers like to share their wish list with friends and family</strong> - Providing your customers a way to email their wish list to their friends and family is a pleasant way to make online shopping enjoyable for the shoppers. It is always a good idea to make the wish list sharable by a unique link so that it can be easily shared though different channels like email or on social media sites.</p><p><strong>Wish list can be a great marketing tool</strong> &ndash; Another way to look at wish list is a great marketing tool because it is extremely targeted and the recipients are always motivated to use it. For example: when your younger brother tells you that his wish list is on a certain e-Commerce store. What is the first thing you are going to do? You are most likely to visit the e-Commerce store, check out the wish list and end up buying something for your younger brother.</p><p>So, how a wish list is a marketing tool? The reason is quite simple, it introduce your online store to new customers just how it is explained in the above example.</p><p><strong>Encourage customers to return to the store site</strong> &ndash; Having a feature of wish list on the store site can increase the return traffic because it encourages customers to come back and buy later. Allowing the customers to save the wish list to their online accounts gives them a reason return to the store site and login to the account at any time to view or edit the wish list items.</p><p><strong>Wish list can be used for gifts for different occasions like weddings or birthdays. So, what kind of benefits a gift-giver gets from a wish list?</strong></p><ul><li>It gives them a surety that they didn&rsquo;t buy a wrong gift</li><li>It guarantees that the recipient will like the gift</li><li>It avoids any awkward moments when the recipient unwraps the gift and as a gift-giver you got something that the recipient do not want</li></ul><p><strong>Wish list is a great feature to have on a store site &ndash; So, what kind of benefits a business owner gets from a wish list</strong></p><ul><li>It is a great way to advertise an online store as many people do prefer to shop where their friend or family shop online</li><li>It allows the current customers to return to the store site and open doors for the new customers</li><li>It allows store admins to track what&rsquo;s in customers wish list and run promotions accordingly to target specific customer segments</li></ul><p><a href=\"http://www.grandnode.com/\">grandnode</a> offers the feature of wish list that allows customers to create a list of products that they desire or planning to buy in future.</p>",
                                             Tags = "e-commerce, grandnode, sample tag, money",
                                             CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                        },
                                };

            await _blogPostRepository.InsertAsync(blogPosts);

            //search engine names
            foreach (var blogPost in blogPosts)
            {
                blogPost.SeName = SeoExtensions.GetSeName(blogPost.Title, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = blogPost.Id,
                    EntityName = "BlogPost",
                    LanguageId = "",
                    IsActive = true,
                    Slug = blogPost.SeName
                });
                await _blogPostRepository.UpdateAsync(blogPost);
            }

        }
        protected virtual async Task InstallNews()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var news = new List<NewsItem>
                                {
                                    new NewsItem
                                    {
                                         AllowComments = false,
                                         Title = "About Grandnode",
                                         Short = "It's stable and highly usable. From downloads to documentation, www.grandnode.com offers a comprehensive base of information, resources, and support to the grandnode community.",
                                         Full = "<p>For full feature list go to <a href=\"http://www.grandnode.com\">grandnode.com</a></p><p>Providing outstanding custom search engine optimization, web development services and e-commerce development solutions to our clients at a fair price in a professional manner.</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow,
                                    },
                                    new NewsItem
                                    {
                                         AllowComments = false,
                                         Title = "Grandnode new release!",
                                         Short = "grandnode includes everything you need to begin your e-commerce online store. We have thought of everything and it's all included! grandnode is a fully customizable shopping cart",
                                         Full = "<p>Grandnode includes everything you need to begin your e-commerce online store. We have thought of everything and it's all included!</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                                    },
                                    new NewsItem
                                    {
                                         AllowComments = false,
                                         Title = "New online store is open!",
                                         Short = "The new grandnode store is open now! We are very excited to offer our new range of products. We will be constantly adding to our range so please register on our site.",
                                         Full = "<p>Our online store is officially up and running. Stock up for the holiday season! We have a great selection of items. We will be constantly adding to our range so please register on our site, this will enable you to keep up to date with any new products.</p><p>All shipping is worldwide and will leave the same day an order is placed! Happy Shopping and spread the word!!</p>",
                                         Published  = true,
                                         CreatedOnUtc = DateTime.UtcNow.AddSeconds(2),
                                    },

                                };
            await _newsItemRepository.InsertAsync(news);

            //search engine names
            foreach (var newsItem in news)
            {
                newsItem.SeName = SeoExtensions.GetSeName(newsItem.Title, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = newsItem.Id,
                    EntityName = "NewsItem",
                    LanguageId = "",
                    IsActive = true,
                    Slug = newsItem.SeName
                });
                await _newsItemRepository.UpdateAsync(newsItem);
            }

        }

        protected virtual async Task InstallPolls()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var poll1 = new Poll {
                Name = "Do you like Grandnode for MongoDB?",
                SystemKeyword = "",
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 1,
            };
            poll1.PollAnswers.Add(new PollAnswer {
                Name = "Like very much",
                DisplayOrder = 1,
            });
            poll1.PollAnswers.Add(new PollAnswer {
                Name = "Like",
                DisplayOrder = 2,
            });
            poll1.PollAnswers.Add(new PollAnswer {
                Name = "Neither Like nor Dislike",
                DisplayOrder = 3,
            });
            poll1.PollAnswers.Add(new PollAnswer {
                Name = "Dislike",
                DisplayOrder = 4,

            });
            poll1.PollAnswers.Add(new PollAnswer {
                Name = "Dislike very much",
                DisplayOrder = 5,

            });
            await _pollRepository.InsertAsync(poll1);
        }

        protected virtual async Task InstallActivityLogTypes()
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
                                                  SystemKeyword = "AddNewContactAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new contact attribute"
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
                                                  SystemKeyword = "AddNewDocument",
                                                  Enabled = false,
                                                  Name = "Add a new document"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewDocumentType",
                                                  Enabled = false,
                                                  Name = "Add a new document type"
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
                                                  SystemKeyword = "DeleteBid",
                                                  Enabled = true,
                                                  Name = "Delete bid"
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
                                                  SystemKeyword = "DeleteContactAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a contact attribute"
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
                                                  SystemKeyword = "DeleteDocument",
                                                  Enabled = false,
                                                  Name = "Delete document"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteDocumentType",
                                                  Enabled = false,
                                                  Name = "Delete document type"
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
                                                  SystemKeyword = "EditContactAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a contact attribute"
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
                                                  SystemKeyword = "EditDocument",
                                                  Enabled = false,
                                                  Name = "Edit document"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditDocumentType",
                                                  Enabled = false,
                                                  Name = "Edit document type"
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
                                                  SystemKeyword = "InteractiveFormDelete",
                                                  Enabled = true,
                                                  Name = "Delete a interactive form"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "InteractiveFormEdit",
                                                  Enabled = true,
                                                  Name = "Edit a interactive form"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "InteractiveFormAdd",
                                                  Enabled = true,
                                                  Name = "Add a interactive form"
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
                                                  SystemKeyword = "PublicStore.Url",
                                                  Enabled = false,
                                                  Name = "Public store. Viewed Url"
                                              },
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
                                                  SystemKeyword = "PublicStore.ViewCourse",
                                                  Enabled = false,
                                                  Name = "Public store. View a course"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewLesson",
                                                  Enabled = false,
                                                  Name = "Public store. View a lesson"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AskQuestion",
                                                  Enabled = false,
                                                  Name = "Public store. Ask a question about product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.InteractiveForm",
                                                  Enabled = false,
                                                  Name = "Public store. Show interactive form"
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
                                                  SystemKeyword = "PublicStore.AddNewBid",
                                                  Enabled = false,
                                                  Name = "Public store. Add new bid"
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
                                                  SystemKeyword = "PublicStore.AddArticleComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add article comment"
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
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteAccount",
                                                  Enabled = false,
                                                  Name = "Public store. Delete account"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.AbandonedCart",
                                                  Enabled = true,
                                                  Name = "Send email Customer reminder - AbandonedCart"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.RegisteredCustomer",
                                                  Enabled = true,
                                                  Name = "Send email Customer reminder - RegisteredCustomer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.LastActivity",
                                                  Enabled = true,
                                                  Name = "Send email Customer reminder - LastActivity"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.LastPurchase",
                                                  Enabled = true,
                                                  Name = "Send email Customer reminder - LastPurchase"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.Birthday",
                                                  Enabled = true,
                                                  Name = "Send email Customer reminder - Birthday"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerReminder.SendCampaign",
                                                  Enabled = true,
                                                  Name = "Send Campaign"
                                              },
                                           new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerAdmin.SendEmail",
                                                  Enabled = true,
                                                  Name = "Send email"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "CustomerAdmin.SendPM",
                                                  Enabled = true,
                                                  Name = "Send PM"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "UpdateKnowledgebaseCategory",
                                                  Enabled = true,
                                                  Name = "Update knowledgebase category"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "CreateKnowledgebaseCategory",
                                                  Enabled = true,
                                                  Name = "Create knowledgebase category"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteKnowledgebaseCategory",
                                                  Enabled = true,
                                                  Name = "Delete knowledgebase category"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "CreateKnowledgebaseArticle",
                                                  Enabled = true,
                                                  Name = "Create knowledgebase article"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "UpdateKnowledgebaseArticle",
                                                  Enabled = true,
                                                  Name = "Update knowledgebase article"
                                              },
                                            new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteKnowledgebaseArticle",
                                                  Enabled = true,
                                                  Name = "Delete knowledgebase category"
                                              },
                                      };
            await _activityLogTypeRepository.InsertAsync(activityLogTypes);
        }

        protected virtual async Task InstallProductTemplates()
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
            await _productTemplateRepository.InsertAsync(productTemplates);
        }

        protected virtual async Task InstallCategoryTemplates()
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
            await _categoryTemplateRepository.InsertAsync(categoryTemplates);
        }

        protected virtual async Task InstallManufacturerTemplates()
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
            await _manufacturerTemplateRepository.InsertAsync(manufacturerTemplates);
        }

        protected virtual async Task InstallTopicTemplates()
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
            await _topicTemplateRepository.InsertAsync(topicTemplates);
        }

        protected virtual async Task InstallScheduleTasks()
        {
            //these tasks are default - they are created in order to insert them into database
            //and nothing above it
            //there is no need to send arguments into ctor - all are null
            var tasks = new List<ScheduleTask>
            {
            new ScheduleTask
            {
                    ScheduleTaskName = "Send emails",
                    Type = "Grand.Services.Tasks.QueuedMessagesSendScheduleTask, Grand.Services",
                    Enabled = true,
                    StopOnError = false,
                    TimeInterval = 1
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Delete guests",
                    Type = "Grand.Services.Tasks.DeleteGuestsScheduleTask, Grand.Services",
                    Enabled = true,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Clear cache",
                    Type = "Grand.Services.Tasks.ClearCacheScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 120
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Clear log",
                    Type = "Grand.Services.Tasks.ClearLogScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Update currency exchange rates",
                    Type = "Grand.Services.Tasks.UpdateExchangeRateScheduleTask, Grand.Services",
                    Enabled = true,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - AbandonedCart",
                    Type = "Grand.Services.Tasks.CustomerReminderAbandonedCartScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 20
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - RegisteredCustomer",
                    Type = "Grand.Services.Tasks.CustomerReminderRegisteredCustomerScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - LastActivity",
                    Type = "Grand.Services.Tasks.CustomerReminderLastActivityScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - LastPurchase",
                    Type = "Grand.Services.Tasks.CustomerReminderLastPurchaseScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - Birthday",
                    Type = "Grand.Services.Tasks.CustomerReminderBirthdayScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - Completed order",
                    Type = "Grand.Services.Tasks.CustomerReminderCompletedOrderScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "Customer reminder - Unpaid order",
                    Type = "Grand.Services.Tasks.CustomerReminderUnpaidOrderScheduleTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
                },
                new ScheduleTask
                {
                    ScheduleTaskName = "End of the auctions",
                    Type = "Grand.Services.Tasks.EndAuctionsTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 60
                },
            };
            await _scheduleTaskRepository.InsertAsync(tasks);
        }

        protected virtual async Task InstallReturnRequestReasons()
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
            await _returnRequestReasonRepository.InsertAsync(returnRequestReasons);
        }
        protected virtual async Task InstallReturnRequestActions()
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
            await _returnRequestActionRepository.InsertAsync(returnRequestActions);
        }

        protected virtual async Task InstallWarehouses()
        {
            var warehouse1address = new Address {
                Address1 = "21 West 52nd Street",
                City = "New York",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _addressRepository.InsertAsync(warehouse1address);
            var warehouse2address = new Address {
                Address1 = "300 South Spring Stree",
                City = "Los Angeles",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "California").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "90013",
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _addressRepository.InsertAsync(warehouse2address);
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

            await _warehouseRepository.InsertAsync(warehouses);
        }

        protected virtual async Task InstallPickupPoints()
        {
            var addresspoint = new Address {
                Address1 = "21 West 52nd Street",
                City = "New York",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _addressRepository.InsertAsync(addresspoint);

            var point = new PickupPoint() {
                Address = addresspoint,
                Name = "My Store - New York",
            };
            await _pickupPointsRepository.InsertAsync(point);
        }

        protected virtual async Task InstallVendors()
        {
            var vendors = new List<Vendor>
            {
                new Vendor
                {
                    Name = "Vendor 1",
                    Email = "vendor1email@gmail.com",
                    Description = "Some description...",
                    AdminComment = "",
                    PictureId = "",
                    Active = true,
                    DisplayOrder = 1,
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = "6, 3, 9, 18",
                },
                new Vendor
                {
                    Name = "Vendor 2",
                    Email = "vendor2email@gmail.com",
                    Description = "Some description...",
                    AdminComment = "",
                    PictureId = "",
                    Active = true,
                    DisplayOrder = 2,
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = "6, 3, 9, 18",
                }
            };

            await _vendorRepository.InsertAsync(vendors);

            //search engine names
            foreach (var vendor in vendors)
            {
                var seName = SeoExtensions.GetSeName(vendor.Name, false, false);
                await _urlRecordRepository.InsertAsync(new UrlRecord {
                    EntityId = vendor.Id,
                    EntityName = "Vendor",
                    LanguageId = "",
                    IsActive = true,
                    Slug = seName
                });
                vendor.SeName = seName;
                await _vendorRepository.UpdateAsync(vendor);
            }
        }

        protected virtual async Task InstallAffiliates()
        {
            var affiliateAddress = new Address {
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
            await _addressRepository.InsertAsync(affiliateAddress);
            var affilate = new Affiliate {
                Active = true,
                Address = affiliateAddress
            };
            await _affiliateRepository.InsertAsync(affilate);
        }

        private async Task AddProductTag(Product product, string tag)
        {
            var productTag = _productTagRepository.Table.FirstOrDefault(pt => pt.Name == tag);
            if (productTag == null)
            {
                productTag = new ProductTag {
                    Name = tag,
                    SeName = SeoExtensions.GetSeName(tag, false, false),
                };

                await _productTagRepository.InsertAsync(productTag);
            }
            productTag.Count = productTag.Count + 1;
            await _productTagRepository.UpdateAsync(productTag);
            product.ProductTags.Add(productTag.Name);
            await _productRepository.UpdateAsync(product);
        }

        private async Task CreateIndexes()
        {
            //version
            await _versionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<GrandNodeVersion>((Builders<GrandNodeVersion>.IndexKeys.Ascending(x => x.DataBaseVersion)), new CreateIndexOptions() { Name = "DataBaseVersion", Unique = true }));

            //Store
            await _storeRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Store>((Builders<Store>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder" }));

            //Language
            await _lsrRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<LocaleStringResource>((Builders<LocaleStringResource>.IndexKeys.Ascending(x => x.LanguageId).Ascending(x => x.ResourceName)), new CreateIndexOptions() { Name = "Language" }));
            await _lsrRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<LocaleStringResource>((Builders<LocaleStringResource>.IndexKeys.Ascending(x => x.ResourceName)), new CreateIndexOptions() { Name = "ResourceName" }));

            //customer
            await _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Descending(x => x.CreatedOnUtc).Ascending(x => x.Deleted).Ascending("CustomerRoles._id")), new CreateIndexOptions() { Name = "CreatedOnUtc_1_CustomerRoles._id_1", Unique = false }));
            await _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.LastActivityDateUtc)), new CreateIndexOptions() { Name = "LastActivityDateUtc_1", Unique = false }));
            await _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.CustomerGuid)), new CreateIndexOptions() { Name = "CustomerGuid_1", Unique = false }));
            await _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email_1", Unique = false }));

            //customer role
            await _customerRoleProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerRoleProduct>((Builders<CustomerRoleProduct>.IndexKeys.Ascending(x => x.Id).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "CustomerRoleId_DisplayOrder", Unique = false }));

            //customer personalize product 
            await _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "CustomerProduct", Unique = false }));
            await _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "CustomerProduct_Unique", Unique = true }));

            //customer product price
            await _customerProductPriceRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProductPrice>((Builders<CustomerProductPrice>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "CustomerProduct", Unique = true }));

            //customer tag history
            await _customerTagProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerTagProduct>((Builders<CustomerTagProduct>.IndexKeys.Ascending(x => x.Id).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "CustomerTagId_DisplayOrder", Unique = false }));

            //customer history password
            await _customerHistoryPasswordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerHistoryPassword>((Builders<CustomerHistoryPassword>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId", Unique = false }));

            //customer note
            await _customerNoteRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerNote>((Builders<CustomerNote>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId", Unique = false, Background = true }));

            //user api
            await _userapiRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<UserApi>((Builders<UserApi>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email", Unique = true, Background = true }));

            //specificationAttribute
            await _specificationAttributeRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<SpecificationAttribute>((Builders<SpecificationAttribute>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder" }));

            //checkoutAttribute
            await _checkoutAttributeRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CheckoutAttribute>((Builders<CheckoutAttribute>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder" }));

            //category
            await _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.ShowOnHomePage).Ascending(x => x.Published).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "ShowOnHomePage_DisplayOrder_1", Unique = false }));
            await _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.ParentCategoryId).Ascending(x => x.Published).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "ParentCategoryId_1_DisplayOrder_1", Unique = false }));
            await _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.FeaturedProductsOnHomaPage).Ascending(x => x.Published).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "FeaturedProductsOnHomaPage_DisplayOrder_1", Unique = false }));

            //manufacturer
            await _manufacturerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Manufacturer>((Builders<Manufacturer>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder_1", Unique = false }));
            await _manufacturerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Manufacturer>((Builders<Manufacturer>.IndexKeys.Ascending("AppliedDiscounts")), new CreateIndexOptions() { Name = "AppliedDiscounts._id_1", Unique = false }));

            //Product
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.MarkAsNew).Ascending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "MarkAsNew_1_CreatedOnUtc_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.ShowOnHomePage).Ascending(x => x.DisplayOrder).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ShowOnHomePage_1_Published_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.ParentGroupedProductId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "ParentGroupedProductId_1_DisplayOrder_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.ProductTags).Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductTags._id_1_Name_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Name)), new CreateIndexOptions() { Name = "Name_1", Unique = false }));

            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending("ProductCategories.DisplayOrder")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_DisplayOrder_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.DisplayOrderCategory)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_OrderCategory_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Name_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Sold)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Sold_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending("ProductCategories.IsFeaturedProduct").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_IsFeaturedProduct_1", Unique = false }));

            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.DisplayOrderManufacturer)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_OrderCategory_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_Name_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Sold)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_Sold_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending("ProductManufacturers.IsFeaturedProduct").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_IsFeaturedProduct_1", Unique = false }));

            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending("ProductSpecificationAttributes.SpecificationAttributeOptionId").Ascending("ProductSpecificationAttributes.AllowFiltering")), new CreateIndexOptions() { Name = "ProductSpecificationAttributes", Unique = false }));

            //productreseration
            await _productReservationRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReservation>((Builders<ProductReservation>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.Date)), new CreateIndexOptions() { Name = "ProductReservation", Unique = false }));

            //bid
            await _bidRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CustomerId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductCustomer", Unique = false }));
            await _bidRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductDate", Unique = false }));

            //ProductReview
            await _productReviewRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReview>((Builders<ProductReview>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "ProductId", Unique = false }));

            //Recently Viewed Products
            await _recentlyViewedProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<RecentlyViewedProduct>((Builders<RecentlyViewedProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId.ProductId" }));

            //Product also purchased
            await _productalsopurchasedRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductAlsoPurchased>((Builders<ProductAlsoPurchased>.IndexKeys.Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "ProductId", Unique = false, Background = true }));

            //url record
            await _urlRecordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<UrlRecord>((Builders<UrlRecord>.IndexKeys.Ascending(x => x.Slug).Ascending(x => x.IsActive)), new CreateIndexOptions() { Name = "Slug" }));
            await _urlRecordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<UrlRecord>((Builders<UrlRecord>.IndexKeys.Ascending(x => x.EntityId).Ascending(x => x.EntityName).Ascending(x => x.LanguageId).Ascending(x => x.IsActive)), new CreateIndexOptions() { Name = "UrlRecord" }));


            //message template
            await _messageTemplateRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<MessageTemplate>((Builders<MessageTemplate>.IndexKeys.Ascending(x => x.Name)), new CreateIndexOptions() { Name = "Name", Unique = false }));

            //forum
            await _forumPostVote.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ForumPostVote>((Builders<ForumPostVote>.IndexKeys.Ascending(x => x.ForumPostId).Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "Vote", Unique = true }));

            // Country and Stateprovince
            await _countryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Country>((Builders<Country>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder" }));
            await _stateProvinceRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<StateProvince>((Builders<StateProvince>.IndexKeys.Ascending(x => x.CountryId).Ascending(x => x.DisplayOrder).Ascending(x => x.Id)), new CreateIndexOptions() { Name = "Country" }));

            //discount
            await _discountCouponRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountCoupon>((Builders<DiscountCoupon>.IndexKeys.Ascending(x => x.CouponCode)), new CreateIndexOptions() { Name = "CouponCode", Unique = true }));
            await _discountCouponRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountCoupon>((Builders<DiscountCoupon>.IndexKeys.Ascending(x => x.DiscountId)), new CreateIndexOptions() { Name = "DiscountId", Unique = false }));

            await _discountusageRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountUsageHistory>((Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "CustomerId" }));
            await _discountusageRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountUsageHistory>((Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.DiscountId)), new CreateIndexOptions() { Name = "DiscountId" }));
            await _discountusageRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountUsageHistory>((Builders<DiscountUsageHistory>.IndexKeys.Ascending(x => x.OrderId)), new CreateIndexOptions() { Name = "OrderId" }));

            //knowledgebase
            await _knowledgebaseArticleRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseArticle>((Builders<KnowledgebaseArticle>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));
            await _knowledgebaseCategoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseCategory>((Builders<KnowledgebaseCategory>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));

            //topic
            await _topicRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Topic>((Builders<Topic>.IndexKeys.Ascending(x => x.SystemName)), new CreateIndexOptions() { Name = "SystemName", Unique = false }));

            //news
            await _newsItemRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsItem>((Builders<NewsItem>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false }));

            //newsletter
            await _newslettersubscriptionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsLetterSubscription>((Builders<NewsLetterSubscription>.IndexKeys.Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "CustomerId", Unique = false }));
            await _newslettersubscriptionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsLetterSubscription>((Builders<NewsLetterSubscription>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email", Unique = false }));

            //Log
            await _logRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Log>((Builders<Log>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false }));

            //Campaign history
            await _campaignHistoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CampaignHistory>((Builders<CampaignHistory>.IndexKeys.Ascending(x => x.CampaignId).Descending(x => x.CreatedDateUtc)), new CreateIndexOptions() { Name = "CampaignId", Unique = false }));

            //reward points
            await _rewardpointshistoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<RewardPointsHistory>((Builders<RewardPointsHistory>.IndexKeys.Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "CustomerId" }));

            //search term
            await _searchtermRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<SearchTerm>((Builders<SearchTerm>.IndexKeys.Descending(x => x.Count)), new CreateIndexOptions() { Name = "Count", Unique = false }));

            //setting
            await _settingRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Setting>((Builders<Setting>.IndexKeys.Ascending(x => x.Name)), new CreateIndexOptions() { Name = "Name", Unique = false }));

            //shipment
            await _shipmentRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Shipment>((Builders<Shipment>.IndexKeys.Ascending(x => x.ShipmentNumber)), new CreateIndexOptions() { Name = "ShipmentNumber", Unique = true }));
            await _shipmentRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Shipment>((Builders<Shipment>.IndexKeys.Ascending(x => x.OrderId)), new CreateIndexOptions() { Name = "OrderId" }));

            //order
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId_1_CreatedOnUtc_-1", Unique = false }));
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc_-1", Unique = false }));
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Descending(x => x.OrderNumber)), new CreateIndexOptions() { Name = "OrderNumber", Unique = false }));
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending(x => x.Code)), new CreateIndexOptions() { Name = "OrderCode", Unique = false }));
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending("OrderItems.ProductId")), new CreateIndexOptions() { Name = "OrderItemsProductId" }));
            await _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending("OrderItems._id")), new CreateIndexOptions() { Name = "OrderItemId" }));

            await _orderNoteRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<OrderNote>((Builders<OrderNote>.IndexKeys.Ascending(x => x.OrderId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "Id", Unique = false, Background = true }));

            //permision
            await _permissionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<PermissionRecord>((Builders<PermissionRecord>.IndexKeys.Ascending(x => x.SystemName)), new CreateIndexOptions() { Name = "SystemName", Unique = true }));
            await _permissionAction.Collection.Indexes.CreateOneAsync(new CreateIndexModel<PermissionAction>((Builders<PermissionAction>.IndexKeys.Ascending(x => x.SystemName)), new CreateIndexOptions() { Name = "SystemName", Unique = false }));

            //externalauth
            await _externalAuthenticationRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ExternalAuthenticationRecord>((Builders<ExternalAuthenticationRecord>.IndexKeys.Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "CustomerId" }));

            //return request
            await _returnRequestRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ReturnRequest>((Builders<ReturnRequest>.IndexKeys.Ascending(x => x.ReturnNumber)), new CreateIndexOptions() { Name = "ReturnNumber", Unique = true }));
            await _returnRequestNoteRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ReturnRequestNote>((Builders<ReturnRequestNote>.IndexKeys.Ascending(x => x.ReturnRequestId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "Id", Unique = false, Background = true }));

            //contactus
            await _contactUsRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ContactUs>((Builders<ContactUs>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email", Unique = false }));
            await _contactUsRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ContactUs>((Builders<ContactUs>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false }));

            //customer action
            await _customerAction.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerAction>((Builders<CustomerAction>.IndexKeys.Ascending(x => x.ActionTypeId)), new CreateIndexOptions() { Name = "ActionTypeId", Unique = false }));

            await _customerActionHistory.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerActionHistory>((Builders<CustomerActionHistory>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.CustomerActionId)), new CreateIndexOptions() { Name = "Customer_Action", Unique = false }));

            //banner
            await _popupArchive.Collection.Indexes.CreateOneAsync(new CreateIndexModel<PopupArchive>((Builders<PopupArchive>.IndexKeys.Ascending(x => x.CustomerActionId)), new CreateIndexOptions() { Name = "CustomerActionId", Unique = false }));

            //customer reminder
            await _customerReminderHistoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerReminderHistory>((Builders<CustomerReminderHistory>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.CustomerReminderId)), new CreateIndexOptions() { Name = "CustomerId", Unique = false }));
        }

        private async Task CreateTables(string local)
        {
            if (string.IsNullOrEmpty(local))
                local = "en";

            try
            {
                var options = new CreateCollectionOptions();
                var collation = new Collation(local);
                options.Collation = collation;
                var dataSettingsManager = new DataSettingsManager();
                var connectionString = dataSettingsManager.LoadSettings().DataConnectionString;

                var mongourl = new MongoUrl(connectionString);
                var databaseName = mongourl.DatabaseName;
                var mongodb = new MongoClient(connectionString).GetDatabase(databaseName);
                var mongoDBContext = new MongoDBContext(mongodb);

                var typeFinder = _serviceProvider.GetRequiredService<ITypeFinder>();
                var q = typeFinder.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Grand.Core");
                foreach (var item in q.GetTypes().Where(x => x.Namespace != null && x.Namespace.StartsWith("Grand.Domain")))
                {
                    if (item.BaseType != null && item.IsClass && item.BaseType == typeof(BaseEntity))
                        await mongoDBContext.Database().CreateCollectionAsync(item.Name, options);
                }
            }
            catch (Exception ex)
            {
                throw new GrandException(ex.Message);
            }
        }

        #endregion

        #region Methods


        public virtual async Task InstallData(string defaultUserEmail,
            string defaultUserPassword, string collation, bool installSampleData = true)
        {

            defaultUserEmail = defaultUserEmail.ToLower();
            await CreateTables(collation);
            await CreateIndexes();
            await InstallVersion();
            await InstallStores();
            await InstallMeasures();
            await InstallTaxCategories();
            await InstallLanguages();
            await InstallCurrencies();
            await InstallCountriesAndStates();
            await InstallShippingMethods();
            await InstallDeliveryDates();
            await InstallCustomersAndUsers(defaultUserEmail, defaultUserPassword);
            await InstallEmailAccounts();
            await InstallMessageTemplates();
            await InstallCustomerAction();
            await InstallSettings(installSampleData);
            await InstallTopicTemplates();
            await InstallTopics();
            await InstallLocaleResources();
            await InstallActivityLogTypes();
            await HashDefaultCustomerPassword(defaultUserEmail, defaultUserPassword);
            await InstallProductTemplates();
            await InstallCategoryTemplates();
            await InstallManufacturerTemplates();
            await InstallScheduleTasks();
            await InstallReturnRequestReasons();
            await InstallReturnRequestActions();
            if (installSampleData)
            {
                await InstallCheckoutAttributes();
                await InstallSpecificationAttributes();
                await InstallProductAttributes();
                await InstallCategories();
                await InstallManufacturers();
                await InstallProducts(defaultUserEmail);
                await InstallForums();
                await InstallDiscounts();
                await InstallBlogPosts();
                await InstallNews();
                await InstallPolls();
                await InstallWarehouses();
                await InstallPickupPoints();
                await InstallVendors();
                await InstallAffiliates();
                await InstallOrderTags();
            }
        }


        #endregion

    }
}
