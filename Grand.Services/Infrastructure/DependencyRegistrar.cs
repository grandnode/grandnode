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
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            RegisterMachineNameProvider(builder, config);

            RegisterConfigurationService(builder);

            RegisterAffiliateService(builder);

            RegisterAuthenticationService(builder);

            RegisterBlogService(builder);

            RegisterCatalogService(builder);

            RegisterCmsService(builder);

            RegisterCommonService(builder);

            RegisterCoursesService(builder);

            RegisterCustomerService(builder);

            RegisterDirectoryService(builder);

            RegisterDiscountsService(builder);

            RegisterDocumentsService(builder);

            RegisterExportImportService(builder);

            RegisterForumService(builder);

            RegisterInstallService(builder);

            RegisterKnowledgebaseService(builder);

            RegisterLocalizationService(builder);

            RegisterLoggingService(builder);

            RegisterMediaService(builder, config);

            RegisterMessageService(builder);

            RegisterNewsService(builder);

            RegisterOrdersService(builder);

            RegisterPaymentsService(builder);

            RegisterPollsService(builder);

            RegisterPushService(builder);

            RegisterSecurityService(builder);

            RegisterSeoService(builder);

            RegisterShippingService(builder);

            RegisterStoresService(builder);

            RegisterTaxService(builder);

            RegisterTopicsService(builder);

            RegisterTask(builder);

        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order {
            get { return 1; }
        }

        private void RegisterMachineNameProvider(IServiceCollection builder, GrandConfig config)
        {
            if (config.RunOnAzureWebApps)
            {
                builder.AddSingleton<IMachineNameProvider, AzureWebAppsMachineNameProvider>();
            }
            else
            {
                builder.AddSingleton<IMachineNameProvider, DefaultMachineNameProvider>();
            }
        }

        private void RegisterAffiliateService(IServiceCollection builder)
        {
            builder.AddScoped<IAffiliateService, AffiliateService>();
        }

        private void RegisterAuthenticationService(IServiceCollection builder)
        {
            builder.AddScoped<IGrandAuthenticationService, CookieAuthenticationService>();
            builder.AddScoped<IApiAuthenticationService, ApiAuthenticationService>();
            builder.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            builder.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
        }
        private void RegisterCommonService(IServiceCollection builder)
        {
            builder.AddScoped<IAddressAttributeParser, AddressAttributeParser>();
            builder.AddScoped<IAddressAttributeService, AddressAttributeService>();
            builder.AddScoped<IAddressService, AddressService>();
            builder.AddScoped<IGenericAttributeService, GenericAttributeService>();
            builder.AddScoped<IHistoryService, HistoryService>();
            builder.AddScoped<IPdfService, WkPdfService>();
            builder.AddScoped<IViewRenderService, ViewRenderService>();
            builder.AddScoped<ISearchTermService, SearchTermService>();
            builder.AddScoped<IDateTimeHelper, DateTimeHelper>();
            builder.AddScoped<ICookiePreference, CookiePreference>();
        }
        private void RegisterCatalogService(IServiceCollection builder)
        {
            builder.AddScoped<IBackInStockSubscriptionService, BackInStockSubscriptionService>();
            builder.AddScoped<ICategoryService, CategoryService>();
            builder.AddScoped<ICompareProductsService, CompareProductsService>();
            builder.AddScoped<IRecentlyViewedProductsService, RecentlyViewedProductsService>();
            builder.AddScoped<IManufacturerService, ManufacturerService>();
            builder.AddScoped<IPriceFormatter, PriceFormatter>();
            builder.AddScoped<IProductAttributeFormatter, ProductAttributeFormatter>();
            builder.AddScoped<IProductAttributeParser, ProductAttributeParser>();
            builder.AddScoped<IProductAttributeService, ProductAttributeService>();
            builder.AddScoped<IProductService, ProductService>();
            builder.AddScoped<IProductReviewService, ProductReviewService>();
            builder.AddScoped<ICopyProductService, CopyProductService>();
            builder.AddScoped<IProductReservationService, ProductReservationService>();
            builder.AddScoped<IAuctionService, AuctionService>();
            builder.AddScoped<IProductCourseService, ProductCourseService>();
            builder.AddScoped<ISpecificationAttributeService, SpecificationAttributeService>();
            builder.AddScoped<IProductTemplateService, ProductTemplateService>();
            builder.AddScoped<ICategoryTemplateService, CategoryTemplateService>();
            builder.AddScoped<IManufacturerTemplateService, ManufacturerTemplateService>();
            builder.AddScoped<IProductTagService, ProductTagService>();
            builder.AddScoped<IInventoryManageService, InventoryManageService>();
            builder.AddScoped<IPriceCalculationService, PriceCalculationService>();
            builder.AddScoped<IOrderTagService, OrderTagService>();

        }

        private void RegisterCoursesService(IServiceCollection builder)
        {
            builder.AddScoped<ICourseActionService, CourseActionService>();
            builder.AddScoped<ICourseLessonService, CourseLessonService>();
            builder.AddScoped<ICourseLevelService, CourseLevelService>();
            builder.AddScoped<ICourseService, CourseService>();
            builder.AddScoped<ICourseSubjectService, CourseSubjectService>();

        }

        private void RegisterCustomerService(IServiceCollection builder)
        {
            builder.AddScoped<IVendorService, VendorService>();
            builder.AddScoped<ICustomerAttributeParser, CustomerAttributeParser>();
            builder.AddScoped<ICustomerAttributeService, CustomerAttributeService>();
            builder.AddScoped<ICustomerService, CustomerService>();
            builder.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
            builder.AddScoped<ICustomerReportService, CustomerReportService>();
            builder.AddScoped<ICustomerTagService, CustomerTagService>();
            builder.AddScoped<ICustomerActionService, CustomerActionService>();
            builder.AddScoped<ICustomerActionEventService, CustomerActionEventService>();
            builder.AddScoped<ICustomerReminderService, CustomerReminderService>();
            builder.AddScoped<ICustomerProductService, CustomerProductService>();
            builder.AddScoped<ICustomerCoordinatesService, CustomerCoordinatesService>();
            builder.AddScoped<ISalesEmployeeService, SalesEmployeeService>();
            builder.AddScoped<IUserApiService, UserApiService>();

        }

        private void RegisterDirectoryService(IServiceCollection builder)
        {
            builder.AddScoped<IGeoLookupService, GeoLookupService>();
            builder.AddScoped<ICountryService, CountryService>();

            builder.AddScoped<ICurrencyService, CurrencyService>();
            builder.AddScoped<IMeasureService, MeasureService>();
            builder.AddScoped<IStateProvinceService, StateProvinceService>();

        }

        private void RegisterDocumentsService(IServiceCollection builder)
        {
            builder.AddScoped<IDocumentTypeService, DocumentTypeService>();
            builder.AddScoped<IDocumentService, DocumentService>();

        }

        private void RegisterDiscountsService(IServiceCollection builder)
        {
            builder.AddScoped<IDiscountService, DiscountService>();
        }

        private void RegisterBlogService(IServiceCollection builder)
        {
            builder.AddScoped<IBlogService, BlogService>();
        }

        private void RegisterCmsService(IServiceCollection builder)
        {
            builder.AddScoped<IWidgetService, WidgetService>();

        }

        private void RegisterConfigurationService(IServiceCollection builder)
        {
            builder.AddScoped<ISettingService, SettingService>();
            builder.AddScoped<IGoogleAnalyticsService, GoogleAnalyticsService>();
        }

        private void RegisterExportImportService(IServiceCollection builder)
        {
            builder.AddScoped<IExportManager, ExportManager>();
            builder.AddScoped<IImportManager, ImportManager>();
        }

        private void RegisterForumService(IServiceCollection builder)
        {
            builder.AddScoped<IForumService, ForumService>();
        }

        private void RegisterInstallService(IServiceCollection builder)
        {
            var databaseInstalled = DataSettingsHelper.DatabaseIsInstalled();
            if (!databaseInstalled)
            {
                //installation service
                builder.AddScoped<IInstallationLocalizationService, InstallationLocalizationService>();
                builder.AddScoped<IInstallationService, CodeFirstInstallationService>();
            }
            else
            {
                builder.AddScoped<IUpgradeService, UpgradeService>();
            }
        }

        private void RegisterKnowledgebaseService(IServiceCollection builder)
        {
            builder.AddScoped<IKnowledgebaseService, KnowledgebaseService>();
        }

        private void RegisterLocalizationService(IServiceCollection builder)
        {
            builder.AddScoped<ILocalizationService, LocalizationService>();
            builder.AddScoped<ILanguageService, LanguageService>();

        }

        private void RegisterLoggingService(IServiceCollection builder)
        {
            builder.AddScoped<ICustomerActivityService, CustomerActivityService>();
            builder.AddScoped<IActivityKeywordsProvider, ActivityKeywordsProvider>();
            builder.AddScoped<ILogger, DefaultLogger>();

        }

        private void RegisterMessageService(IServiceCollection builder)
        {
            builder.AddScoped<IBannerService, BannerService>();
            builder.AddScoped<IPopupService, PopupService>();
            builder.AddScoped<IInteractiveFormService, InteractiveFormService>();
            builder.AddScoped<INewsLetterSubscriptionService, NewsLetterSubscriptionService>();
            builder.AddScoped<INewsletterCategoryService, NewsletterCategoryService>();
            builder.AddScoped<ICampaignService, CampaignService>();
            builder.AddScoped<IMessageTemplateService, MessageTemplateService>();
            builder.AddScoped<IQueuedEmailService, QueuedEmailService>();
            builder.AddScoped<IEmailAccountService, EmailAccountService>();
            builder.AddScoped<IWorkflowMessageService, WorkflowMessageService>();
            builder.AddScoped<IMessageTokenProvider, MessageTokenProvider>();
            builder.AddScoped<ITokenizer, Tokenizer>();
            builder.AddScoped<IEmailSender, EmailSender>();

            builder.AddScoped<IContactAttributeParser, ContactAttributeParser>();
            builder.AddScoped<IContactAttributeService, ContactAttributeService>();

            builder.AddScoped<IContactUsService, ContactUsService>();
        }
        private void RegisterNewsService(IServiceCollection builder)
        {
            builder.AddScoped<INewsService, NewsService>();
        }

        private void RegisterOrdersService(IServiceCollection builder)
        {
            builder.AddScoped<IRewardPointsService, RewardPointsService>();
            builder.AddScoped<IGiftCardService, GiftCardService>();
            builder.AddScoped<IOrderService, OrderService>();
            builder.AddScoped<IOrderReportService, OrderReportService>();
            builder.AddScoped<IOrderProcessingService, OrderProcessingService>();
            builder.AddScoped<IOrderConfirmationService, OrderConfirmationService>();
            builder.AddScoped<IOrderRecurringPayment, OrderRecurringPayment>();
            builder.AddScoped<IOrderTotalCalculationService, OrderTotalCalculationService>();
            builder.AddScoped<IReturnRequestService, ReturnRequestService>();
            builder.AddScoped<IRewardPointsService, RewardPointsService>();
            builder.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.AddScoped<ICheckoutAttributeFormatter, CheckoutAttributeFormatter>();
            builder.AddScoped<ICheckoutAttributeParser, CheckoutAttributeParser>();
            builder.AddScoped<ICheckoutAttributeService, CheckoutAttributeService>();

        }
        private void RegisterPaymentsService(IServiceCollection builder)
        {
            builder.AddScoped<IPaymentService, PaymentService>();
        }
        private void RegisterPollsService(IServiceCollection builder)
        {
            builder.AddScoped<IPollService, PollService>();
        }

        private void RegisterPushService(IServiceCollection builder)
        {
            builder.AddScoped<IPushNotificationsService, PushNotificationsService>();
        }

        private void RegisterSecurityService(IServiceCollection builder)
        {
            builder.AddScoped<IPermissionService, PermissionService>();
            builder.AddScoped<IAclService, AclService>();
            builder.AddScoped<IEncryptionService, EncryptionService>();
        }

        private void RegisterSeoService(IServiceCollection builder)
        {
            builder.AddScoped<ISitemapGenerator, SitemapGenerator>();
            builder.AddScoped<IUrlRecordService, UrlRecordService>();

        }

        private void RegisterShippingService(IServiceCollection builder)
        {
            builder.AddScoped<IShipmentService, ShipmentService>();
            builder.AddScoped<IShippingService, ShippingService>();
            builder.AddScoped<IPickupPointService, PickupPointService>();
            builder.AddScoped<IDeliveryDateService, DeliveryDateService>();
            builder.AddScoped<IWarehouseService, WarehouseService>();
            builder.AddScoped<IShippingMethodService, ShippingMethodService>();
        }
        private void RegisterStoresService(IServiceCollection builder)
        {
            builder.AddScoped<IStoreService, StoreService>();
            builder.AddScoped<IStoreMappingService, StoreMappingService>();
        }
        private void RegisterTaxService(IServiceCollection builder)
        {
            builder.AddScoped<ITaxService, TaxService>();
            builder.AddScoped<IVatService, VatService>();
            builder.AddScoped<ITaxCategoryService, TaxCategoryService>();
        }

        private void RegisterTopicsService(IServiceCollection builder)
        {
            builder.AddScoped<ITopicTemplateService, TopicTemplateService>();
            builder.AddScoped<ITopicService, TopicService>();

        }

        private void RegisterMediaService(IServiceCollection builder, GrandConfig config)
        {


            builder.AddScoped<IMimeMappingService, MimeMappingService>(x =>
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
                builder.AddScoped<IPictureService, AzurePictureService>();
            }
            else if (useAmazonBlobStorage)
            {
                //Amazon S3 Simple Storage Service
                builder.AddScoped<IPictureService, AmazonPictureService>();
            }
            else
            {
                //standard file system
                builder.AddScoped<IPictureService, PictureService>();
            }

            builder.AddScoped<IDownloadService, DownloadService>();
        }

        private void RegisterTask(IServiceCollection builder)
        {
            builder.AddScoped<IScheduleTaskService, ScheduleTaskService>();

            builder.AddScoped<IScheduleTask, QueuedMessagesSendScheduleTask>();
            builder.AddScoped<IScheduleTask, ClearCacheScheduleTask>();
            builder.AddScoped<IScheduleTask, ClearLogScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderAbandonedCartScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderBirthdayScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderCompletedOrderScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderLastActivityScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderLastPurchaseScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderRegisteredCustomerScheduleTask>();
            builder.AddScoped<IScheduleTask, CustomerReminderUnpaidOrderScheduleTask>();
            builder.AddScoped<IScheduleTask, DeleteGuestsScheduleTask>();
            builder.AddScoped<IScheduleTask, UpdateExchangeRateScheduleTask>();
            builder.AddScoped<IScheduleTask, EndAuctionsTask>();
            builder.AddScoped<IScheduleTask, CancelOrderScheduledTask>();

        }
    }
}
