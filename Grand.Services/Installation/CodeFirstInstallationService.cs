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

        protected virtual async Task InstallStores(string companyName, string companyAddress, string companyPhoneNumber, string companyEmail)
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
                    CompanyName = companyName,
                    CompanyAddress = companyAddress,
                    CompanyPhoneNumber = companyPhoneNumber,
                    CompanyVat = null,
                    CompanyEmail = companyEmail,
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
                                           Name = "Lego",
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
                                           Name = "Balls",
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
                    NumberDecimal = 2,
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
                    NumberDecimal = 2,
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
                    NumberDecimal = 2,
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
                    NumberDecimal = 2,
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
                    NumberDecimal = 2,
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
                    NumberDecimal = 2,
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
                                        SubjectToVat = false,
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

            var crSalesManager = new CustomerRole {
                Name = "Sales manager",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.SalesManager,
            };
            await _customerRoleRepository.InsertAsync(crSalesManager);

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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
                                       },
                                   new Topic
                                       {
                                           SystemName = "PageNotFound",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           DisplayOrder = 1,
                                           Title = "",
                                           Body = "<p><strong>The page you requested was not found, and we have a fine guess why.</strong></p><ul><li>If you typed the URL directly, please make sure the spelling is correct.</li><li>The page longer exists. In this case, we profusely apologize for the inconvenience and for any damage this may cause.</li></ul>",
                                           TopicTemplateId = defaultTopicTemplate.Id,
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                                           Published = true
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
                DisplayNewProductsMenu = true,
                DisplaySearchMenu = !installSampleData,
                DisplayCustomerMenu = !installSampleData,
                DisplayBlogMenu = true,
                DisplayForumsMenu = !installSampleData,
                DisplayContactUsMenu = true
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
                ProductDetailsPictureSize = 800,
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
                        "home",
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
                IgnoreFilterableSpecAttributeOption = false,
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
                SecondPictureOnCatalogPages = true
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
                GeoEnabled = false,
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
                DeactivateGiftCardsAfterDeletingOrder = true,
                DeactivateGiftCardsAfterCancelOrder = true,
                GiftCards_Activated_OrderStatusId = 30,
                CompleteOrderWhenDelivered = true,
                UserCanCancelUnpaidOrder = false,
                LengthCode = 8
            });

            await _settingService.SaveSetting(new ShippingSettings {
                ActiveShippingRateComputationMethodSystemNames = new List<string> { "Shipping.FixedRate" },
                ShipToSameAddress = true,
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
                        "Payments.CashOnDelivery",
                        "Payments.PayPalStandard",
                        "Payments.BrainTree",
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
                CalculateRoundPrice = 2,
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
                SeName = SeoExtensions.GenerateSlug("Screensize", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa1);

            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "13.0''",
                DisplayOrder = 2,
                SeName = SeoExtensions.GenerateSlug("13.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "13.3''",
                DisplayOrder = 3,
                SeName = SeoExtensions.GenerateSlug("13.3''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "14.0''",
                DisplayOrder = 4,
                SeName = SeoExtensions.GenerateSlug("14.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "15.0''",
                DisplayOrder = 4,
                SeName = SeoExtensions.GenerateSlug("15.0''", false, false),
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "15.6''",
                DisplayOrder = 5,
                SeName = SeoExtensions.GenerateSlug("15.6''", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa1);

            var sa2 = new SpecificationAttribute {
                Name = "CPU Type",
                DisplayOrder = 2,
                SeName = SeoExtensions.GenerateSlug("CPU Type", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa2);

            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "Intel Core i5",
                DisplayOrder = 1,
                SeName = SeoExtensions.GenerateSlug("Intel Core i5", false, false),
            });

            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "Intel Core i7",
                DisplayOrder = 2,
                SeName = SeoExtensions.GenerateSlug("Intel Core i7", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa2);

            var sa3 = new SpecificationAttribute {
                Name = "Memory",
                DisplayOrder = 3,
                SeName = SeoExtensions.GenerateSlug("Memory", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa3);

            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "4 GB",
                DisplayOrder = 1,
                SeName = SeoExtensions.GenerateSlug("4 GB", false, false),
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "8 GB",
                DisplayOrder = 2,
                SeName = SeoExtensions.GenerateSlug("8 GB", false, false),
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "16 GB",
                DisplayOrder = 3,
                SeName = SeoExtensions.GenerateSlug("16 GB", false, false),
            });
            await _specificationAttributeRepository.UpdateAsync(sa3);

            var sa4 = new SpecificationAttribute {
                Name = "Hardrive",
                DisplayOrder = 5,
                SeName = SeoExtensions.GenerateSlug("Hardrive", false, false),
            };
            await _specificationAttributeRepository.InsertAsync(sa4);

            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "128 GB",
                DisplayOrder = 7,
                SeName = SeoExtensions.GenerateSlug("128 GB", false, false),
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "500 GB",
                DisplayOrder = 4,
                SeName = SeoExtensions.GenerateSlug("500 GB", false, false),
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption {
                Name = "1 TB",
                DisplayOrder = 3,
                SeName = SeoExtensions.GenerateSlug("1 TB", false, false),
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

            var categoryTablets = new Category {
                Name = "Tablets",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_tablets.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Tablets"))).Id,
                PriceRanges = "-1000;1000-1200;1200-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryTablets);

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

            var categorySmartwatches = new Category {
                Name = "Smartwatches",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryComputers.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_smartwatches.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Smartwatches"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categorySmartwatches);

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
                ShowOnHomePage = false,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryElectronics);

            var categoryDisplay = new Category {
                Name = "Display",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_display.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Display"))).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryDisplay);

            var categorySmartphones = new Category {
                Name = "Smartphones",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categoryElectronics.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_smartphones.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Smartphones"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categorySmartphones);

            var categoryOthers = new Category {
                Name = "Others",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                ShowOnHomePage = true,
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

            var categorySport = new Category {
                Name = "Sport",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 9,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9, 12",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_sport.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Sport"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                ShowOnHomePage = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categorySport);

            var categoryShoes = new Category {
                Name = "Shoes",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categorySport.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_shoes.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Shoes"))).Id,
                PriceRanges = "-500;500-;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryShoes);

            var categoryApparel = new Category {
                Name = "Apparel",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                ParentCategoryId = categorySport.Id,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_sport.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Apparel"))).Id,
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryApparel);

            var categoryBalls = new Category {
                Name = "Balls",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 6,
                ParentCategoryId = categorySport.Id,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_balls.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Balls"))).Id,
                PriceRanges = "0-500;500-700;700-3000;",
                IncludeInTopMenu = true,
                Published = true,
                DisplayOrder = 6,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            allCategories.Add(categoryBalls);

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

            var categoryLego = new Category {
                Name = "Lego",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MetaKeywords = "Lego, Dictionary, Textbooks",
                MetaDescription = "Books category description",
                PageSize = 6,
                ParentCategoryId = "",
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_lego.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Lego"))).Id,
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
            allCategories.Add(categoryLego);

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
                category.SeName = SeoExtensions.GenerateSlug(category.Name, false, false);
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
            var manufacturerXiaomi = new Manufacturer {
                Name = "Xiaomi",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_xiaomi.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Xiaomi"))).Id,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerXiaomi);
            allManufacturers.Add(manufacturerXiaomi);


            var manufacturerDell = new Manufacturer {
                Name = "Dell",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_dell.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Dell"))).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerDell);
            allManufacturers.Add(manufacturerDell);


            var manufacturerAdidas = new Manufacturer {
                Name = "Adidas",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9",
                Published = true,
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_adidas.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Adidas"))).Id,
                DisplayOrder = 5,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _manufacturerRepository.InsertAsync(manufacturerAdidas);
            allManufacturers.Add(manufacturerAdidas);

            //search engine names
            foreach (var manufacturer in allManufacturers)
            {
                manufacturer.SeName = SeoExtensions.GenerateSlug(manufacturer.Name, false, true);
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

            #region Computers


            var productBuildComputer = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Build your own computer",
                ShortDescription = "Build it",
                FullDescription = "<p>Fight back against cluttered workspaces with the stylish DELL Inspiron desktop PC, featuring powerful computing resources and a stunning 20.1-inch widescreen display with stunning XBRITE-HiColor LCD technology. The black IBM zBC12 has a built-in microphone and MOTION EYE camera with face-tracking technology that allows for easy communication with friends and family. And it has a built-in DVD burner and Sony's Movie Store software so you can create a digital entertainment library for personal viewing at your convenience. Easy to setup and even easier to use, this JS-series All-in-One includes an elegantly designed keyboard and a USB mouse.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Others").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productBuildComputer);

            var Picture1 = await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_desktop_1.png"), "image/png", pictureService.GetPictureSeName(productBuildComputer.Name));
            var Picture2 = await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_desktop_2.png"), "image/png", pictureService.GetPictureSeName(productBuildComputer.Name));

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

            var productSonyPS5Pad = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Playstation 5 Gamepad",
                ShortDescription = "The DualSense wireless controller for PS5 offers realistic touch effects2, adaptive 'Trigger' effects and a built-in microphone - all integrated into an iconic design.",
                FullDescription = "<p>The DualSense wireless controller for PS5 offers realistic touch effects2, adaptive 'Trigger' effects and a built-in microphone - all integrated into an iconic design.</p><p>Feel a physical reaction to your in-game actions thanks to dual actuators that replace traditional vibration motors. Such dynamic vibrations in your hands can simulate the tactile sensations of many things, from the world around you to the recoil of various weapons.</p><p>Enjoy intuitive motion controls for selected games thanks to the built-in accelerometer and gyroscope.</p><p>Chat with friends online using the built-in microphone or by plugging a headset into the 3.5mm jack. With the dedicated MUTE button you can disable voice recording.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 59M,
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

            allProducts.Add(productSonyPS5Pad);
            productSonyPS5Pad.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps5_pad_1.png"), "image/png", pictureService.GetPictureSeName(productSonyPS5Pad.Name))).Id,
                DisplayOrder = 1,
            });
            productSonyPS5Pad.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps5_pad_2.png"), "image/png", pictureService.GetPictureSeName(productSonyPS5Pad.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productSonyPS5Pad);

            var productLenovoIdeaPadDual = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo IdeaPad Dual 3i",
                ShortDescription = "Get dependable performance for work and play from the Dual 3i’s Intel® Pentium® processor, which gives you the ability to effortlessly multitask with multi-screen capabilities.",
                FullDescription = "<p>Get dependable performance for work and play from the Duet 3i’s Intel® Pentium® processor, which gives you the ability to effortlessly multitask with multi-screen capabilities, communicate easily with friends and family, and take all your favorite entertainment to go.</p><p>Work more freely on the elegant IdeaPad Duet 3i than on a regular laptop. The detachable Bluetooth keyboard allows you to easily switch between laptop and tablet modes, and the stand makes it easy to position your computer on any surface. The laptop runs on the power of an Intel® Pentium® processor, and the HD touchscreen, Dolby Audio ™ sound and optional LTE connectivity will keep you entertained anywhere.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 99M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Tablets").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLenovoIdeaPadDual);
            productLenovoIdeaPadDual.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_ideapad_dual_1.png"), "image/png", pictureService.GetPictureSeName(productLenovoIdeaPadDual.Name))).Id,
                DisplayOrder = 1,
            });
            productLenovoIdeaPadDual.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_ideapad_dual_2.png"), "image/png", pictureService.GetPictureSeName(productLenovoIdeaPadDual.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productLenovoIdeaPadDual);

            #endregion

            #region Notebooks

            var productMiNotebook = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Mi Notebook 14",
                ShortDescription = "Designed with utmost patience and craftsmanship, the Mi NoteBook 14 is so beautiful that you can't help but notice it. Weighing just 1.5kg, the sleek unibody metal chassis and an anodized sandblasted coating makes your device sturdy and gives it a svelte look.",
                FullDescription = "<p>Designed with utmost patience and craftsmanship, the Mi NoteBook 14 is so beautiful that you can't help but notice it. Weighing just 1.5kg, the sleek unibody metal chassis and an anodized sandblasted coating makes your device sturdy and gives it a slim look.</p><p>With the power-efficient NVIDIA® GeForce® MX250 graphics, now enjoy incredible HD photo and video editing, faster and smoother gaming. The powerful graphics engine and next-gen technologies gives you performance you desire.</p><p>The Mi Notebook 14 offers great clock speeds at 2666MHz, and thus makes you say goodbye to slow and insufficient memory. This helps you multi-task with your favorite editing/productivity tools and casual games.</p><p>Comes with a wider air intake area of 2530mm² and a large diameter fan which brings excellent cooling to the whole machine. This keeps your machine cool so that you can hold onto yours. The maximum sound of the fan is a mere 37 dB even when the system is loaded to its max.</p><p>Based on the scissor mechanism, the keys have ABS texture and a travel distance of 1.3mm which makes typing a lot more comfortable and low-profile on this device. The in-built dust protection layer is also an excellent addition.</p>",
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
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
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
            allProducts.Add(productMiNotebook);
            productMiNotebook.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_mi_notebook_1.png"), "image/png", pictureService.GetPictureSeName(productMiNotebook.Name))).Id,
                DisplayOrder = 1,
            });
            productMiNotebook.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_mi_notebook_2.png"), "image/png", pictureService.GetPictureSeName(productMiNotebook.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productMiNotebook);


            var productLenovoLegionY740 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo Legion Y740",
                ShortDescription = "The 17.3 ”Y740 Legion is a gaming masterpiece. You will be immersed in the action thanks to best-in-class Corsair iCUE lighting and Dolby realistic image and surround sound technologies.",
                FullDescription = "<p>The 17.3 ”Y740 Legion is a gaming masterpiece. You will be immersed in the action thanks to best-in-class Corsair iCUE lighting and Dolby realistic image and surround sound technologies. Check out what the Lenovo Legion Y740 looks like in reality.Grab the photo below and drag it left or right to rotate the product, or use the navigation buttons.</p>",
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
            allProducts.Add(productLenovoLegionY740);
            productLenovoLegionY740.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_legion_y740_1.png"), "image/png", pictureService.GetPictureSeName(productLenovoLegionY740.Name))).Id,
                DisplayOrder = 1,
            });
            productLenovoLegionY740.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_legion_y740_2.png"), "image/png", pictureService.GetPictureSeName(productLenovoLegionY740.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLenovoLegionY740);


            var productPs5Camera = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Playstation 5 Camera",
                ShortDescription = "Use the new Sony HD Camera for PlayStation 5 to show other players your reactions during the game.",
                FullDescription = "<p>Use the new Sony HD Camera for PlayStation 5 to show other players your reactions during the game. Equipped with two lenses, the camera can record images in 1080p quality and works seamlessly with the PS5 background removal tools. They put you in the spotlight of viewers. In addition, the camera has been equipped with a stand that makes it easy to mount it above or below the TV.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 150M,
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
            allProducts.Add(productPs5Camera);
            productPs5Camera.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ps5_camera_1.png"), "image/png", pictureService.GetPictureSeName(productPs5Camera.Name))).Id,
                DisplayOrder = 1,
            });
            productPs5Camera.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ps5_camera_2.png"), "image/png", pictureService.GetPictureSeName(productPs5Camera.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productPs5Camera);

            var productAcerNitro = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Acer Nitro 5",
                ShortDescription = "Experience a new dimension of gameplay with the Acer Nitro 5 laptop. Equipped with a powerful Intel Core i5 processor and GTX1050 graphics card, it is able to cope with even the most demanding tasks.",
                FullDescription = "<p>Experience a new dimension of gameplay with the Acer Nitro 5 laptop. Equipped with a powerful Intel Core i5 processor and GTX1050 graphics card, it is able to cope with even the most demanding tasks. Additionally, the matrix in IPS technology will ensure high quality of the displayed image, good color reproduction and wide viewing angles. The Acer Nitro 5 laptop is the perfect choice for both gaming and work. Dominate the virtual battlefield with the GeForce GTX 1050 graphics card, featuring the groundbreaking NVIDIA Pascal architecture. Excellent performance, innovative gaming technologies and support for DirectX 12 libraries will allow you to immerse yourself in phenomenal 4K resolution, enriched with HDR mode, or play in an amazingly realistic VR scenery. Every time without cuts or delays, every time on high details. Play the latest, most challenging games the way they deserve.</p>",
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
            allProducts.Add(productAcerNitro);
            productAcerNitro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_nitro_1.png"), "image/png", pictureService.GetPictureSeName(productAcerNitro.Name))).Id,
                DisplayOrder = 1,
            });
            productAcerNitro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_nitro_2.png"), "image/png", pictureService.GetPictureSeName(productAcerNitro.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productAcerNitro);


            var productDellG5 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Dell Inspiron G5",
                ShortDescription = "The Dell Inspiron G5 is a 15-inch gaming notebook with fantastic graphics capabilities, thanks to the NVIDIA® GeForce® GTX 1060 Max-Q graphics card, the latest 8th generation Intel® Core ™ i7 hexa-core processor and efficient DDR4 2666MHz RAM memory. The Inspiron G5 is designed specifically with the specific, demanding needs of gaming enthusiasts in mind.",
                FullDescription = "The Dell Inspiron G5 is a 15-inch gaming notebook with fantastic graphics capabilities, thanks to the NVIDIA® GeForce® GTX 1060 Max-Q graphics card, the latest 8th generation Intel® Core ™ i7 hexa-core processor and efficient DDR4 2666MHz RAM memory. The Inspiron G5 is designed specifically with the specific, demanding needs of gaming enthusiasts in mind.</p><p>The enormous performance, speed and dynamics of the eighth generation Intel Core i7 Coffee Lake processor is a guarantee of the highest performance in gaming and smooth operation with advanced applications. When more power is needed, Turbo Boost 2.0 technology intelligently speeds up the clock speed, unleashing the maximum potential of each CPU core. In addition, the unit flawlessly supports the highest definition video as well as spherical videos, while ensuring the security of transactions concluded on the network.</p><p>Play the latest, most demanding games with GeForce GTX 1060 Max-Q. Pull the sliders to the maximum and admire virtual worlds in 4K quality, enriched with HDR mode and DirectX 12 functions. All this with excellent smoothness of the image, without lag and clipping, thanks to the breakthrough architecture of Pascal GPU, packed with technologies for players.</p>",
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
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Dell").Id,
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
            allProducts.Add(productDellG5);
            productDellG5.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_dell_g5_1.png"), "image/png", pictureService.GetPictureSeName(productDellG5.Name))).Id,
                DisplayOrder = 1,
            });
            productDellG5.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_dell_g5_2.png"), "image/png", pictureService.GetPictureSeName(productDellG5.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDellG5);


            var productDellXPS = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Dell XPS",
                ShortDescription = "Dell laptop with a screen size of 13.4 inches and a resolution of 1920 x 1200 pixels. It is equipped with an Intel Core i7-1065G7 processor with a clock frequency of 1.3 - 3.9 GHz, DDR4 RAM memory with a size of 16 GB. 1000 GB SSD hard drive. Intel HD Graphics.",
                FullDescription = "<p>Dell laptop with a screen size of 13.4 inches and a resolution of 1920 x 1200 pixels. It is equipped with an Intel Core i7-1065G7 processor with a clock frequency of 1.3 - 3.9 GHz, DDR4 RAM memory with a size of 16 GB. 1000 GB SSD hard drive. Intel HD Graphics. Graphics card size shared with RAM, integrated. The installed operating system is Windows 10 Home.</p>",
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Dell").Id,
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
            allProducts.Add(productDellXPS);
            productDellXPS.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_dell_xps_1.png"), "image/png", pictureService.GetPictureSeName(productDellXPS.Name))).Id,
                DisplayOrder = 1,
            });
            productDellXPS.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_dell_xps_2.png"), "image/png", pictureService.GetPictureSeName(productDellXPS.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productDellXPS);

            #endregion

            #region Accessories


            var productLenovoYogaDuet = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo Yoga Duet",
                ShortDescription = "The adjustable stand allows for more convenient and effective work, sketching or taking notes in laptop mode or at a lower angle. The detachable Bluetooth® keyboard allows you to write and look at the screen with even more freedom.",
                FullDescription = "<p>Weighing just 1.16 kg, the Yoga Duet 7i is light and versatile enough to be used anywhere. The adjustable stand allows for more convenient and effective work, sketching or taking notes in laptop mode or at a lower angle. The detachable Bluetooth® keyboard allows you to write and look at the screen with even more freedom.</p><p>The Yoga Duet 7i is an advanced mobile device that is not only intuitive to use and easily personalized, but also uncompromisingly efficient. How is this possible? Powered by the 10th Gen Intel® Core ™ processor and artificial intelligence features that dynamically adjust power to optimize battery life. So it works for up to 10.8 hours on a single charge. It also has USB-C ports for faster charging.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Tablets").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLenovoYogaDuet);
            productLenovoYogaDuet.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_yoga_duet_1.png"), "image/png", pictureService.GetPictureSeName(productLenovoYogaDuet.Name))).Id,
                DisplayOrder = 1,
            });
            productLenovoYogaDuet.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_yoga_duet_2.png"), "image/png", pictureService.GetPictureSeName(productLenovoYogaDuet.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLenovoYogaDuet);


            var productLenovoSmartTab = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lenovo Smart Tab",
                ShortDescription = "The most versatile entertainment tablet now can do even more.",
                FullDescription = "<p>The most versatile entertainment tablet now can do even more. Lenovo Yoga Smart Tab with Google Assistant is a development of the groundbreaking Yoga Tab 3 with a stand for working in various modes. This tablet offers high-end entertainment features such as an IPS display with FHD resolution and two JBL® stereo speakers. Additionally, it also acts as a digital home control center. The built-in LTE modem ensures permanent access to the Internet.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Tablets").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLenovoSmartTab);
            productLenovoSmartTab.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_smart_tab_1.png"), "image/png", pictureService.GetPictureSeName(productLenovoSmartTab.Name))).Id,
                DisplayOrder = 1,
            });
            productLenovoSmartTab.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lenovo_smart_tab_2.png"), "image/png", pictureService.GetPictureSeName(productLenovoSmartTab.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLenovoSmartTab);


            var productAsusMixedReality = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Asus Mixed Reality",
                ShortDescription = "Explore exciting new virtual worlds with the ASUS Windows Mixed Reality Headset!",
                FullDescription = "<p>Explore exciting new virtual worlds with the ASUS Windows Mixed Reality Headset! It features a unique and beautiful 3D-pattern aesthetic and a comfy weight-balanced design with premium antibacterial cushioned materials, so it’s not only stylish but also supremely cool and comfortable for extended periods of exploring. Unlike other headsets, the ASUS Windows Mixed Reality Headset doesn’t need any external sensors, making initial set up super easy — you’ll be ready to play in 10 minutes or less*! It’s the revolutionary, easy-to-use and affordable way to explore your imagination.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 399M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Display").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productAsusMixedReality);
            productAsusMixedReality.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asus_mixed_reality_1.png"), "image/png", pictureService.GetPictureSeName(productAsusMixedReality.Name))).Id,
                DisplayOrder = 1,
            });
            productAsusMixedReality.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asus_mixed_reality_2.png"), "image/png", pictureService.GetPictureSeName(productAsusMixedReality.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAsusMixedReality);


            #endregion

            #region Display
            
            var productAcerProjector = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Acer Projector C250",
                ShortDescription = "Auto-Portrait Technology Now the projector image can rotate automatically, just like on your phone!",
                FullDescription = "<p>Auto-Portrait Technology Now the projector image can rotate automatically, just like on your phone! Equipped with Auto-Portrait technology, the C250i is the first model to be able to rotate the projected image in real time. There is no need to customize settings or content. Simply place the image vertically and the Acer C250i projector will remove intrusive black stripes by itself when you activate this mode. No stand, projecting from any angle The projector can be easily taken anywhere. The exclusive design does not take up much space and allows a 360-degree projection from any angle without using the stand. FHD Resolutions A beautiful 100-inch 1080p Full HD picture looks almost as realistic as the view from the window.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Display").Id,
                        DisplayOrder = 3,
                    }
                }
            };
            allProducts.Add(productAcerProjector);
            productAcerProjector.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_projector_1.png"), "image/png", pictureService.GetPictureSeName(productAcerProjector.Name))).Id,
                DisplayOrder = 1,
            });
            productAcerProjector.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_projector_2.png"), "image/png", pictureService.GetPictureSeName(productAcerProjector.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAcerProjector);


            var productAcerMonitor = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Acer Nitro XZ2",
                ShortDescription = "Eliminate choppy gameplay and distracting visual tear with AMD Radeon FreeSync™1. Savor the smooth, responsive visuals as the monitor’s refresh rate is synched to your computer’s framerate.",
                FullDescription = "<p>Eliminate choppy gameplay and distracting visual tear with AMD Radeon FreeSync™1. Savor the smooth, responsive visuals as the monitor’s refresh rate is synched to your computer’s framerate.</p><p>Enjoy seamless, lag-free gaming with a blazingly fast 165Hz2 refresh rate. To keep pace with the action, the rapid 4ms response time provides clearer, more immersive images.</p><p>Take your gameplay to the next level with improved color accuracy and contrast guaranteed by this VESA Certified DisplayHDR™ 400 monitor. This industry standard specifies HDR quality, including luminance, color gamut, bit depth, and rise time.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Display").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productAcerMonitor);
            productAcerMonitor.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_monitor_1.png"), "image/png", pictureService.GetPictureSeName(productAcerMonitor.Name))).Id,
                DisplayOrder = 1,
            });
            productAcerMonitor.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_acer_monitor_2.png"), "image/png", pictureService.GetPictureSeName(productAcerMonitor.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productAcerMonitor);

            #endregion

            #region Smartphone

            var productRedmiK30 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Redmi K30 Ultra",
                ShortDescription = "Redmi K30 Ultra will be equipped with top-class components. The smartphone is to debut on the market with a screen made in IPS LCD technology with a maximum refresh rate of 144 Hz, which guarantees smooth scrolling and perfect sharpness in movies and video games. It is worth noting that until now, displays of this type have been reserved exclusively for high-end gaming smartphones. The heart of the latest Redmi will be the MediaTek Dimensity 1000+ processor.",
                FullDescription = "<p>Redmi K30 Ultra equipped with top-class components. The smartphone debut on the market with a screen made in IPS LCD technology with a maximum refresh rate of 144 Hz, which guarantees smooth scrolling and perfect sharpness in movies and video games. It is worth noting that until now, displays of this type have been reserved exclusively for high-end gaming smartphones. The heart of the latest Redmi will be the MediaTek Dimensity 1000+ processor. It is the manufacturer's flagship chip offering eight cores, support for 5G connectivity and support for up to 16 GB RAM.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                Flag = "New",
                AllowCustomerReviews = true,
                Price = 199M,
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
                        DisplayOrder = 2,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartphones").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productRedmiK30);
            productRedmiK30.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Redmi_K30.png"), "image/png", pictureService.GetPictureSeName(productRedmiK30.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productRedmiK30);


            var productRedmiNote9 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Redmi Note 9",
                ShortDescription = "Redmi Note 9 is equipped with a high-performance octa-core processor with a maximum clock frequency of 2.0 GHz. The maximum GPU frequency of 1.0GHz ensures better performance and thus offers a smooth gaming experience.",
                FullDescription = "<p>Redmi Note 9 is equipped with a high-performance octa-core processor with a maximum clock frequency of 2.0 GHz. The maximum GPU frequency of 1.0GHz ensures better performance and thus offers a smooth gaming experience.</p><p>Thanks to the improved 5020mAh battery, you can enjoy long work on a single charge. In combination with the 18W fast charge, you will get excellent results and charge the battery in no time.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 249M,
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
                        DisplayOrder = 2,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartphones").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productRedmiNote9);
            productRedmiNote9.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Redmi_Note_9_1.png"), "image/png", pictureService.GetPictureSeName(productRedmiNote9.Name))).Id,
                DisplayOrder = 1,
            });
            productRedmiNote9.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Redmi_Note_9_2.png"), "image/png", pictureService.GetPictureSeName(productRedmiNote9.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productRedmiNote9);


            var productPocoF2Pro = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "POCO F2 Pro",
                ShortDescription = "The speed demon is now even better. Powered by an octa-core trifecta processor with a liquid cooling system, it provides a perfect working experience. Quad Camera with Pro Mode support.",
                FullDescription = "<p>The speed demon is now even better. Powered by an octa-core trifecta processor with a liquid cooling system, it provides a perfect working experience. Quad Camera with Pro Mode support.</p><p>The technology of execution in 7nm provides a 25% increase in performance, improving the smoothness of graphics rendering, while significantly reducing energy consumption. Kryo 585 ™ processor | Adreno 650 ™ graphics processor</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 299M,
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
                        DisplayOrder = 2,
                    }
                },
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartphones").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productPocoF2Pro);
            productPocoF2Pro.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_POCO_F2_Pro.png"), "image/png", pictureService.GetPictureSeName(productPocoF2Pro.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productPocoF2Pro);


            #endregion

            #region Others



            var productMiSmartBand = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Mi Smart Band 3i",
                ShortDescription = "<b>Mi Smart Band 3i:</b> Ignite your fitness journey with water resistant smart watch. Intuitive and Easy to View with large 0.78 inch OLED touch display. Get moving with the Fit App. Activity tracker and sleep tracker included.",
                FullDescription = "<p><b>Mi Smart Band 3i:</b> Ignite your fitness journey with water resistant smart watch. Intuitive and Easy to View with large 0.78 inch OLED touch display. Get moving with the Fit App. Activity tracker and sleep tracker included.</p>",
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
                        DisplayOrder = 2,
                    }
                },
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartwatches").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productMiSmartBand);
            productMiSmartBand.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Mi_Smart_Band_3i_1.png"), "image/png", pictureService.GetPictureSeName(productMiSmartBand.Name))).Id,
                DisplayOrder = 1,
            });
            productMiSmartBand.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Mi_Smart_Band_3i_2.png"), "image/png", pictureService.GetPictureSeName(productMiSmartBand.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productMiSmartBand);


            var productPs4 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Playstation 4 Slim",
                ShortDescription = "Meet the sleeker, smaller PS4 ™ that offers gamers an amazing gaming experience. The volume of the new PS4 is more than 30% smaller compared to previous console models, and its weight has been reduced by 25% and 16% respectively compared to the first (CUH-1000 series) and current (CUH-1200) versions of the PS4™.",
                FullDescription = "<p>Meet the sleeker, smaller PS4 ™ that offers gamers an amazing gaming experience. The volume of the new PS4 is more than 30% smaller compared to previous console models, and its weight has been reduced by 25% and 16% respectively compared to the first (CUH-1000 series) and current (CUH-1200) versions of the PS4 ™.</p>",
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
            allProducts.Add(productPs4);
            productPs4.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ps4_1.png"), "image/png", pictureService.GetPictureSeName(productPs4.Name))).Id,
                DisplayOrder = 1,
            });
            productPs4.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ps4_2.png"), "image/png", pictureService.GetPictureSeName(productPs4.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productPs4);


            var productMiBeard = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Xiaomi Mi Beard",
                ShortDescription = "Rounded blades for skin - friendly performance. Advanced self - sharpening. When in a rush, simply plug in the cord and get trimming. IPX7 fully-washable body The whole body is hydro - resistant and fully washable for your convenience.Comes with detachable head.",
                FullDescription = "<p>Rounded blades for skin - friendly performance. Advanced self - sharpening. With 2 combs that can go between 0.5mm and 20mm, this trimmer will perfectly sculpt your beard.Precision is at its finest with 6000 oscillations per min delivering accurate cuts and even shape.</p><p>IPX7 fully-washable body The whole body is hydro - resistant and fully washable for your convenience.Comes with detachable head.</p>",
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
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Xiaomi").Id,
                        DisplayOrder = 2,
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
            allProducts.Add(productMiBeard);
            productMiBeard.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_mi_beard_1.png"), "image/png", pictureService.GetPictureSeName(productMiBeard.Name))).Id,
                DisplayOrder = 1,
            });
            productMiBeard.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_mi_beard_2.png"), "image/png", pictureService.GetPictureSeName(productMiBeard.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productMiBeard);


            #endregion

            #region Shoes


            var productAdidasPredator = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "adidas Predator Instinct",
                ShortDescription = "Adidas Predator is the highest quality model of football boots. The special construction of the sole guarantees high flexibility and great adhesion, and the ingredients used to make the upper (synthetic material) ensure optimal weight of the shoe and adequate protection throughout the year.",
                FullDescription = "<p>Adidas Predator is the highest quality model of football boots. The special construction of the sole guarantees high flexibility and great adhesion, and the ingredients used to make the upper (synthetic material) ensure optimal weight of the shoe and adequate protection throughout the year. A feature of this model is also excellent vapor permeability - the moisture generated during the game is effectively expelled to the outside. The unique comfort and excellent foot support are due to the modern construction elements used by Adidas, which improve the player's features on the pitch. In Adidas footwear, the footballer turns into a ruthless predator. Thanks to the combination of modern technologies and great design, it is an excellent choice and fun to play.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 149M,
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
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Adidas").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productAdidasPredator);
            productAdidasPredator.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_predator_1.png"), "image/png", pictureService.GetPictureSeName(productAdidasPredator.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAdidasPredator);


            var productAdidasNitrocharge = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adidas Nitrocharge",
                ShortDescription = "The shoes are made of synthetic materials, they fit well to the foot thanks to their anatomical design and the insole made of Eva material. They are designed for playing and running on natural surfaces.",
                FullDescription = "<p>One of three colorways of the adidas Consortium Campus 80s Primeknit set to drop alongside each other. This pair comes in light maroon and running white. Featuring a maroon-based primeknit upper with white accents. A limited release, look out for these at select adidas Consortium accounts worldwide.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 99M,
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
                                Name = "Yellow",
                                IsPreSelected = true,
                                ColorSquaresRgb = "#FFFF00",
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
                                Name = "Orange",
                                ColorSquaresRgb = "#FF8000",
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
                },
                ProductManufacturers =
                {
                    new ProductManufacturer
                    {
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Adidas").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productAdidasNitrocharge);
            productAdidasNitrocharge.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas.png"), "image/png", pictureService.GetPictureSeName(productAdidasNitrocharge.Name))).Id,
                DisplayOrder = 1,
            });
            productAdidasNitrocharge.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_2.png"), "image/png", pictureService.GetPictureSeName(productAdidasNitrocharge.Name))).Id,
                DisplayOrder = 2,
            });
            productAdidasNitrocharge.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_3.png"), "image/png", pictureService.GetPictureSeName(productAdidasNitrocharge.Name))).Id,
                DisplayOrder = 3,
            });
            productAdidasNitrocharge.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidas_4.png"), "image/png", pictureService.GetPictureSeName(productAdidasNitrocharge.Name))).Id,
                DisplayOrder = 4,
            });


            await _productRepository.InsertAsync(productAdidasNitrocharge);

            var productAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Color").FirstOrDefault();

            productAdidasNitrocharge.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Blue").First().PictureId = productAdidasNitrocharge.ProductPictures.ElementAt(1).PictureId;
            productAdidasNitrocharge.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Yellow").First().PictureId = productAdidasNitrocharge.ProductPictures.ElementAt(2).PictureId;
            productAdidasNitrocharge.ProductAttributeMappings.Where(x => x.ProductAttributeId == productAttribute.Id).First().ProductAttributeValues.Where(x => x.Name == "Orange").First().PictureId = productAdidasNitrocharge.ProductPictures.ElementAt(3).PictureId;
            await _productRepository.UpdateAsync(productAdidasNitrocharge);


            var productAdidasTurfs = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adidas Turfs",
                ShortDescription = "Be unpredictable. Shoes designed for the youngest players starting their adventure with football. The sole is designed to provide perfect grip on artificial turf and hard or frozen surfaces.",
                FullDescription = "Be unpredictable. Shoes designed for the youngest players starting their adventure with football. The sole is designed to provide perfect grip on artificial turf and hard or frozen surfaces. The X-SKIN INSPIRATION upper made of synthetic material will give a feeling of lightness and support while guiding the ball thanks to the convex texture. The heel stiffening will provide stability, and the textile inner lining will provide comfort and adequate cushioning. A profiled insole reflecting the anatomical shape of the foot and symmetrical lacing will keep the foot in the right position. The sole will allow for dynamic feints and changes in the direction of the run on artificial turf as well as hard or frozen surfaces.",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 89M,
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
                        ManufacturerId = _manufacturerRepository.Table.Single(c => c.Name == "Adidas").Id,
                        DisplayOrder = 2,
                    }
                }
            };
            allProducts.Add(productAdidasTurfs);
            productAdidasTurfs.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidasturfs.png"), "image/png", pictureService.GetPictureSeName(productAdidasTurfs.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAdidasTurfs);


            #endregion

            #region Apparel

            //this one is a grouped product with two associated ones
            var productDerbyKit = new Product {
                ProductType = ProductType.GroupedProduct,
                VisibleIndividually = true,
                Name = "Derby County Kit",
                ShortDescription = "Show your pride and support and show off in The Rams Homemade Costumes.",
                FullDescription = "<p>Show your pride and support and show off in The Rams Homemade Costumes.</p><p>This is an official t-shirt made according to The Rams homewear specification. The whole is decorated with the club badge and the Umbro Double Diamond logo.</p>",
                ProductTemplateId = productTemplateGrouped.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 129.99M,
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
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyKit);
            productDerbyKit.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_awayshirt_1.png"), "image/png", pictureService.GetPictureSeName(productDerbyKit.Name))).Id,
                DisplayOrder = 1,
            });
            productDerbyKit.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_shirt_1.png"), "image/png", pictureService.GetPictureSeName(productDerbyKit.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productDerbyKit);
            var productDerbyKit_associated_1 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false, //hide this products
                ParentGroupedProductId = productDerbyKit.Id,
                Name = "Derby County Shirt - Away",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 129.99M,
                IsShipEnabled = true,
                Flag = "Grouped",
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
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyKit_associated_1);
            productDerbyKit_associated_1.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_awayshirt_1.png"), "image/png", pictureService.GetPictureSeName("Derby County Away Shirt"))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDerbyKit_associated_1);
            var productDerbyKit_associated_2 = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false,
                ParentGroupedProductId = productDerbyKit.Id,
                Name = "Derby County Shirt - Home",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 149.99M,
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
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyKit_associated_2);
            productDerbyKit_associated_2.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_shirt_1.png"), "image/png", pictureService.GetPictureSeName("Derby County Shirt - Home"))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDerbyKit_associated_2);

            var productNikeKids = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Kids Kit",
                ShortDescription = "Nike Dry-FIT football kit for kids. The set includes a T-shirt, shorts and football socks. Clothes made of high-quality synthetic materials that perfectly transport moisture and dry quickly.",
                FullDescription = "<p>Nike Dry-FIT football kit for kids. The set includes a T-shirt, shorts and football socks. Clothes made of high-quality synthetic materials that perfectly transport moisture and dry quickly. The set is perfect for training, matches, PE lessons, and the T-shirt and shorts are also perfect for everyday use. The shirt has ventilation panels under the arms and the back is made of airy mesh that removes excess heat. Children's shorts with an elastic, rubber belt will adapt to any figure. The set also includes football socks made of a pleasant-to-touch material that ensures high comfort of use.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 39M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productNikeKids);
            productNikeKids.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_kidskit.png"), "image/png", pictureService.GetPictureSeName(productNikeKids.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productNikeKids);

            var productPsgKit = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Paris Saint Germain Home Kit",
                ShortDescription = "",
                FullDescription = "<p>This oversized women t-Shirt needs minimum ironing. It is a great product at a great value!</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 99.99M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productPsgKit);
            productPsgKit.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_psg_1.png"), "image/png", pictureService.GetPictureSeName(productPsgKit.Name))).Id,
                DisplayOrder = 1,
            });
            productPsgKit.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_psg_2.png"), "image/png", pictureService.GetPictureSeName(productPsgKit.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productPsgKit);


            var productDerbyShirt = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Derby County Home Shirt",
                ShortDescription = "Show your pride and support and show off in The Rams Homemade Kits.",
                FullDescription = "<p>Show your pride and support and show off in The Rams Homemade Costumes.</p><p>This is an official t-shirt made according to The Rams homewear specification. The whole is decorated with the club badge and the Umbro Double Diamond logo.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 59M,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyShirt);
            productDerbyShirt.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_shirt_1.png"), "image/png", pictureService.GetPictureSeName(productDerbyShirt.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDerbyShirt);

            var productDerbyShorts = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Derby County Home Shorts",
                ShortDescription = "Show your pride and support and show off in The Rams Homemade Kits.",
                FullDescription = "<p>Show your pride and support and show off in The Rams Homemade Costumes.</p><p>This is an official t-shirt made according to The Rams homewear specification. The whole is decorated with the club badge and the Umbro Double Diamond logo.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 29M,
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
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyShorts);
            productDerbyShorts.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_shorts_1.png"), "image/png", pictureService.GetPictureSeName(productDerbyShorts.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDerbyShorts);

            var productDerbyKitHome = new Product {
                ProductType = ProductType.BundledProduct,
                VisibleIndividually = true,
                Name = "Derby County Home Shirt",
                ShortDescription = "Show your pride and support and show off in The Rams Homemade Kits.",
                FullDescription = "<p>Show your pride and support and show off in The Rams Homemade Costumes.</p><p>This is an official t-shirt made according to The Rams homewear specification. The whole is decorated with the club badge and the Umbro Double Diamond logo.</p>",
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
                ProductCategories =
                {
                    new ProductCategory
                    {
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productDerbyKitHome);
            productDerbyKitHome.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_derby_shirt_1.png"), "image/png", pictureService.GetPictureSeName(productDerbyKitHome.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productDerbyKitHome);

            var productChicagoBulls = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Chicago Bulls Jersey",
                ShortDescription = "Capture your team's distinct identity when you grab this custom Chicago Bulls jersey, It features classic trims and Chicago Bulls graphics along with Nike Dry and Dri-FIT technologies for added comfort.",
                FullDescription = "<p>Capture your team's distinct identity when you grab this custom Chicago Bulls jersey, It features classic trims and Chicago Bulls graphics along with Nike Dry and Dri-FIT technologies for added comfort. Before you watch the next game, grab this incredible jersey so everyone knows your fandom is on the cutting edge.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Apparel").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productChicagoBulls);

            productChicagoBulls.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_chicago_jersey_1.png"), "image/png", pictureService.GetPictureSeName(productChicagoBulls.Name))).Id,
                DisplayOrder = 1,
            });
            productChicagoBulls.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_chicago_jersey_2.png"), "image/png", pictureService.GetPictureSeName(productChicagoBulls.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productChicagoBulls);


            #endregion

            #region Smartwatches


            var productVivoactive = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Garmin VivoActive",
                ShortDescription = "The Vívoactive watch offers easy-to-repeat animated cardio, strength, yoga and Pilates exercises that you can view on your watch screen. Choose from preloaded animated workouts or download more from the Garmin Connect ™ community site.",
                FullDescription = "<p>You no longer need to search for videos and advice on the web to know what to do while training. The Vívoactive watch offers easy-to-repeat animated cardio, strength, yoga and Pilates exercises that you can view on your watch screen. Choose from preloaded animated workouts or download more from the Garmin Connect ™ community site.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartwatches").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productVivoactive);
            productVivoactive.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_vivoactive.png"), "image/png", pictureService.GetPictureSeName(productVivoactive.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productVivoactive);



            var productGarminFenix = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Garmin Fenix 2",
                ShortDescription = "Fēnix 2 combines the best features of our fitness watches with outdoor training watches. It is both a great navigation system and an ideal training partner in many different sports.",
                FullDescription = "<p>Fēnix 2 combines the best features of our fitness watches with outdoor training watches. It is both a great navigation system and an ideal training partner in many different sports. Whether you're running, swimming, skiing, cycling or hiking in the mountains, fēnix 2 lets you easily switch between groups of settings optimized for each activity.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartwatches").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productGarminFenix);
            productGarminFenix.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_garmin_fenix_1.png"), "image/png", pictureService.GetPictureSeName(productGarminFenix.Name))).Id,
                DisplayOrder = 1,
            });
            productGarminFenix.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_garmin_fenix_2.png"), "image/png", pictureService.GetPictureSeName(productGarminFenix.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productGarminFenix);



            var productForerunner = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Garmin Forerunner",
                ShortDescription = "This easy-to-use running watch is great for everyday runs, workouts and even pre-race training in a 10k run. Built-in GPS tracks your running route and provides accurate distance, pace and interval statistics.",
                FullDescription = "<p>This easy-to-use running watch is great for everyday runs, workouts and even pre-race training in a 10k run. Built-in GPS tracks your running route and provides accurate distance, pace and interval statistics. Its intuitive interface makes it easy to mark laps or pause the timer, even with sweaty hands. Forerunner 45 also monitors heart rate on the wrist during the day and while you sleep.</p>",
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Smartwatches").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productForerunner);
            productForerunner.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_forerunner.png"), "image/png", pictureService.GetPictureSeName(productForerunner.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productForerunner);

            #endregion

            #region Digital Downloads


            var downloadCyberpunk1 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_cyberpunk_1.zip"),
                Extension = ".zip",
                Filename = "Cyberpunk",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadCyberpunk1);
            var downloadCyberpunk2 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_cyberpunk_2.txt"),
                Extension = ".txt",
                Filename = "Cyberpunk",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadCyberpunk2);
            var productCyberpunk = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Cyberpunk 2077",
                ShortDescription = "Cyberpunk 2077 is an open-world adventure set in Night City, a megalopolis ruled by an obsessive quest for power, fame and body remake. Your name is V and you must acquire a one-of-a-kind implant - the key to immortality. Create your own playstyle and set out to conquer the mighty city of the future, whose history is shaped by your decisions.",
                FullDescription = "<p>Cyberpunk 2077 is an open-world adventure set in Night City, a megalopolis ruled by an obsessive quest for power, fame and body remake. Your name is V and you must acquire a one-of-a-kind implant - the key to immortality. Create your own playstyle and set out to conquer the mighty city of the future, whose history is shaped by your decisions.</p><p>Become a cyberpunk, a freelance armed to the teeth, and become the legend of the most dangerous city of the future. Create your character from scratch. Take on the role of the outlaw Punk, freedom-loving Nomad or ruthless Corp.</p><p>Get the most powerful implant in Night City and take on those who shake the whole city. Follow Rockerboy Johnny Silverhand (played by Keanu Reeves) and change a world ruled by large corporations forever. And all this is accompanied by music from bands and creators such as Run the Jewels, Refused, Grimes, A $ AP Rocky, Gazelle Twin, Ilan Rubin, Richard Devine, Nina Kraviz, Deadly Hunta, Rat Boy and Tina Guo.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 69M,
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
                DownloadId = downloadCyberpunk1.Id,
                DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
                UnlimitedDownloads = true,
                HasUserAgreement = false,
                HasSampleDownload = true,
                SampleDownloadId = downloadCyberpunk2.Id,
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
            allProducts.Add(productCyberpunk);
            productCyberpunk.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_cyberpunk_1.png"), "image/png", pictureService.GetPictureSeName(productCyberpunk.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productCyberpunk);



            var downloadGTA1 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_GTA_1.zip"),
                Extension = ".zip",
                Filename = "GTA",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadGTA1);
            var downloadGTA2 = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "text/plain",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_GTA_2.txt"),
                Extension = ".txt",
                Filename = "GTA",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadGTA2);
            var productGTA = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Grand Theft Auto 5",
                ShortDescription = "When a young street hustler, a retired bank robber and a terrifying psychopath land themselves in trouble, they must pull off a series of dangerous heists to survive in a city in which they can trust nobody, least of all each other.",
                FullDescription = "<p>When a young street hustler, a retired bank robber and a terrifying psychopath land themselves in trouble, they must pull off a series of dangerous heists to survive in a city in which they can trust nobody, least of all each other.</p><p>Launch business ventures from your Maze Bank West Executive Office, research powerful weapons technology from your underground Gunrunning Bunker and use your Counterfeit Cash Factory to start a lucrative counterfeiting operation.</p><p>Tear through the streets with a range of 10 high performance vehicles including a Supercar, Motorcycles, the weaponized Dune FAV, a Helicopter, a Rally Car and more. You’ll also get properties including a 10 car garage to store your growing fleet.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 49M,
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
                DownloadId = downloadGTA1.Id,
                DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
                UnlimitedDownloads = true,
                HasUserAgreement = false,
                HasSampleDownload = true,
                SampleDownloadId = downloadGTA2.Id,
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
            allProducts.Add(productGTA);

            productGTA.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_gta_1.png"), "image/png", pictureService.GetPictureSeName(productGTA.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productGTA);


            var downloadCod = new Download {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "application/x-zip-co",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_cod_1.zip"),
                Extension = ".zip",
                Filename = "Call of Duty",
                IsNew = true,
            };
            await downloadService.InsertDownload(downloadCod);
            var productCod = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Call of Duty: ColdWar",
                ShortDescription = "The iconic Black Ops series is back with Call of Duty®: Black Ops Cold War - the direct sequel to the original and fan-favorite Call of Duty®: Black Ops. Black Ops Cold War will drop fans into the depths of the Cold War’s volatile geopolitical battle of the early 1980s.",
                FullDescription = "<p>The iconic Black Ops series is back with Call of Duty®: Black Ops Cold War - the direct sequel to the original and fan-favorite Call of Duty®: Black Ops. Black Ops Cold War will drop fans into the depths of the Cold War’s volatile geopolitical battle of the early 1980s. Nothing is ever as it seems in a gripping single-player Campaign, where players will come face-to-face with historical figures and hard truths, as they battle around the globe through iconic locales like East Berlin, Vietnam, Turkey, Soviet KGB headquarters and more. As elite operatives, you will follow the trail of a shadowy figure named Perseus who is on a mission to destabilize the global balance of power and change the course of history. Descend into the dark center of this global conspiracy alongside iconic characters Woods, Mason and Hudson and a new cast of operatives attempting to stop a plot decades in the making. Beyond the Campaign, players will bring a Cold War arsenal of weapons and equipment into the next generation of Multiplayer and Zombies experiences.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 69M,
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
                DownloadId = downloadCod.Id,
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
            allProducts.Add(productCod);
            productCod.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_cod_1.png"), "image/png", pictureService.GetPictureSeName(productCod.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productCod);



            #endregion

            #region Lego

            var productLegoFalcon = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "LEGO Millennium Falcon",
                ShortDescription = "Inspire kids and adults with the LEGO® Star Wars ™ 75257 Millennium Falcon model. The brick-built version of the iconic Corellian freighter features a variety of details. This iconic set from the LEGO Star Wars series is a great addition to any fan's collection.",
                FullDescription = "<p>Inspire kids and adults with the LEGO® Star Wars ™ 75257 Millennium Falcon model. The brick-built version of the iconic Corellian freighter features a variety of details, including a rotating lower and upper gun turret, 2 spring-loaded shooters, a lowering ramp and an opening cockpit with space for 2 minifigures. The top panels fold out to reveal a detailed interior where children will love to reenact scenes from Star Wars: Skywalker. Rebirth ”featuring characters from the“ Star Wars ”universe - Finn, Chewbakka, Lando Calrissian, Boolio, C-3PO, R2-D2 and D-O. This iconic set from the LEGO Star Wars series is a great addition to any fan's collection.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 109M,
                OldPrice = 199M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Lego").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Lego").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLegoFalcon);
            productLegoFalcon.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lego_falcon_1.png"), "image/png", pictureService.GetPictureSeName(productLegoFalcon.Name))).Id,
                DisplayOrder = 1,
            });
            productLegoFalcon.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lego_falcon_2.png"), "image/png", pictureService.GetPictureSeName(productLegoFalcon.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLegoFalcon);



            var productLegoHogwarts = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lego Hogwarts",
                ShortDescription = "Taste real magic at the LEGO® Harry Potter™ Hogwarts™ Castle! Unforgettable building satisfaction with this highly detailed LEGO Harry Potter collectible set with over 6,000 pieces.",
                FullDescription = "<p>Taste real magic at the LEGO® Harry Potter™ Hogwarts™ Castle! Unforgettable building satisfaction with this highly detailed LEGO Harry Potter collectible set with over 6,000 pieces. It is packed with elements from the Harry Potter series - you will find towers, turrets, chambers, classrooms, creatures, Whomping Willow ™, Hagrid's hut and many other signature details. Plus, with 4 minifigures and 27 microfigures of students, teachers, statues and 5 Dementors, this advanced construction toy set is the perfect gift for any Harry Potter fan.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 99M,
                OldPrice = 149M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Lego").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Lego").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLegoHogwarts);
            productLegoHogwarts.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lego_hogwarts_1.png"), "image/png", pictureService.GetPictureSeName(productLegoHogwarts.Name))).Id,
                DisplayOrder = 1,
            });
            productLegoHogwarts.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_lego_hogwarts_2.png"), "image/png", pictureService.GetPictureSeName(productLegoHogwarts.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productLegoHogwarts);

            var productLegoCity = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Lego City Police Base",
                ShortDescription = "Everything is awesome about the LEGO® City Police Station (60246) playset. Little law enforcers and fans of the LEGO City TV series will love creating stories with a host of fun characters, including Duke DeTain, Chief Wheeler and Daisy Kaboom. ",
                FullDescription = "<p>Everything is awesome about the LEGO® City Police Station (60246) playset. Little law enforcers and fans of the LEGO City TV series will love creating stories with a host of fun characters, including Duke DeTain, Chief Wheeler and Daisy Kaboom. </p><p>This fantastic set includes a police station with a light-brick searchlight and a police car with sound-brick siren, plus a cool truck, motorcycle and surveillance drone. A building toy with a little extra With this toy playset you get a simple building guide and Instructions.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 59M,
                OldPrice = 99M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Lego").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Lego").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productLegoCity);
            productLegoCity.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LegoCity_1.png"), "image/png", pictureService.GetPictureSeName(productLegoCity.Name))).Id,
                DisplayOrder = 1,
            });
            productLegoCity.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LegoCity_2.png"), "image/png", pictureService.GetPictureSeName(productLegoCity.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productLegoCity);



            #endregion

            #region Balls

            var productAdidasBall = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adidas Ball",
                ShortDescription = "The Adidas Finale Top Capitano is a durable training ball with strong references to the UEFA Champions League. The ball is a replica of the model used in this year's Champions League group stage and is perfect for training and spontaneous games.",
                FullDescription = "<p>The Adidas Finale Top Capitano is a durable training ball with strong references to the UEFA Champions League. The ball is a replica of the model used in this year's Champions League group stage and is perfect for training and spontaneous games. The strong TPU coating has been machine-stitched to increase the durability of the ball. The ball's electrifying multicolored design shows the emotions of fans around the world as Europe's top teams compete for the highest honor.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 69M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Balls").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Balls").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productAdidasBall);
            productAdidasBall.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_adidasball.png"), "image/png", pictureService.GetPictureSeName(productAdidasBall.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productAdidasBall);


            var productMikasa = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Volleyball Ball",
                ShortDescription = "Made of high-quality synthetic leather (PU) A high-class ball based on the V200W match model. Solid and strong machine sewing.The 18 - panel, colorful design increases the visibility of the ball during the game.",
                FullDescription = "<p>Made of high-quality synthetic leather (PU) A high-class ball based on the V200W match model. Solid and strong machine sewing.The 18 - panel, colorful design increases the visibility of the ball during the game.</p><p> Weight: 260 - 280g </p><p> Circumference: 65 - 67cm </p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 29.99M,
                IsShipEnabled = true,
                IsFreeShipping = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Balls").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Balls").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productMikasa);
            productMikasa.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_mikasa.png"), "image/png", pictureService.GetPictureSeName(productMikasa.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productMikasa);


            var productSpalding = new Product {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Spalding Basketball Ball",
                ShortDescription = "The panels of leather, composite leather are attached to the rubber body by hand. This technique is used for indoor and indoor / outdoor balls. Balloon - the highest quality inner tube that maintains the pressure of the ball.",
                FullDescription = "<p>The panels of leather, composite leather are attached to the rubber body by hand. This technique is used for indoor and indoor / outdoor balls. Balloon - the highest quality inner tube that maintains the pressure of the ball. A specialized nylon braid - nylon lines give the ball integrity and durability. Smooth body and channels for a softer feel and strength - optimized deep channel design for better grip and control. Composite leather cover - provides a good grip, feel and aesthetic appearance of the ball, as well as the necessary strength and resistance to abrasion. Composite leather has an advanced moisture management system to improve dry and wet grip.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 49M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 2,
                TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Balls").Id,
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
                        CategoryId = _categoryRepository.Table.Single(c => c.Name == "Balls").Id,
                        DisplayOrder = 1,
                    }
                }
            };
            allProducts.Add(productSpalding);
            productSpalding.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_spalding.png"), "image/png", pictureService.GetPictureSeName(productSpalding.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(productSpalding);



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
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_25giftcart.png"), "image/png", pictureService.GetPictureSeName(product25GiftCard.Name))).Id,
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
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_50giftcart.png"), "image/png", pictureService.GetPictureSeName(product50GiftCard.Name))).Id,
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
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_100giftcart.png"), "image/png", pictureService.GetPictureSeName(product100GiftCard.Name))).Id,
                DisplayOrder = 1,
            });
            await _productRepository.InsertAsync(product100GiftCard);

            var productPlaystationBundlePack = new Product {
                ProductType = ProductType.BundledProduct,
                VisibleIndividually = true,
                Name = "Playstation 5 Kit",
                ShortDescription = "Meet the sleeker, smaller PS4 ™ that offers gamers an amazing gaming experience. The volume of the new PS4 is more than 30% smaller compared to previous console models, and its weight has been reduced by 25% and 16% respectively compared to the first (CUH-1000 series) and current (CUH-1200) versions of the PS4™.",
                FullDescription = "<p>Meet the sleeker, smaller PS4 ™ that offers gamers an amazing gaming experience. The volume of the new PS4 is more than 30% smaller compared to previous console models, and its weight has been reduced by 25% and 16% respectively compared to the first (CUH-1000 series) and current (CUH-1200) versions of the PS4 ™.</p>",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Price = 259M,
                IsShipEnabled = true,
                Flag = "Bundle Product",
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
            allProducts.Add(productPlaystationBundlePack);
            productPlaystationBundlePack.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps5_console_1.png"), "image/png", pictureService.GetPictureSeName(productPlaystationBundlePack.Name))).Id,
                DisplayOrder = 1,
            });
            productPlaystationBundlePack.ProductPictures.Add(new ProductPicture {
                PictureId = (await pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps5_console_2.png"), "image/png", pictureService.GetPictureSeName(productPlaystationBundlePack.Name))).Id,
                DisplayOrder = 2,
            });
            await _productRepository.InsertAsync(productPlaystationBundlePack);

            var productbundle1 = new BundleProduct {
                ProductBundleId = productPlaystationBundlePack.Id,
                ProductId = productPs4.Id,
                DisplayOrder = 1,
                Quantity = 1
            };
            var productbundle2 = new BundleProduct {
                ProductBundleId = productPlaystationBundlePack.Id,
                ProductId = productSonyPS5Pad.Id,
                DisplayOrder = 2,
                Quantity = 2
            };
            var productbundle3 = new BundleProduct {
                ProductBundleId = productPlaystationBundlePack.Id,
                ProductId = productPs5Camera.Id,
                DisplayOrder = 3,
                Quantity = 1
            };
            var productbundle4 = new BundleProduct {
                ProductBundleId = productPlaystationBundlePack.Id,
                ProductId = productCod.Id,
                DisplayOrder = 4,
                Quantity = 1
            };
            productPlaystationBundlePack.BundleProducts.Add(productbundle1);
            productPlaystationBundlePack.BundleProducts.Add(productbundle2);
            productPlaystationBundlePack.BundleProducts.Add(productbundle3);
            productPlaystationBundlePack.BundleProducts.Add(productbundle4);
            await _productRepository.UpdateAsync(productPlaystationBundlePack);

            #endregion

            //search engine names
            foreach (var product in allProducts)
            {
                product.SeName = SeoExtensions.GenerateSlug(product.Name, false, false);
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

            productMikasa.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productMikasa.Id,
                    ProductId2 = productSpalding.Id,
                });

            productMikasa.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productMikasa.Id,
                    ProductId2 = productAdidasBall.Id,
                });

            productSpalding.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSpalding.Id,
                    ProductId2 = productMikasa.Id,
                });

            productSpalding.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSpalding.Id,
                    ProductId2 = productAdidasBall.Id,
                });

            productAdidasBall.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasBall.Id,
                    ProductId2 = productMikasa.Id,
                });

            productAdidasBall.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasBall.Id,
                    ProductId2 = productSpalding.Id,
                });

            productGTA.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productGTA.Id,
                    ProductId2 = productCyberpunk.Id,
                });

            productGTA.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productGTA.Id,
                    ProductId2 = productCod.Id,
                });

            productCyberpunk.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCyberpunk.Id,
                    ProductId2 = productGTA.Id,
                });

            productCyberpunk.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productCyberpunk.Id,
                    ProductId2 = productCod.Id,
                });

            productLegoCity.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoCity.Id,
                    ProductId2 = productLegoHogwarts.Id,
                });

            productLegoCity.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoCity.Id,
                    ProductId2 = productLegoFalcon.Id,
                });

            productLegoHogwarts.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoHogwarts.Id,
                    ProductId2 = productLegoCity.Id,
                });

            productLegoHogwarts.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoHogwarts.Id,
                    ProductId2 = productLegoFalcon.Id,
                });

            productLegoFalcon.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoFalcon.Id,
                    ProductId2 = productLegoHogwarts.Id,
                });

            productLegoFalcon.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLegoFalcon.Id,
                    ProductId2 = productLegoCity.Id,
                });

            productLenovoLegionY740.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoLegionY740.Id,
                    ProductId2 = productDellXPS.Id,
                });

            productLenovoLegionY740.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoLegionY740.Id,
                    ProductId2 = productMiNotebook.Id,
                });

            productLenovoLegionY740.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoLegionY740.Id,
                    ProductId2 = productAsusMixedReality.Id,
                });

            productLenovoLegionY740.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoLegionY740.Id,
                    ProductId2 = productAcerNitro.Id,
                });

            productDellXPS.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellXPS.Id,
                    ProductId2 = productLenovoLegionY740.Id,
                });

            productDellXPS.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellXPS.Id,
                    ProductId2 = productMiNotebook.Id,
                });

            productDellXPS.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellXPS.Id,
                    ProductId2 = productAcerMonitor.Id,
                });

            productDellXPS.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productDellXPS.Id,
                     ProductId2 = productDellG5.Id,
                 });

            productMiNotebook.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productMiNotebook.Id,
                    ProductId2 = productDellXPS.Id,
                });

            productMiNotebook.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productMiNotebook.Id,
                    ProductId2 = productAcerMonitor.Id,
                });

            productMiNotebook.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productMiNotebook.Id,
                    ProductId2 = productLenovoLegionY740.Id,
                });

            productMiNotebook.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productMiNotebook.Id,
                     ProductId2 = productAcerNitro.Id,
                 });

            productAcerNitro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerNitro.Id,
                    ProductId2 = productDellXPS.Id,
                });

            productAcerNitro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerNitro.Id,
                    ProductId2 = productAcerProjector.Id,
                });

            productAcerNitro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerNitro.Id,
                    ProductId2 = productLenovoLegionY740.Id,
                });

            productAcerNitro.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productAcerNitro.Id,
                     ProductId2 = productDellG5.Id,
                 });

            productDellG5.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellG5.Id,
                    ProductId2 = productLenovoLegionY740.Id,
                });

            productDellG5.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellG5.Id,
                    ProductId2 = productMiNotebook.Id,
                });

            productDellG5.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellG5.Id,
                    ProductId2 = productAcerNitro.Id,
                });

            productDellG5.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDellG5.Id,
                    ProductId2 = productAcerMonitor.Id,
                });
            productPs5Camera.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPs5Camera.Id,
                    ProductId2 = productPlaystationBundlePack.Id,
                });
            productPs5Camera.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPs5Camera.Id,
                    ProductId2 = productPs4.Id,
                });

            productPs5Camera.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPs5Camera.Id,
                    ProductId2 = productDellG5.Id,
                });
            productPs5Camera.RelatedProducts.Add(
                 new RelatedProduct {
                     ProductId1 = productPs5Camera.Id,
                     ProductId2 = productAcerNitro.Id,
                 });
            productAcerProjector.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerProjector.Id,
                    ProductId2 = productRedmiNote9.Id,
                });

            productAcerProjector.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerProjector.Id,
                    ProductId2 = productDerbyKit.Id,
                });

            productAcerProjector.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerProjector.Id,
                    ProductId2 = productAcerMonitor.Id,
                });

            productAcerProjector.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAcerProjector.Id,
                    ProductId2 = productPocoF2Pro.Id,
                });
            productRedmiK30.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiK30.Id,
                    ProductId2 = productRedmiNote9.Id,
                });

            productRedmiK30.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiK30.Id,
                    ProductId2 = productPocoF2Pro.Id,
                });
            productRedmiK30.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiK30.Id,
                    ProductId2 = productMiSmartBand.Id,
                });

            productRedmiK30.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiK30.Id,
                    ProductId2 = productMiBeard.Id,
                });

            productRedmiNote9.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiNote9.Id,
                    ProductId2 = productRedmiK30.Id,
                });
            productRedmiNote9.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiNote9.Id,
                    ProductId2 = productPocoF2Pro.Id,
                });

            productRedmiNote9.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiNote9.Id,
                    ProductId2 = productMiSmartBand.Id,
                });
            productRedmiNote9.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productRedmiNote9.Id,
                    ProductId2 = productMiBeard.Id,
                });
            productPocoF2Pro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPocoF2Pro.Id,
                    ProductId2 = productRedmiK30.Id,
                });
            productPocoF2Pro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPocoF2Pro.Id,
                    ProductId2 = productRedmiNote9.Id,
                });

            productPocoF2Pro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPocoF2Pro.Id,
                    ProductId2 = productMiSmartBand.Id,
                });
            productPocoF2Pro.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productPocoF2Pro.Id,
                    ProductId2 = productMiBeard.Id,
                });

            productAdidasNitrocharge.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasNitrocharge.Id,
                    ProductId2 = productChicagoBulls.Id,
                });

            productAdidasNitrocharge.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasNitrocharge.Id,
                    ProductId2 = productAdidasPredator.Id,
                });

            productAdidasNitrocharge.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasNitrocharge.Id,
                    ProductId2 = productAdidasTurfs.Id,
                });
            productAdidasNitrocharge.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productAdidasNitrocharge.Id,
                    ProductId2 = productNikeKids.Id,
                });
            productChicagoBulls.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productChicagoBulls.Id,
                    ProductId2 = productAdidasNitrocharge.Id,
                });
            productChicagoBulls.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productChicagoBulls.Id,
                    ProductId2 = productAdidasPredator.Id,
                });

            productChicagoBulls.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productChicagoBulls.Id,
                    ProductId2 = productAdidasTurfs.Id,
                });
            productChicagoBulls.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productChicagoBulls.Id,
                    ProductId2 = productNikeKids.Id,
                });

            productDerbyShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDerbyShirt.Id,
                    ProductId2 = productChicagoBulls.Id,
                });

            productDerbyShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDerbyShirt.Id,
                    ProductId2 = productNikeKids.Id,
                });
            productDerbyShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDerbyShirt.Id,
                    ProductId2 = productPsgKit.Id,
                });
            productDerbyShirt.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productDerbyShirt.Id,
                    ProductId2 = productVivoactive.Id,
                });
            productSonyPS5Pad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSonyPS5Pad.Id,
                    ProductId2 = productBuildComputer.Id,
                });
            productSonyPS5Pad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSonyPS5Pad.Id,
                    ProductId2 = productLenovoIdeaPadDual.Id,
                });
            productSonyPS5Pad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSonyPS5Pad.Id,
                    ProductId2 = productDellXPS.Id,
                });
            productSonyPS5Pad.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productSonyPS5Pad.Id,
                    ProductId2 = productMiNotebook.Id,
                });

            productLenovoIdeaPadDual.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaPadDual.Id,
                    ProductId2 = productBuildComputer.Id,
                });

            productLenovoIdeaPadDual.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaPadDual.Id,
                    ProductId2 = productSonyPS5Pad.Id,
                });

            productLenovoIdeaPadDual.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaPadDual.Id,
                    ProductId2 = productDellXPS.Id,
                });

            productLenovoIdeaPadDual.RelatedProducts.Add(
                new RelatedProduct {
                    ProductId1 = productLenovoIdeaPadDual.Id,
                    ProductId2 = productMiNotebook.Id,
                });


            #endregion

            #region Product Tags

            //product tags
            await AddProductTag(product25GiftCard, "nice");
            await AddProductTag(product25GiftCard, "gift");
            await AddProductTag(productNikeKids, "cool");
            await AddProductTag(productNikeKids, "apparel");
            await AddProductTag(productNikeKids, "shirt");
            await AddProductTag(productMiSmartBand, "computer");
            await AddProductTag(productMiSmartBand, "cool");
            await AddProductTag(productAdidasPredator, "cool");
            await AddProductTag(productAdidasPredator, "shoes");
            await AddProductTag(productAdidasPredator, "apparel");
            await AddProductTag(productLenovoYogaDuet, "tablet");
            await AddProductTag(productLenovoYogaDuet, "awesome");
            await AddProductTag(productPs4, "computer");
            await AddProductTag(productPs4, "cool");
            await AddProductTag(productPsgKit, "cool");
            await AddProductTag(productPsgKit, "apparel");
            await AddProductTag(productPsgKit, "shirt");
            await AddProductTag(productMiNotebook, "compact");
            await AddProductTag(productMiNotebook, "awesome");
            await AddProductTag(productMiNotebook, "computer");
            await AddProductTag(productLenovoLegionY740, "compact");
            await AddProductTag(productLenovoLegionY740, "awesome");
            await AddProductTag(productLenovoLegionY740, "computer");
            await AddProductTag(productLegoFalcon, "awesome");
            await AddProductTag(productLegoFalcon, "lego");
            await AddProductTag(productLegoFalcon, "nice");
            await AddProductTag(productRedmiK30, "cell");
            await AddProductTag(productRedmiK30, "compact");
            await AddProductTag(productRedmiK30, "awesome");
            await AddProductTag(productBuildComputer, "awesome");
            await AddProductTag(productBuildComputer, "computer");
            await AddProductTag(productDerbyKit, "cool");
            await AddProductTag(productDerbyKit, "football kit");
            await AddProductTag(productAcerProjector, "projector");
            await AddProductTag(productAcerProjector, "cool");
            await AddProductTag(productSonyPS5Pad, "cool");
            await AddProductTag(productSonyPS5Pad, "computer");
            await AddProductTag(productLenovoSmartTab, "awesome");
            await AddProductTag(productLenovoSmartTab, "tablet");
            await AddProductTag(productDerbyShirt, "cool");
            await AddProductTag(productDerbyShirt, "shirt");
            await AddProductTag(productDerbyShirt, "apparel");
            await AddProductTag(productAdidasBall, "Balls");
            await AddProductTag(productAdidasBall, "awesome");
            await AddProductTag(productMikasa, "awesome");
            await AddProductTag(productMikasa, "Balls");
            await AddProductTag(productLegoHogwarts, "lego");
            await AddProductTag(productAdidasNitrocharge, "cool");
            await AddProductTag(productAdidasNitrocharge, "shoes");
            await AddProductTag(productAdidasNitrocharge, "apparel");
            await AddProductTag(productLenovoIdeaPadDual, "awesome");
            await AddProductTag(productLenovoIdeaPadDual, "tablet");
            await AddProductTag(productPs5Camera, "nice");
            await AddProductTag(productPs5Camera, "computer");
            await AddProductTag(productPs5Camera, "compact");
            await AddProductTag(productAcerNitro, "nice");
            await AddProductTag(productAcerNitro, "computer");
            await AddProductTag(productDellG5, "computer");
            await AddProductTag(productDellG5, "cool");
            await AddProductTag(productDellG5, "compact");
            await AddProductTag(productVivoactive, "apparel");
            await AddProductTag(productVivoactive, "cool");
            await AddProductTag(productChicagoBulls, "cool");
            await AddProductTag(productChicagoBulls, "sport");
            await AddProductTag(productChicagoBulls, "apparel");
            await AddProductTag(productAsusMixedReality, "game");
            await AddProductTag(productAsusMixedReality, "computer");
            await AddProductTag(productAsusMixedReality, "cool");
            await AddProductTag(productCyberpunk, "awesome");
            await AddProductTag(productCyberpunk, "digital");
            await AddProductTag(productForerunner, "apparel");
            await AddProductTag(productForerunner, "cool");
            await AddProductTag(productRedmiNote9, "awesome");
            await AddProductTag(productRedmiNote9, "compact");
            await AddProductTag(productRedmiNote9, "cell");
            await AddProductTag(productGTA, "digital");
            await AddProductTag(productGTA, "game");
            await AddProductTag(productPocoF2Pro, "awesome");
            await AddProductTag(productPocoF2Pro, "cool");
            await AddProductTag(productPocoF2Pro, "camera");
            await AddProductTag(productCod, "digital");
            await AddProductTag(productCod, "awesome");
            await AddProductTag(productLegoCity, "lego");
            await AddProductTag(productDellXPS, "awesome");
            await AddProductTag(productDellXPS, "computer");
            await AddProductTag(productDellXPS, "compact");
            await AddProductTag(productAdidasTurfs, "jeans");
            await AddProductTag(productAdidasTurfs, "cool");
            await AddProductTag(productAdidasTurfs, "apparel");
            await AddProductTag(productSpalding, "Balls");
            await AddProductTag(productSpalding, "awesome");


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
                var seName = SeoExtensions.GenerateSlug(blogPost.Title, false, false);
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
                newsItem.SeName = SeoExtensions.GenerateSlug(newsItem.Title, false, false);
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
                                                  SystemKeyword = "AddNewSalesEmployee",
                                                  Enabled = true,
                                                  Name = "Add a sales employee"
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
                                                  SystemKeyword = "AddRewardPoints",
                                                  Enabled = true,
                                                  Name = "Assign new reward points"
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
                                                  SystemKeyword = "DeleteSalesEmployee",
                                                  Enabled = true,
                                                  Name = "Delete a sales employee"
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
                                                  SystemKeyword = "EditSalesEmployee",
                                                  Enabled = true,
                                                  Name = "Edit a sales employee"
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
                                                  SystemKeyword = "CustomerAdmin.UpdateCartCustomer",
                                                  Enabled = true,
                                                  Name = "Update shopping cart"
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
                new ScheduleTask
                {
                    ScheduleTaskName = "Cancel unpaid and pending orders",
                    Type = "Grand.Services.Tasks.CancelOrderScheduledTask, Grand.Services",
                    Enabled = false,
                    StopOnError = false,
                    TimeInterval = 1440
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

            var warehouse2address = new Address {
                Address1 = "300 South Spring Stree",
                City = "Los Angeles",
                StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "California").Id,
                CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA").Id,
                ZipPostalCode = "90013",
                CreatedOnUtc = DateTime.UtcNow,
            };

            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    Name = "Warehouse 1 (New York)",
                    Address = warehouse1address,
                    DisplayOrder = 0,
                },
                new Warehouse
                {
                    Name = "Warehouse 2 (Los Angeles)",
                    Address = warehouse2address,
                    DisplayOrder = 1,
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
                var seName = SeoExtensions.GenerateSlug(vendor.Name, false, false);
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
                    SeName = SeoExtensions.GenerateSlug(tag, false, false),
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

            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.DisplayOrder")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_DisplayOrder_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.DisplayOrderCategory).Ascending("ProductCategories.CategoryId")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_OrderCategory_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name).Ascending("ProductCategories.CategoryId")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Name_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Price).Ascending("ProductCategories.CategoryId")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Price_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Sold).Ascending("ProductCategories.CategoryId")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Sold_1", Unique = false }));
            await _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending("ProductCategories.CategoryId").Ascending("ProductCategories.IsFeaturedProduct")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_IsFeaturedProduct_1", Unique = false }));

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
                var q = typeFinder.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Grand.Domain");
                foreach (var item in q.GetTypes())
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
            string defaultUserPassword, string collation, bool installSampleData = true, string companyName = "", string companyAddress = "", string companyPhoneNumber = "", string companyEmail = "")
        {

            defaultUserEmail = defaultUserEmail.ToLower();
            await CreateTables(collation);
            await CreateIndexes();
            await InstallVersion();
            await InstallStores(companyName, companyAddress, companyPhoneNumber, companyEmail);
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
