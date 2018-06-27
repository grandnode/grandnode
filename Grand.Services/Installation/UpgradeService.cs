using MongoDB.Driver;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Configuration;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Topics;
using Grand.Core.Infrastructure;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Seo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Tasks;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Logging;
using Grand.Services.Security;
using MongoDB.Bson;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Bindings;
using System.Threading;
using Grand.Services.Topics;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain;
using Grand.Core.Domain.PushNotifications;

namespace Grand.Services.Installation
{
    public partial class UpgradeService : IUpgradeService
    {
        #region Fields

        private readonly IRepository<GrandNodeVersion> _versionRepository;
        private readonly IWebHelper _webHelper;

        private const string version_360 = "3.60";
        private const string version_370 = "3.70";
        private const string version_380 = "3.80";
        private const string version_390 = "3.90";
        private const string version_400 = "4.00";
        private const string version_410 = "4.10";
        private const string version_420 = "4.20";

        #endregion

        #region Ctor
        public UpgradeService(IRepository<GrandNodeVersion> versionRepository, IWebHelper webHelper)
        {
            this._versionRepository = versionRepository;
            this._webHelper = webHelper;
        }
        #endregion

        public virtual string DatabaseVersion()
        {
            string version = version_360;
            var databaseversion = _versionRepository.Table.FirstOrDefault();
            if (databaseversion != null)
                version = databaseversion.DataBaseVersion;
            return version;
        }
        public virtual void UpgradeData(string fromversion, string toversion)
        {
            if (fromversion == version_360)
            {
                From360To370();
                fromversion = version_370;
            }

            if (fromversion == version_380)
            {
                From380To390();
                fromversion = version_390;
            }

            if (fromversion == version_390)
            {
                From390To400();
                fromversion = version_400;
            }

            if (fromversion == version_400)
            {
                From400To410();
                fromversion = version_410;
            }
            if (fromversion == version_410)
            {
                From410To420();
                fromversion = version_420;
            }
            if (fromversion == toversion)
            {
                var databaseversion = _versionRepository.Table.FirstOrDefault();
                if (databaseversion != null)
                {
                    databaseversion.DataBaseVersion = GrandVersion.CurrentVersion;
                    _versionRepository.Update(databaseversion);
                }
                else
                {
                    databaseversion = new GrandNodeVersion();
                    databaseversion.DataBaseVersion = GrandVersion.CurrentVersion;
                    _versionRepository.Insert(databaseversion);
                }
            }
        }

        private void From360To370()
        {
            #region Install String resources
                InstallStringResources("360_370.nopres.xml");
            #endregion

            #region MessageTemplates

            var eaGeneral = EngineContext.Current.Resolve<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
                               {
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
            EngineContext.Current.Resolve<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion

            #region Topics

            var defaultTopicTemplate = EngineContext.Current.Resolve<IRepository<TopicTemplate>>().Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                throw new Exception("Topic template cannot be loaded");

            var topics = new List<Topic>
            {
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
            EngineContext.Current.Resolve<IRepository<Topic>>().Insert(topics);

            var ltopics = EngineContext.Current.Resolve<IRepository<Topic>>().Table.Where(x => x.SystemName == "ApplyVendor");
            //search engine names
            foreach (var topic in ltopics)
            {
                var seName = topic.ValidateSeName("", !String.IsNullOrEmpty(topic.Title) ? topic.Title : topic.SystemName, true);
                EngineContext.Current.Resolve<IRepository<UrlRecord>>().Insert(new UrlRecord
                {
                    EntityId = topic.Id,
                    EntityName = "Topic",
                    LanguageId = "",
                    IsActive = true,
                    Slug = seName
                });
                topic.SeName = seName;
                EngineContext.Current.Resolve<IRepository<Topic>>().Update(topic);
            }


            #endregion

            #region Settings

            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.AllowViewUnpublishedProductPage", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.PublishBackProductWhenCancellingOrders", Value = "false", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.NewProductsNumber", Value = "6", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.NewProductsEnabled", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.AjaxProcessAttributeChange", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.DisplayTaxShippingInfoShoppingCart", Value = "false", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CustomerSettings.DateOfBirthRequired", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "MediaSettings.VendorThumbPictureSize", Value = "450", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "StoreInformationSettings.HidePoweredByGrandNode", Value = "false", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "RewardPointsSettings.PointsAccumulatedForAllStores", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "VendorSettings.AllowCustomersToApplyForVendorAccount", Value = "true", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "PaymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods", Value = "false", StoreId = "" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CommonSettings.SitemapCustomUrls", Value = "", StoreId = "" });

            #endregion

            #region Product Template

            var productTemplateGrouped = EngineContext.Current.Resolve<IRepository<ProductTemplate>>().Table.FirstOrDefault(pt => pt.Name == "Grouped product");
            if (productTemplateGrouped != null)
            {
                productTemplateGrouped.Name = "Grouped product (with variants)";
                EngineContext.Current.Resolve<IRepository<ProductTemplate>>().Update(productTemplateGrouped);
            }

            #endregion

            #region InstallReturnRequestReason

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
            EngineContext.Current.Resolve<IRepository<ReturnRequestReason>>().Insert(returnRequestReasons);

            #endregion

            #region InstallReturnRequestAction

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
            EngineContext.Current.Resolve<IRepository<ReturnRequestAction>>().Insert(returnRequestActions);

            #endregion

        }

