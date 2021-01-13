using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Services.Affiliates;
using Grand.Services.Authentication;
using Grand.Services.Authentication.External;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Cms;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Courses;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Documents;
using Grand.Services.ExportImport;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Installation;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.MachineNameProvider;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.News;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Polls;
using Grand.Services.PushNotifications;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tasks;
using Grand.Services.Tax;
using Grand.Services.Topics;
using Grand.Services.Vendors;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grand.Services.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="serviceCollection">Service Collection</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            RegisterMachineNameProvider(serviceCollection, config);

            RegisterConfigurationService(serviceCollection);

            RegisterAffiliateService(serviceCollection);

            RegisterAuthenticationService(serviceCollection);

            RegisterBlogService(serviceCollection);

            RegisterCatalogService(serviceCollection);

            RegisterCmsService(serviceCollection);

            RegisterCommonService(serviceCollection);

            RegisterCoursesService(serviceCollection);

            RegisterCustomerService(serviceCollection);

            RegisterDirectoryService(serviceCollection);

            RegisterDiscountsService(serviceCollection);

            RegisterDocumentsService(serviceCollection);

            RegisterExportImportService(serviceCollection);

            RegisterForumService(serviceCollection);

            RegisterInstallService(serviceCollection);

            RegisterKnowledgebaseService(serviceCollection);

            RegisterLocalizationService(serviceCollection);

            RegisterLoggingService(serviceCollection);

            RegisterMediaService(serviceCollection, config);

            RegisterMessageService(serviceCollection);

            RegisterNewsService(serviceCollection);

            RegisterOrdersService(serviceCollection);

            RegisterPaymentsService(serviceCollection);

            RegisterPollsService(serviceCollection);

            RegisterPushService(serviceCollection);

            RegisterSecurityService(serviceCollection);

            RegisterSeoService(serviceCollection);

            RegisterShippingService(serviceCollection);

            RegisterStoresService(serviceCollection);

            RegisterTaxService(serviceCollection);

            RegisterTopicsService(serviceCollection);

            RegisterTask(serviceCollection);

        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order {
            get { return 1; }
        }

        private void RegisterMachineNameProvider(IServiceCollection serviceCollection, GrandConfig config)
        {
            if (config.RunOnAzureWebApps)
            {
                serviceCollection.AddSingleton<IMachineNameProvider, AzureWebAppsMachineNameProvider>();
            }
            else
            {
                serviceCollection.AddSingleton<IMachineNameProvider, DefaultMachineNameProvider>();
            }
        }

        private void RegisterAffiliateService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAffiliateService, AffiliateService>();
        }

        private void RegisterAuthenticationService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGrandAuthenticationService, CookieAuthenticationService>();
            serviceCollection.AddScoped<IApiAuthenticationService, ApiAuthenticationService>();
            serviceCollection.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            serviceCollection.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
        }
        private void RegisterCommonService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAddressAttributeParser, AddressAttributeParser>();
            serviceCollection.AddScoped<IAddressAttributeService, AddressAttributeService>();
            serviceCollection.AddScoped<IAddressService, AddressService>();
            serviceCollection.AddScoped<IGenericAttributeService, GenericAttributeService>();
            serviceCollection.AddScoped<IHistoryService, HistoryService>();
            serviceCollection.AddScoped<IPdfService, WkPdfService>();
            serviceCollection.AddScoped<IViewRenderService, ViewRenderService>();
            serviceCollection.AddScoped<ISearchTermService, SearchTermService>();
            serviceCollection.AddScoped<IDateTimeHelper, DateTimeHelper>();
            serviceCollection.AddScoped<ICookiePreference, CookiePreference>();
        }
        private void RegisterCatalogService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBackInStockSubscriptionService, BackInStockSubscriptionService>();
            serviceCollection.AddScoped<ICategoryService, CategoryService>();
            serviceCollection.AddScoped<ICompareProductsService, CompareProductsService>();
            serviceCollection.AddScoped<IRecentlyViewedProductsService, RecentlyViewedProductsService>();
            serviceCollection.AddScoped<IManufacturerService, ManufacturerService>();
            serviceCollection.AddScoped<IPriceFormatter, PriceFormatter>();
            serviceCollection.AddScoped<IProductAttributeFormatter, ProductAttributeFormatter>();
            serviceCollection.AddScoped<IProductAttributeParser, ProductAttributeParser>();
            serviceCollection.AddScoped<IProductAttributeService, ProductAttributeService>();
            serviceCollection.AddScoped<IProductService, ProductService>();
            serviceCollection.AddScoped<IProductReviewService, ProductReviewService>();
            serviceCollection.AddScoped<ICopyProductService, CopyProductService>();
            serviceCollection.AddScoped<IProductReservationService, ProductReservationService>();
            serviceCollection.AddScoped<IAuctionService, AuctionService>();
            serviceCollection.AddScoped<IProductCourseService, ProductCourseService>();
            serviceCollection.AddScoped<ISpecificationAttributeService, SpecificationAttributeService>();
            serviceCollection.AddScoped<IProductTemplateService, ProductTemplateService>();
            serviceCollection.AddScoped<ICategoryTemplateService, CategoryTemplateService>();
            serviceCollection.AddScoped<IManufacturerTemplateService, ManufacturerTemplateService>();
            serviceCollection.AddScoped<IProductTagService, ProductTagService>();
            serviceCollection.AddScoped<IInventoryManageService, InventoryManageService>();
            serviceCollection.AddScoped<IPriceCalculationService, PriceCalculationService>();
            serviceCollection.AddScoped<IOrderTagService, OrderTagService>();

        }

        private void RegisterCoursesService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICourseActionService, CourseActionService>();
            serviceCollection.AddScoped<ICourseLessonService, CourseLessonService>();
            serviceCollection.AddScoped<ICourseLevelService, CourseLevelService>();
            serviceCollection.AddScoped<ICourseService, CourseService>();
            serviceCollection.AddScoped<ICourseSubjectService, CourseSubjectService>();

        }

        private void RegisterCustomerService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IVendorService, VendorService>();
            serviceCollection.AddScoped<ICustomerAttributeParser, CustomerAttributeParser>();
            serviceCollection.AddScoped<ICustomerAttributeService, CustomerAttributeService>();
            serviceCollection.AddScoped<ICustomerService, CustomerService>();
            serviceCollection.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
            serviceCollection.AddScoped<ICustomerReportService, CustomerReportService>();
            serviceCollection.AddScoped<ICustomerTagService, CustomerTagService>();
            serviceCollection.AddScoped<ICustomerActionService, CustomerActionService>();
            serviceCollection.AddScoped<ICustomerActionEventService, CustomerActionEventService>();
            serviceCollection.AddScoped<ICustomerReminderService, CustomerReminderService>();
            serviceCollection.AddScoped<ICustomerProductService, CustomerProductService>();
            serviceCollection.AddScoped<ICustomerCoordinatesService, CustomerCoordinatesService>();
            serviceCollection.AddScoped<ISalesEmployeeService, SalesEmployeeService>();
            serviceCollection.AddScoped<IUserApiService, UserApiService>();

        }

        private void RegisterDirectoryService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGeoLookupService, GeoLookupService>();
            serviceCollection.AddScoped<ICountryService, CountryService>();

            serviceCollection.AddScoped<ICurrencyService, CurrencyService>();
            serviceCollection.AddScoped<IMeasureService, MeasureService>();
            serviceCollection.AddScoped<IStateProvinceService, StateProvinceService>();

        }

        private void RegisterDocumentsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDocumentTypeService, DocumentTypeService>();
            serviceCollection.AddScoped<IDocumentService, DocumentService>();

        }

        private void RegisterDiscountsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDiscountService, DiscountService>();
        }

        private void RegisterBlogService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBlogService, BlogService>();
        }

        private void RegisterCmsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IWidgetService, WidgetService>();

        }

        private void RegisterConfigurationService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISettingService, SettingService>();
            serviceCollection.AddScoped<IGoogleAnalyticsService, GoogleAnalyticsService>();
        }

        private void RegisterExportImportService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IExportManager, ExportManager>();
            serviceCollection.AddScoped<IImportManager, ImportManager>();
        }

        private void RegisterForumService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IForumService, ForumService>();
        }

        private void RegisterInstallService(IServiceCollection serviceCollection)
        {
            var databaseInstalled = DataSettingsHelper.DatabaseIsInstalled();
            if (!databaseInstalled)
            {
                //installation service
                serviceCollection.AddScoped<IInstallationLocalizationService, InstallationLocalizationService>();
                serviceCollection.AddScoped<IInstallationService, CodeFirstInstallationService>();
            }
            else
            {
                serviceCollection.AddScoped<IUpgradeService, UpgradeService>();
            }
        }

        private void RegisterKnowledgebaseService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IKnowledgebaseService, KnowledgebaseService>();
        }

        private void RegisterLocalizationService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ILocalizationService, LocalizationService>();
            serviceCollection.AddScoped<ILanguageService, LanguageService>();

        }

        private void RegisterLoggingService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICustomerActivityService, CustomerActivityService>();
            serviceCollection.AddScoped<IActivityKeywordsProvider, ActivityKeywordsProvider>();
            serviceCollection.AddScoped<ILogger, DefaultLogger>();

        }

        private void RegisterMessageService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBannerService, BannerService>();
            serviceCollection.AddScoped<IPopupService, PopupService>();
            serviceCollection.AddScoped<IInteractiveFormService, InteractiveFormService>();
            serviceCollection.AddScoped<INewsLetterSubscriptionService, NewsLetterSubscriptionService>();
            serviceCollection.AddScoped<INewsletterCategoryService, NewsletterCategoryService>();
            serviceCollection.AddScoped<ICampaignService, CampaignService>();
            serviceCollection.AddScoped<IMessageTemplateService, MessageTemplateService>();
            serviceCollection.AddScoped<IQueuedEmailService, QueuedEmailService>();
            serviceCollection.AddScoped<IEmailAccountService, EmailAccountService>();
            serviceCollection.AddScoped<IWorkflowMessageService, WorkflowMessageService>();
            serviceCollection.AddScoped<IMessageTokenProvider, MessageTokenProvider>();
            serviceCollection.AddScoped<ITokenizer, Tokenizer>();
            serviceCollection.AddScoped<IEmailSender, EmailSender>();

            serviceCollection.AddScoped<IContactAttributeParser, ContactAttributeParser>();
            serviceCollection.AddScoped<IContactAttributeService, ContactAttributeService>();

            serviceCollection.AddScoped<IContactUsService, ContactUsService>();
        }
        private void RegisterNewsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<INewsService, NewsService>();
        }

        private void RegisterOrdersService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IRewardPointsService, RewardPointsService>();
            serviceCollection.AddScoped<IGiftCardService, GiftCardService>();
            serviceCollection.AddScoped<IOrderService, OrderService>();
            serviceCollection.AddScoped<IOrderReportService, OrderReportService>();
            serviceCollection.AddScoped<IOrderProcessingService, OrderProcessingService>();
            serviceCollection.AddScoped<IOrderConfirmationService, OrderConfirmationService>();
            serviceCollection.AddScoped<IOrderRecurringPayment, OrderRecurringPayment>();
            serviceCollection.AddScoped<IOrderTotalCalculationService, OrderTotalCalculationService>();
            serviceCollection.AddScoped<IReturnRequestService, ReturnRequestService>();
            serviceCollection.AddScoped<IRewardPointsService, RewardPointsService>();
            serviceCollection.AddScoped<IShoppingCartService, ShoppingCartService>();
            serviceCollection.AddScoped<ICheckoutAttributeFormatter, CheckoutAttributeFormatter>();
            serviceCollection.AddScoped<ICheckoutAttributeParser, CheckoutAttributeParser>();
            serviceCollection.AddScoped<ICheckoutAttributeService, CheckoutAttributeService>();

        }
        private void RegisterPaymentsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPaymentService, PaymentService>();
        }
        private void RegisterPollsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPollService, PollService>();
        }

        private void RegisterPushService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPushNotificationsService, PushNotificationsService>();
        }

        private void RegisterSecurityService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            serviceCollection.AddScoped<IAclService, AclService>();
            serviceCollection.AddScoped<IEncryptionService, EncryptionService>();
        }

        private void RegisterSeoService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISitemapGenerator, SitemapGenerator>();
            serviceCollection.AddScoped<IUrlRecordService, UrlRecordService>();

        }

        private void RegisterShippingService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IShipmentService, ShipmentService>();
            serviceCollection.AddScoped<IShippingService, ShippingService>();
            serviceCollection.AddScoped<IPickupPointService, PickupPointService>();
            serviceCollection.AddScoped<IDeliveryDateService, DeliveryDateService>();
            serviceCollection.AddScoped<IWarehouseService, WarehouseService>();
            serviceCollection.AddScoped<IShippingMethodService, ShippingMethodService>();
        }
        private void RegisterStoresService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IStoreService, StoreService>();
            serviceCollection.AddScoped<IStoreMappingService, StoreMappingService>();
        }
        private void RegisterTaxService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITaxService, TaxService>();
            serviceCollection.AddScoped<IVatService, VatService>();
            serviceCollection.AddScoped<ITaxCategoryService, TaxCategoryService>();
        }

        private void RegisterTopicsService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITopicTemplateService, TopicTemplateService>();
            serviceCollection.AddScoped<ITopicService, TopicService>();

        }

        private void RegisterMediaService(IServiceCollection serviceCollection, GrandConfig config)
        {


            serviceCollection.AddScoped<IMimeMappingService, MimeMappingService>(x =>
            {
                var provider = new FileExtensionContentTypeProvider();
                return new MimeMappingService(provider);

            });

            //picture service
            var useAzureBlobStorage = !String.IsNullOrEmpty(config.AzureBlobStorageConnectionString);
            var useAmazonBlobStorage = (!String.IsNullOrEmpty(config.AmazonAwsAccessKeyId) && !String.IsNullOrEmpty(config.AmazonAwsSecretAccessKey) && !String.IsNullOrEmpty(config.AmazonBucketName) && !String.IsNullOrEmpty(config.AmazonRegion));

            if (useAzureBlobStorage)
            {
                //Windows Azure BLOB
                serviceCollection.AddScoped<IPictureService, AzurePictureService>();
            }
            else if (useAmazonBlobStorage)
            {
                //Amazon S3 Simple Storage Service
                serviceCollection.AddScoped<IPictureService, AmazonPictureService>();
            }
            else
            {
                //standard file system
                serviceCollection.AddScoped<IPictureService, PictureService>();
            }

            serviceCollection.AddScoped<IDownloadService, DownloadService>();
        }

        private void RegisterTask(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IScheduleTaskService, ScheduleTaskService>();

            serviceCollection.AddScoped<IScheduleTask, QueuedMessagesSendScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, ClearCacheScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, ClearLogScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderAbandonedCartScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderBirthdayScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderCompletedOrderScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderLastActivityScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderLastPurchaseScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderRegisteredCustomerScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, CustomerReminderUnpaidOrderScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, DeleteGuestsScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, UpdateExchangeRateScheduleTask>();
            serviceCollection.AddScoped<IScheduleTask, EndAuctionsTask>();
            serviceCollection.AddScoped<IScheduleTask, CancelOrderScheduledTask>();

        }
    }
}
