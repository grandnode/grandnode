using Autofac;
using FluentValidation;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Http;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Core.Plugins;
using Grand.Framework.Middleware;
using Grand.Framework.Mvc.Routing;
using Grand.Framework.TagHelpers;
using Grand.Framework.Themes;
using Grand.Framework.UI;
using Grand.Framework.Validators;
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
using Grand.Services.Infrastructure;
using Grand.Services.Installation;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Logging;
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
using MongoDB.Driver;
using System;
using System.Linq;
using System.Reflection;

namespace Grand.Framework.Infrastructure
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
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {

            //web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();

            //powered by
            builder.RegisterType<PoweredByMiddlewareOptions>().As<IPoweredByMiddlewareOptions>().SingleInstance();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            builder.Register(c => dataSettingsManager.LoadSettings()).As<DataSettings>();
            builder.Register(x => new MongoDBDataProviderManager(x.Resolve<DataSettings>())).As<BaseDataProviderManager>().InstancePerDependency();
            builder.Register(x => x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var connectionString = dataProviderSettings.DataConnectionString;
                var databaseName = new MongoUrl(connectionString).DatabaseName;
                builder.Register(c => new MongoClient(connectionString).GetDatabase(databaseName)).SingleInstance();
                builder.Register<IMongoDBContext>(c => new MongoDBContext(connectionString)).InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<MongoDBContext>().As<IMongoDBContext>().InstancePerLifetimeScope();
            }

            //MongoDbRepository
            builder.RegisterGeneric(typeof(MongoDBRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            //plugins
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerLifetimeScope();

            //cache manager
            builder.RegisterType<PerRequestCacheManager>().InstancePerLifetimeScope();
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().SingleInstance();

            if (config.RedisCachingEnabled)
            {
                builder.RegisterType<DistributedRedisCache>().As<ICacheManager>().SingleInstance();
                builder.RegisterType<DistributedRedisCacheExtended>().As<IDistributedRedisCacheExtended>().SingleInstance();
            }

            if (config.RunOnAzureWebApps)
            {
                builder.RegisterType<AzureWebAppsMachineNameProvider>().As<IMachineNameProvider>().SingleInstance();
            }
            else
            {
                builder.RegisterType<DefaultMachineNameProvider>().As<IMachineNameProvider>().SingleInstance();
            }
            //work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerLifetimeScope();
            //store context
            builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerLifetimeScope();

            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().InstancePerLifetimeScope();

            //services
            builder.RegisterType<BackInStockSubscriptionService>().As<IBackInStockSubscriptionService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<CompareProductsService>().As<ICompareProductsService>().InstancePerLifetimeScope();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerService>().As<IManufacturerService>().InstancePerLifetimeScope();
            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().InstancePerLifetimeScope();

            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<CopyProductService>().As<ICopyProductService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductReservationService>().As<IProductReservationService>().InstancePerLifetimeScope();
            builder.RegisterType<AuctionService>().As<IAuctionService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductCourseService>().As<IProductCourseService>().InstancePerLifetimeScope();

            builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().InstancePerLifetimeScope();

            builder.RegisterType<ProductTemplateService>().As<IProductTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryTemplateService>().As<ICategoryTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerTemplateService>().As<IManufacturerTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<TopicTemplateService>().As<ITopicTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductTagService>().As<IProductTagService>().InstancePerLifetimeScope();
            builder.RegisterType<AddressAttributeFormatter>().As<IAddressAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<AddressAttributeParser>().As<IAddressAttributeParser>().InstancePerLifetimeScope();
            builder.RegisterType<AddressAttributeService>().As<IAddressAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerLifetimeScope();
            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerLifetimeScope();
            builder.RegisterType<VendorService>().As<IVendorService>().InstancePerLifetimeScope();
            builder.RegisterType<SearchTermService>().As<ISearchTermService>().InstancePerLifetimeScope();
            builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAttributeFormatter>().As<ICustomerAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAttributeParser>().As<ICustomerAttributeParser>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAttributeService>().As<ICustomerAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerTagService>().As<ICustomerTagService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerActionService>().As<ICustomerActionService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerActionEventService>().As<ICustomerActionEventService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderService>().As<ICustomerReminderService>().InstancePerLifetimeScope();
            builder.RegisterType<UserApiService>().As<IUserApiService>().InstancePerLifetimeScope();

            builder.RegisterType<RewardPointsService>().As<IRewardPointsService>().InstancePerLifetimeScope();

            builder.RegisterType<PermissionService>().As<IPermissionService>().InstancePerLifetimeScope();
            builder.RegisterType<AclService>().As<IAclService>().InstancePerLifetimeScope();
            builder.RegisterType<PriceCalculationService>().As<IPriceCalculationService>().InstancePerLifetimeScope();

            builder.RegisterType<GeoLookupService>().As<IGeoLookupService>().InstancePerLifetimeScope();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerLifetimeScope();

            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerLifetimeScope();
            builder.RegisterType<MeasureService>().As<IMeasureService>().InstancePerLifetimeScope();
            builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerLifetimeScope();

            builder.RegisterType<StoreService>().As<IStoreService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<DiscountService>().As<IDiscountService>().InstancePerLifetimeScope();


            builder.RegisterType<LanguageService>().As<ILanguageService>().InstancePerLifetimeScope();
            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerLifetimeScope();

            var provider = new FileExtensionContentTypeProvider();
            builder.RegisterType<MimeMappingService>().As<IMimeMappingService>()
                .WithParameter(new TypedParameter(typeof(FileExtensionContentTypeProvider), provider))
                .InstancePerLifetimeScope();

            //picture service
            var useAzureBlobStorage = !String.IsNullOrEmpty(config.AzureBlobStorageConnectionString);
            var useAmazonBlobStorage = (!String.IsNullOrEmpty(config.AmazonAwsAccessKeyId) && !String.IsNullOrEmpty(config.AmazonAwsSecretAccessKey) && !String.IsNullOrEmpty(config.AmazonBucketName) && !String.IsNullOrEmpty(config.AmazonRegion));

            if (useAzureBlobStorage)
            {
                //Windows Azure BLOB
                builder.RegisterType<AzurePictureService>().As<IPictureService>().InstancePerLifetimeScope();
            }
            else if (useAmazonBlobStorage)
            {
                //Amazon S3 Simple Storage Service
                builder.RegisterType<AmazonPictureService>().As<IPictureService>().InstancePerLifetimeScope();
            }
            else
            {
                //standard file system
                builder.RegisterType<PictureService>().As<IPictureService>().InstancePerLifetimeScope();
            }

            builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerLifetimeScope();
            builder.RegisterType<NewsletterCategoryService>().As<INewsletterCategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerLifetimeScope();
            builder.RegisterType<BannerService>().As<IBannerService>().InstancePerLifetimeScope();
            builder.RegisterType<PopupService>().As<IPopupService>().InstancePerLifetimeScope();
            builder.RegisterType<InteractiveFormService>().As<IInteractiveFormService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkflowMessageService>().As<IWorkflowMessageService>().InstancePerLifetimeScope();
            builder.RegisterType<ContactAttributeFormatter>().As<IContactAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ContactAttributeParser>().As<IContactAttributeParser>().InstancePerLifetimeScope();
            builder.RegisterType<ContactAttributeService>().As<IContactAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<MessageTokenProvider>().As<IMessageTokenProvider>().InstancePerLifetimeScope();
            builder.RegisterType<Tokenizer>().As<ITokenizer>().InstancePerLifetimeScope();
            builder.RegisterType<EmailSender>().As<IEmailSender>().InstancePerLifetimeScope();
            builder.RegisterType<HistoryService>().As<IHistoryService>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutAttributeFormatter>().As<ICheckoutAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutAttributeParser>().As<ICheckoutAttributeParser>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutAttributeService>().As<ICheckoutAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<GiftCardService>().As<IGiftCardService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportService>().As<IOrderReportService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderTotalCalculationService>().As<IOrderTotalCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReturnRequestService>().As<IReturnRequestService>().InstancePerLifetimeScope();
            builder.RegisterType<RewardPointsService>().As<IRewardPointsService>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentService>().As<IPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerLifetimeScope();
            builder.RegisterType<CookieAuthenticationService>().As<IGrandAuthenticationService>().InstancePerLifetimeScope();
            builder.RegisterType<ApiAuthenticationService>().As<IApiAuthenticationService>().InstancePerLifetimeScope();
            builder.RegisterType<UrlRecordService>().As<IUrlRecordService>().InstancePerLifetimeScope();
            builder.RegisterType<ShipmentService>().As<IShipmentService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingService>().As<IShippingService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerLifetimeScope();
            builder.RegisterType<ContactUsService>().As<IContactUsService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityKeywordsProvider>().As<IActivityKeywordsProvider>().InstancePerLifetimeScope();

            bool databaseInstalled = DataSettingsHelper.DatabaseIsInstalled();
            if (!databaseInstalled)
            {
                //installation service
                builder.RegisterType<CodeFirstInstallationService>().As<IInstallationService>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<UpgradeService>().As<IUpgradeService>().InstancePerLifetimeScope();
            }

            builder.RegisterType<ForumService>().As<IForumService>().InstancePerLifetimeScope();
            builder.RegisterType<KnowledgebaseService>().As<IKnowledgebaseService>().InstancePerLifetimeScope();
            builder.RegisterType<PushNotificationsService>().As<IPushNotificationsService>().InstancePerLifetimeScope();
            builder.RegisterType<PollService>().As<IPollService>().InstancePerLifetimeScope();
            builder.RegisterType<BlogService>().As<IBlogService>().InstancePerLifetimeScope();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerLifetimeScope();
            builder.RegisterType<TopicService>().As<ITopicService>().InstancePerLifetimeScope();
            builder.RegisterType<NewsService>().As<INewsService>().InstancePerLifetimeScope();
            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerLifetimeScope();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerLifetimeScope();
            builder.RegisterType<PageHeadBuilder>().As<IPageHeadBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerLifetimeScope();
            builder.RegisterType<ExportManager>().As<IExportManager>().InstancePerLifetimeScope();
            builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerLifetimeScope();
            builder.RegisterType<PdfService>().As<IPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerLifetimeScope();
            builder.RegisterType<ExternalAuthenticationService>().As<IExternalAuthenticationService>().InstancePerLifetimeScope();
            builder.RegisterType<GoogleAnalyticsService>().As<IGoogleAnalyticsService>().InstancePerLifetimeScope();
            builder.RegisterType<DocumentTypeService>().As<IDocumentTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<DocumentService>().As<IDocumentService>().InstancePerLifetimeScope();

            builder.RegisterType<CourseActionService>().As<ICourseActionService>().InstancePerLifetimeScope();
            builder.RegisterType<CourseLessonService>().As<ICourseLessonService>().InstancePerLifetimeScope();
            builder.RegisterType<CourseLevelService>().As<ICourseLevelService>().InstancePerLifetimeScope();
            builder.RegisterType<CourseService>().As<ICourseService>().InstancePerLifetimeScope();
            builder.RegisterType<CourseSubjectService>().As<ICourseSubjectService>().InstancePerLifetimeScope();

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            var validators = typeFinder.FindClassesOfType(typeof(IValidator)).ToList();
            foreach (var validator in validators)
            {
                builder.RegisterType(validator);
            }

            //validator consumers
            var validatorconsumers = typeFinder.FindClassesOfType(typeof(IValidatorConsumer<>)).ToList();
            foreach (var consumer in validatorconsumers)
            {
                builder.RegisterType(consumer)
                    .As(consumer.GetTypeInfo().FindInterfaces((type, criteria) =>
                    {
                        var isMatch = type.GetTypeInfo().IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                        return isMatch;
                    }, typeof(IValidatorConsumer<>)))
                    .InstancePerLifetimeScope();
            }

            builder.RegisterType<ResourceManager>().As<IResourceManager>().InstancePerLifetimeScope();

            //Register task
            builder.RegisterType<QueuedMessagesSendScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<ClearCacheScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<ClearLogScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderAbandonedCartScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderBirthdayScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderCompletedOrderScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderLastActivityScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderLastPurchaseScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderRegisteredCustomerScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReminderUnpaidOrderScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteGuestsScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateExchangeRateScheduleTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            builder.RegisterType<EndAuctionsTask>().As<IScheduleTask>().InstancePerLifetimeScope();

        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order {
            get { return 0; }
        }
    }

}