        private void From380To390()
        {

            #region Run scripts
            
            var filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/Upgrade"), "UpgradeScript_380_390.js");
            string upgrade_script = File.ReadAllText(filePath);
            var bscript = new BsonJavaScript(upgrade_script);
            var operation = new EvalOperation(_versionRepository.Database.DatabaseNamespace, bscript, null);
            var writeBinding = new WritableServerBinding(_versionRepository.Database.Client.Cluster, NoCoreSession.NewHandle());
            operation.Execute(writeBinding, CancellationToken.None);

            #endregion

            #region Install String resources
            InstallStringResources("380_390.nopres.xml");
            #endregion

            #region Install forum Vote
            var _forumPostVote = EngineContext.Current.Resolve<IRepository<ForumPostVote>>();
            _forumPostVote.Collection.Indexes.DropAll();
            _forumPostVote.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ForumPostVote>((Builders<ForumPostVote>.IndexKeys.Ascending(x => x.ForumPostId).Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "Vote", Unique = true }));

            #endregion

            #region Install new task

            var shtask1 = new ScheduleTask
            {
                ScheduleTaskName = "Customer reminder - Completed order",
                Type = "Grand.Services.Tasks.CustomerReminderCompletedOrderScheduleTask, Grand.Services",
                Enabled = true,
                StopOnError = false,
                LastStartUtc = DateTime.MinValue,
                LastNonSuccessEndUtc = DateTime.MinValue,
                LastSuccessUtc = DateTime.MinValue,
                TimeIntervalChoice = TimeIntervalChoice.EveryDays,
                TimeInterval = 1,
                MinuteOfHour = 1,
                HourOfDay = 1,
                DayOfWeek = DayOfWeek.Thursday,
                MonthOptionChoice = MonthOptionChoice.OnSpecificDay,
                DayOfMonth = 1
            };
            EngineContext.Current.Resolve<IRepository<ScheduleTask>>().Insert(shtask1);

            var shtask2 = new ScheduleTask
            {
                ScheduleTaskName = "Customer reminder - Unpaid order",
                Type = "Grand.Services.Tasks.CustomerReminderUnpaidOrderScheduleTask, Grand.Services",
                Enabled = true,
                StopOnError = false,
                LastStartUtc = DateTime.MinValue,
                LastNonSuccessEndUtc = DateTime.MinValue,
                LastSuccessUtc = DateTime.MinValue,
                TimeIntervalChoice = TimeIntervalChoice.EveryDays,
                TimeInterval = 1,
                MinuteOfHour = 1,
                HourOfDay = 1,
                DayOfWeek = DayOfWeek.Thursday,
                MonthOptionChoice = MonthOptionChoice.OnSpecificDay,
                DayOfMonth = 1
            };
            EngineContext.Current.Resolve<IRepository<ScheduleTask>>().Insert(shtask2);

            #endregion

            #region MessageTemplates

            var emailAccount = EngineContext.Current.Resolve<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
                             {
                                new MessageTemplate
                                {
                                    Name = "NewReturnRequest.CustomerNotification",
                                    Subject = "%Store.Name%. New return request.",
                                    Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%!<br /> You have just submitted a new return request. Details are below:<br />Request ID: %ReturnRequest.CustomNumber%<br />Product: %ReturnRequest.Product.Quantity% x Product: %ReturnRequest.Product.Name%<br />Reason for return: %ReturnRequest.Reason%<br />Requested action: %ReturnRequest.RequestedAction%<br />Customer comments:<br />%ReturnRequest.CustomerComment%</p>",
                                    IsActive = true,
                                    EmailAccountId = emailAccount.Id,
                                },
                                new MessageTemplate
                                {
                                    Name = "Service.ContactUs",
                                    Subject = "%Store.Name%. Contact us",
                                    Body = string.Format("<p>From %ContactUs.SenderName% - %ContactUs.SenderEmail% {0} %ContactUs.Body%{0}</p>{0}", Environment.NewLine),
                                    IsActive = true,
                                    EmailAccountId = emailAccount.Id,
                                },
                                new MessageTemplate
                                {
                                    Name = "Service.ContactVendor",
                                    Subject = "%Store.Name%. Contact us",
                                    Body = string.Format("<p>From %ContactUs.SenderName% - %ContactUs.SenderEmail% {0} %ContactUs.Body%{0}</p>{0}", Environment.NewLine),
                                    IsActive = true,
                                    EmailAccountId = emailAccount.Id,
                                },
                            };

            EngineContext.Current.Resolve<IRepository<MessageTemplate>>().Insert(messageTemplates);


            #endregion

            #region Recently Viewed products

            var _recentlyViewedProductRepository = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<RecentlyViewedProduct>>();
            _recentlyViewedProductRepository.Collection.Indexes.DropAll();
            _recentlyViewedProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<RecentlyViewedProduct>((Builders<RecentlyViewedProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId.ProductId" }));

            #endregion

            #region Rebuild index

            //customer
            var _customerRepository = EngineContext.Current.Resolve<IRepository<Customer>>();
            _customerRepository.Collection.Indexes.DropAll();
            _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Descending(x => x.CreatedOnUtc).Ascending(x => x.Deleted).Ascending("CustomerRoles._id")), new CreateIndexOptions() { Name = "CreatedOnUtc_1_CustomerRoles._id_1", Unique = false }));
            _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.LastActivityDateUtc)), new CreateIndexOptions() { Name = "LastActivityDateUtc_1", Unique = false }));
            _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.CustomerGuid)), new CreateIndexOptions() { Name = "CustomerGuid_1", Unique = false }));
            _customerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Customer>((Builders<Customer>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email_1", Unique = false }));

            //customer history password
            var _customerHistoryPasswordRepository = EngineContext.Current.Resolve<IRepository<CustomerHistoryPassword>>();
            _customerHistoryPasswordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerHistoryPassword>((Builders<CustomerHistoryPassword>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId", Unique = false }));

            //category
            var _categoryRepository = EngineContext.Current.Resolve<IRepository<Category>>();
            _categoryRepository.Collection.Indexes.DropAll();
            _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder_1", Unique = false }));
            _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.ParentCategoryId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "ParentCategoryId_1_DisplayOrder_1", Unique = false }));

            //manufacturer
            var _manufacturerRepository = EngineContext.Current.Resolve<IRepository<Manufacturer>>();
            _manufacturerRepository.Collection.Indexes.DropAll();
            _manufacturerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Manufacturer>((Builders<Manufacturer>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder_1", Unique = false }));
            _manufacturerRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Manufacturer>((Builders<Manufacturer>.IndexKeys.Ascending("AppliedDiscounts")), new CreateIndexOptions() { Name = "AppliedDiscounts._id_1", Unique = false }));

            //Product
            var _productRepository = EngineContext.Current.Resolve<IRepository<Product>>();
            _productRepository.Collection.Indexes.DropAll();
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.MarkAsNew).Ascending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "MarkAsNew_1_CreatedOnUtc_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.ShowOnHomePage).Ascending(x => x.Published).Ascending(x => x.DisplayOrder).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ShowOnHomePage_1_Published_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.ParentGroupedProductId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "ParentGroupedProductId_1_DisplayOrder_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.ProductTags).Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductTags._id_1_Name_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Name)), new CreateIndexOptions() { Name = "Name_1", Unique = false }));

            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending("ProductCategories.DisplayOrder")), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_DisplayOrder_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.DisplayOrderCategory)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_OrderCategory_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Name_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Sold)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Sold_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Price)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_Price_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductCategories.CategoryId").Ascending("ProductCategories.IsFeaturedProduct").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually)), new CreateIndexOptions() { Name = "ProductCategories.CategoryId_1_IsFeaturedProduct_1", Unique = false }));

            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.DisplayOrderManufacturer)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_OrderCategory_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Name)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_Name_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Sold)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_Sold_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending(x => x.Price)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_Price_1", Unique = false }));
            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending("ProductManufacturers.ManufacturerId").Ascending("ProductManufacturers.IsFeaturedProduct").Ascending(x => x.Published).Ascending(x => x.VisibleIndividually)), new CreateIndexOptions() { Name = "ProductManufacturers.ManufacturerId_1_IsFeaturedProduct_1", Unique = false }));

            _productRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>((Builders<Product>.IndexKeys.Ascending(x => x.Published).Ascending(x => x.VisibleIndividually).Ascending("ProductSpecificationAttributes.SpecificationAttributeOptionId").Ascending("ProductSpecificationAttributes.AllowFiltering")), new CreateIndexOptions() { Name = "ProductSpecificationAttributes", Unique = false }));

            //ProductReview
            var _productReviewRepository = EngineContext.Current.Resolve<IRepository<ProductReview>>();
            _productReviewRepository.Collection.Indexes.DropAll();
            _productReviewRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReview>((Builders<ProductReview>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "ProductId", Unique = false }));


            //topic
            var _topicRepository = EngineContext.Current.Resolve<IRepository<Topic>>();
            _topicRepository.Collection.Indexes.DropAll();
            _topicRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Topic>((Builders<Topic>.IndexKeys.Ascending(x => x.SystemName)), new CreateIndexOptions() { Name = "SystemName", Unique = false }));

            //news
            var _newsItemRepository = EngineContext.Current.Resolve<IRepository<NewsItem>>();
            _newsItemRepository.Collection.Indexes.DropAll();
            _newsItemRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsItem>((Builders<NewsItem>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false }));

            //newsletter
            var _newslettersubscriptionRepository = EngineContext.Current.Resolve<IRepository<NewsLetterSubscription>>();
            _newslettersubscriptionRepository.Collection.Indexes.DropAll();
            _newslettersubscriptionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsLetterSubscription>((Builders<NewsLetterSubscription>.IndexKeys.Ascending(x => x.CustomerId)), new CreateIndexOptions() { Name = "CustomerId", Unique = false }));
            _newslettersubscriptionRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<NewsLetterSubscription>((Builders<NewsLetterSubscription>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email", Unique = false }));

            //Log
            var _logRepository = EngineContext.Current.Resolve<IRepository<Log>>();
            _logRepository.Collection.Indexes.DropAll();
            _logRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Log>((Builders<Log>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc", Unique = false }));

            //search term
            var _searchtermRepository = EngineContext.Current.Resolve<IRepository<SearchTerm>>();
            _searchtermRepository.Collection.Indexes.DropAll();
            _searchtermRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<SearchTerm>((Builders<SearchTerm>.IndexKeys.Descending(x => x.Count)), new CreateIndexOptions() { Name = "Count", Unique = false }));

            //setting
            var _searchtermsRepository = EngineContext.Current.Resolve<IRepository<Setting>>();
            _searchtermsRepository.Collection.Indexes.DropAll();
            _searchtermsRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Setting>((Builders<Setting>.IndexKeys.Ascending(x => x.Name)), new CreateIndexOptions() { Name = "Name", Unique = false }));

            //order
            var _orderRepository = EngineContext.Current.Resolve<IRepository<Order>>();
            _orderRepository.Collection.Indexes.DropAll();
            _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId_1_CreatedOnUtc_-1", Unique = false }));
            _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CreatedOnUtc_-1", Unique = false }));
            _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Descending(x => x.OrderNumber)), new CreateIndexOptions() { Name = "OrderNumber", Unique = true }));
            _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending("OrderItems.ProductId")), new CreateIndexOptions() { Name = "OrderItemsProductId" }));
            _orderRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Order>((Builders<Order>.IndexKeys.Ascending("OrderItems._id")), new CreateIndexOptions() { Name = "OrderItemId" }));

            //url record
            var _urlRecordRepository = EngineContext.Current.Resolve<IRepository<UrlRecord>>();
            _urlRecordRepository.Collection.Indexes.DropAll();
            _urlRecordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<UrlRecord>((Builders<UrlRecord>.IndexKeys.Ascending(x => x.Slug).Ascending(x => x.IsActive)), new CreateIndexOptions() { Name = "Slug" }));
            _urlRecordRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<UrlRecord>((Builders<UrlRecord>.IndexKeys.Ascending(x => x.EntityId).Ascending(x => x.EntityName).Ascending(x => x.LanguageId).Ascending(x => x.IsActive)), new CreateIndexOptions() { Name = "UrlRecord" }));

            #endregion

            #region Settings

            var settingService = EngineContext.Current.Resolve<ISettingService>();

            var catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();
            catalogSettings.LimitOfFeaturedProducts = 30;
            settingService.SaveSetting(catalogSettings, x => x.LimitOfFeaturedProducts, "", false);

            var adminAreaSettings = EngineContext.Current.Resolve<AdminAreaSettings>();
            adminAreaSettings.UseIsoDateTimeConverterInJson = true;
            settingService.SaveSetting(adminAreaSettings, x => x.UseIsoDateTimeConverterInJson, "", false);

            settingService.SaveSetting(new MenuItemSettings
            {
                DisplayHomePageMenu = false,
                DisplayNewProductsMenu = false,
                DisplaySearchMenu = false,
                DisplayCustomerMenu = false,
                DisplayBlogMenu = false,
                DisplayForumsMenu = false,
                DisplayContactUsMenu = false
            });


            #endregion

            #region ActivityLog

            var _activityLogTypeRepository = EngineContext.Current.Resolve<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "PublicStore.Url",
                Enabled = false,
                Name = "Public store. Viewed Url"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "InteractiveFormDelete",
                Enabled = true,
                Name = "Delete a interactive form"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "InteractiveFormEdit",
                Enabled = true,
                Name = "Edit a interactive form"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "InteractiveFormAdd",
                Enabled = true,
                Name = "Add a interactive form"
            });


            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);
             
            #endregion
        }

        private void From390To400()
        {            
            #region Install String resources
            InstallStringResources("390_400.nopres.xml");
            #endregion

            #region MessageTemplates

            var emailAccount = EngineContext.Current.Resolve<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
                             {
                                new MessageTemplate
                                {
                                    Name = "OrderCancelled.StoreOwnerNotification",
                                    Subject = "%Store.Name%. Customer cancelled an order",
                                    Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br /><br />Customer cancelled an order. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                    IsActive = true,
                                    EmailAccountId = emailAccount.Id,
                                },
                                new MessageTemplate
                                {
                                    Name = "VendorInformationChange.StoreOwnerNotification",
                                    Subject = "%Store.Name%. Vendor information change.",
                                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Vendor %Vendor.Name% (%Vendor.Email%) has just changed information about itself.{Environment.NewLine}</p>{Environment.NewLine}",
                                    IsActive = true,
                                    EmailAccountId = emailAccount.Id
                                },
                                new MessageTemplate
                                       {
                                           Name = "Vendor.VendorReview",
                                           Subject = "%Store.Name%. New vendor review.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new vendor review has been written.</p>",
                                           IsActive = true,
                                           EmailAccountId = emailAccount.Id,
                                },
                            };

            EngineContext.Current.Resolve<IRepository<MessageTemplate>>().Insert(messageTemplates);

            #endregion

            #region Upgrade products

            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;
            filter = filter & builder.Where(x => x.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock);
            filter = filter & builder.Where(x => x.StockQuantity < 0 || x.MinStockQuantity > 0);
            var productRepository = EngineContext.Current.Resolve<IRepository<Product>>();
            var products = productRepository.Collection.Find(filter).ToList();
            foreach (var product in products)
            {
                if(product.MinStockQuantity >= product.StockQuantity)
                {
                    product.LowStock = true;
                    var _filter = Builders<Product>.Filter.Eq("Id", product.Id);
                    var _update = Builders<Product>.Update
                            .Set(x => x.LowStock, true);
                    productRepository.Collection.UpdateOneAsync(_filter, _update);
                }
            }

            #endregion

            #region CustomerProductPrice

            var _customerProductPriceRepository = EngineContext.Current.Resolve<IRepository<CustomerProductPrice>>();
            _customerProductPriceRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProductPrice>((Builders<CustomerProductPrice>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "CustomerProduct", Unique = true }));


            #endregion

            #region Discount coupon code

            var _discountCouponRepository = EngineContext.Current.Resolve<IRepository<DiscountCoupon>>();
            _discountCouponRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountCoupon>((Builders<DiscountCoupon>.IndexKeys.Ascending(x => x.CouponCode)), new CreateIndexOptions() { Name = "CouponCode", Unique = true }));
            _discountCouponRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<DiscountCoupon>((Builders<DiscountCoupon>.IndexKeys.Ascending(x => x.DiscountId)), new CreateIndexOptions() { Name = "DiscountId", Unique = false }));

            #endregion

            #region Install new Topics

            var defaultTopicTemplate = EngineContext.Current.Resolve<IRepository<TopicTemplate>>().Table.FirstOrDefault(tt => tt.Name == "Default template");
            if(defaultTopicTemplate==null)
                defaultTopicTemplate = EngineContext.Current.Resolve<IRepository<TopicTemplate>>().Table.FirstOrDefault();

            var vendorTermsOfService = new Topic
            {
                SystemName = "VendorTermsOfService",
                IncludeInSitemap = false,
                IsPasswordProtected = false,
                DisplayOrder = 1,
                Title = "",
                Body = "<p>Put your terms of service information here. You can edit this in the admin site.</p>",
                TopicTemplateId = defaultTopicTemplate.Id
            };

            var topicService = EngineContext.Current.Resolve<ITopicService>();
            topicService.InsertTopic(vendorTermsOfService);

            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);

            #endregion

            #region CustomerActionType - add store condition

            var _customerActionType = EngineContext.Current.Resolve<IRepository<CustomerActionType>>();
            foreach (var item in _customerActionType.Table.ToList())
            {
                item.ConditionType.Add(13);
                _customerActionType.Update(item);
            }

            #endregion
        }

        private void From400To410()
        {
            #region Install String resources
            InstallStringResources("EN_400_410.nopres.xml");
            #endregion

            #region Install product reservation
            EngineContext.Current.Resolve<IRepository<ProductReservation>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReservation>((Builders<ProductReservation>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.Date)), new CreateIndexOptions() { Name = "ProductReservation", Unique = false }));
            #endregion

            #region Security settings
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var securitySettings = EngineContext.Current.Resolve<SecuritySettings>();
            securitySettings.AllowNonAsciiCharInHeaders = true;
            settingService.SaveSetting(securitySettings, x => x.AllowNonAsciiCharInHeaders, "", false);
            #endregion

            #region MessageTemplates

            var eaGeneral = EngineContext.Current.Resolve<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                        {
                            Name = "AuctionEnded.CustomerNotificationWin",
                            Subject = "%Store.Name%. Auction ended.",
                            Body = "<p>Hello, %Customer.FullName%!</p><p></p><p>At %Auctions.EndTime% you have won <a href=\"%Store.URL%%Auctions.ProductSeName%\">%Auctions.ProductName%</a> for %Auctions.Price%. Visit <a href=\"%Store.URL%/cart\">cart</a> to finish checkout process. </p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
                new MessageTemplate
                        {
                            Name = "AuctionEnded.CustomerNotificationLost",
                            Subject = "%Store.Name%. Auction ended.",
                            Body = "<p>Hello, %Customer.FullName%!</p><p></p><p>Unfortunately you did not win the bid %Auctions.ProductName%</p> <p>End price:  %Auctions.Price% </p> <p>End date auction %Auctions.EndTime% </p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
                new MessageTemplate
                        {
                            Name = "AuctionEnded.CustomerNotificationBin",
                            Subject = "%Store.Name%. Auction ended.",
                            Body = "<p>Hello, %Customer.FullName%!</p><p></p><p>Unfortunately you did not win the bid %Product.Name%</p> <p>Product was bought by option Buy it now for price: %Product.Price% </p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
                new MessageTemplate
                        {
                            Name = "AuctionEnded.StoreOwnerNotification",
                            Subject = "%Store.Name%. Auction ended.",
                            Body = "<p>At %Auctions.EndTime% %Customer.FullName% have won <a href=\"%Store.URL%%Auctions.ProductSeName%\">%Auctions.ProductName%</a> for %Auctions.Price%.</p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
                new MessageTemplate
                        {
                            Name = "BidUp.CustomerNotification",
                            Subject = "%Store.Name%. Your offer has been outbid.",
                            Body = "<p>Hi %Customer.FullName%!</p><p>Your offer for product <a href=\"%Auctions.ProductSeName%\">%Auctions.ProductName%</a> has been outbid. New price is %Auctions.Price%.<br />Raise a price by raising one's offer. Auction will be ended on %Auctions.EndTime%</p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
            };
            EngineContext.Current.Resolve<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion

            #region Tasks

            var keepliveTask = EngineContext.Current.Resolve<IRepository<ScheduleTask>>();

            var endtask = new ScheduleTask
            {
                ScheduleTaskName = "End of the auctions",
                Type = "Grand.Services.Tasks.EndAuctionsTask, Grand.Services",
                Enabled = false,
                StopOnError = false,
                TimeIntervalChoice = TimeIntervalChoice.EveryMinutes,
                TimeInterval = 10,
                MinuteOfHour = 1,
                HourOfDay = 1,
                DayOfWeek = DayOfWeek.Monday,
                MonthOptionChoice = MonthOptionChoice.OnSpecificDay,
                DayOfMonth = 1
            };
            keepliveTask.Insert(endtask);

            var _keepAliveScheduleTask = keepliveTask.Table.Where(x => x.Type == "Grand.Services.Tasks.KeepAliveScheduleTask").FirstOrDefault();
            if(_keepAliveScheduleTask !=null)
                keepliveTask.Delete(_keepAliveScheduleTask);

            #endregion

            #region Insert activities

            var _activityLogTypeRepository = EngineContext.Current.Resolve<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "PublicStore.AddNewBid",
                Enabled = false,
                Name = "Public store. Add new bid"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "DeleteBid",
                Enabled = false,
                Name = "Delete bid"
            });


            #endregion

            #region Index bid

            EngineContext.Current.Resolve<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CustomerId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductCustomer", Unique = false }));
            EngineContext.Current.Resolve<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductDate", Unique = false }));

            #endregion

        }

        private void From410To420()
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            
            #region Install String resources
            InstallStringResources("EN_410_420.nopres.xml");
            #endregion
            
            #region Update string resources

            var _localeStringResource = EngineContext.Current.Resolve<IRepository<LocaleStringResource>>();

            _localeStringResource.Collection.Find(new BsonDocument()).ForEachAsync((e) =>
            {
                e.ResourceName = e.ResourceName.ToLowerInvariant();
                _localeStringResource.Update(e);
            }).Wait();

            #endregion
            
            #region Admin area settings

            var adminareasettings = EngineContext.Current.Resolve<AdminAreaSettings>();
            adminareasettings.AdminLayout = "Default";
            adminareasettings.KendoLayout = "custom";
            _settingService.SaveSetting(adminareasettings);

            #endregion
            
            #region ActivityLog

            var _activityLogTypeRepository = EngineContext.Current.Resolve<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType()
            {
                SystemKeyword = "PublicStore.DeleteAccount",
                Enabled = false,
                Name = "Public store. Delete account"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "UpdateKnowledgebaseCategory",
                Enabled = true,
                Name = "Update knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "CreateKnowledgebaseCategory",
                Enabled = true,
                Name = "Create knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "DeleteKnowledgebaseCategory",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "CreateKnowledgebaseArticle",
                Enabled = true,
                Name = "Create knowledgebase article"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "UpdateKnowledgebaseArticle",
                Enabled = true,
                Name = "Update knowledgebase article"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "DeleteKnowledgebaseArticle",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "AddNewContactAttribute",
                Enabled = true,
                Name = "Add a new contact attribute"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "EditContactAttribute",
                Enabled = true,
                Name = "Edit a contact attribute"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType
            {
                SystemKeyword = "DeleteContactAttribute",
                Enabled = true,
                Name = "Delete a contact attribute"
            });

            #endregion
            
            #region MessageTemplates

            var emailAccount = EngineContext.Current.Resolve<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                {
                    Name = "CustomerDelete.StoreOwnerNotification",
                    Subject = "%Store.Name%. Customer has been deleted.",
                    Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> ,<br />%Customer.FullName% (%Customer.Email%) has just deleted from your database. </p>",
                    IsActive = true,
                    EmailAccountId = emailAccount.Id,
                },
            };
            EngineContext.Current.Resolve<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion
            
            #region Install new Topics
            var defaultTopicTemplate = EngineContext.Current.Resolve<IRepository<TopicTemplate>>().Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                defaultTopicTemplate = EngineContext.Current.Resolve<IRepository<TopicTemplate>>().Table.FirstOrDefault();

            var knowledgebaseHomepageTopic = new Topic
            {
                SystemName = "KnowledgebaseHomePage",
                IncludeInSitemap = false,
                IsPasswordProtected = false,
                DisplayOrder = 1,
                Title = "",
                Body = "<p>Knowledgebase homepage. You can edit this in the admin site.</p>",
                TopicTemplateId = defaultTopicTemplate.Id
            };

            var topicService = EngineContext.Current.Resolve<ITopicService>();
            topicService.InsertTopic(knowledgebaseHomepageTopic);
            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);

            #endregion

            #region Knowledge settings

            var knowledgesettings = EngineContext.Current.Resolve<KnowledgebaseSettings>();
            knowledgesettings.Enabled = false;
            _settingService.SaveSetting(knowledgesettings);

            #endregion

            #region Push notifications settings

            var pushNotificationSettings = EngineContext.Current.Resolve<PushNotificationsSettings>();
            pushNotificationSettings.Enabled = false;
            pushNotificationSettings.AllowGuestNotifications = true;
            _settingService.SaveSetting(pushNotificationSettings);

            #endregion

            #region Knowledge table

            EngineContext.Current.Resolve<IRepository<KnowledgebaseArticle>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseArticle>((Builders<KnowledgebaseArticle>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));
            EngineContext.Current.Resolve<IRepository<KnowledgebaseCategory>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseCategory>((Builders<KnowledgebaseCategory>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));

            #endregion

            #region Update gift card

            IRepository<GiftCard> _giftCardRepository = EngineContext.Current.Resolve<IRepository<GiftCard>>();
            foreach (var gift in _giftCardRepository.Table)
            {
                gift.GiftCardCouponCode = gift.GiftCardCouponCode.ToLowerInvariant();
                _giftCardRepository.Update(gift);
            }

            #endregion

        }
        private void InstallStringResources(string filenames)
        {
            //'English' language            
            var language = EngineContext.Current.Resolve<IRepository<Language>>().Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/Upgrade"), "*" + filenames , SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXmlInstall(language, localesXml);
            }
        }
    }
}