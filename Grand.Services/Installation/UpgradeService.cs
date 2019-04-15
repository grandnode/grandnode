using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.AdminSearch;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.PushNotifications;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Tasks;
using Grand.Core.Domain.Topics;
using Grand.Data;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Topics;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Grand.Services.Installation
{
    public partial class UpgradeService : IUpgradeService
    {
        #region Fields
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<GrandNodeVersion> _versionRepository;
        private readonly IWebHelper _webHelper;

        private const string version_400 = "4.00";
        private const string version_410 = "4.10";
        private const string version_420 = "4.20";
        private const string version_430 = "4.30";
        private const string version_440 = "4.40";
        private const string version_450 = "4.50";

        #endregion

        #region Ctor
        public UpgradeService(IServiceProvider serviceProvider, IRepository<GrandNodeVersion> versionRepository, IWebHelper webHelper)
        {
            this._serviceProvider = serviceProvider;
            this._versionRepository = versionRepository;
            this._webHelper = webHelper;
        }
        #endregion

        public virtual string DatabaseVersion()
        {
            string version = version_400;
            var databaseversion = _versionRepository.Table.FirstOrDefault();
            if (databaseversion != null)
                version = databaseversion.DataBaseVersion;
            return version;
        }
        public virtual void UpgradeData(string fromversion, string toversion)
        {
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
            if (fromversion == version_420)
            {
                From420To430();
                fromversion = version_430;
            }
            if (fromversion == version_430)
            {
                From430To440();
                fromversion = version_440;
            }
            if (fromversion == version_440)
            {
                From440To450();
                fromversion = version_450;
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

        private void From400To410()
        {
            #region Install String resources
            InstallStringResources("EN_400_410.nopres.xml");
            #endregion

            #region Install product reservation
            _serviceProvider.GetRequiredService<IRepository<ProductReservation>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReservation>((Builders<ProductReservation>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.Date)), new CreateIndexOptions() { Name = "ProductReservation", Unique = false }));
            #endregion

            #region Security settings
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var securitySettings = _serviceProvider.GetRequiredService<SecuritySettings>();
            securitySettings.AllowNonAsciiCharInHeaders = true;
            settingService.SaveSetting(securitySettings, x => x.AllowNonAsciiCharInHeaders, "", false);
            #endregion

            #region MessageTemplates

            var eaGeneral = _serviceProvider.GetRequiredService<IRepository<EmailAccount>>().Table.FirstOrDefault();
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
                            Body = "<p>Hi %Customer.FullName%!</p><p>Your offer for product <a href=\"%Auctions.ProductSeName%\">%Auctions.ProductName%</a> has been outbid. Your price was %Auctions.Price%.<br />Raise a price by raising one's offer. Auction will be ended on %Auctions.EndTime%</p>",
                            IsActive = true,
                            EmailAccountId = eaGeneral.Id,
                        },
            };
            _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion

            #region Tasks

            var keepliveTask = _serviceProvider.GetRequiredService<IRepository<ScheduleTask>>();

            var endtask = new ScheduleTask {
                ScheduleTaskName = "End of the auctions",
                Type = "Grand.Services.Tasks.EndAuctionsTask, Grand.Services",
                Enabled = false,
                StopOnError = false,
                TimeInterval = 1440
            };
            keepliveTask.Insert(endtask);

            var _keepAliveScheduleTask = keepliveTask.Table.Where(x => x.Type == "Grand.Services.Tasks.KeepAliveScheduleTask").FirstOrDefault();
            if (_keepAliveScheduleTask != null)
                keepliveTask.Delete(_keepAliveScheduleTask);

            #endregion

            #region Insert activities

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType() {
                SystemKeyword = "PublicStore.AddNewBid",
                Enabled = false,
                Name = "Public store. Add new bid"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType() {
                SystemKeyword = "DeleteBid",
                Enabled = false,
                Name = "Delete bid"
            });


            #endregion

            #region Index bid

            _serviceProvider.GetRequiredService<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CustomerId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductCustomer", Unique = false }));
            _serviceProvider.GetRequiredService<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductDate", Unique = false }));

            #endregion

        }

        private void From410To420()
        {
            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();

            #region Install String resources
            InstallStringResources("EN_410_420.nopres.xml");
            #endregion

            #region Update string resources

            var _localeStringResource = _serviceProvider.GetRequiredService<IRepository<LocaleStringResource>>();

            _localeStringResource.Collection.Find(new BsonDocument()).ForEachAsync((e) =>
            {
                e.ResourceName = e.ResourceName.ToLowerInvariant();
                _localeStringResource.Update(e);
            }).Wait();

            #endregion

            #region Admin area settings

            var adminareasettings = _serviceProvider.GetRequiredService<AdminAreaSettings>();
            adminareasettings.AdminLayout = "Default";
            adminareasettings.KendoLayout = "custom";
            _settingService.SaveSetting(adminareasettings);

            #endregion

            #region ActivityLog

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType() {
                SystemKeyword = "PublicStore.DeleteAccount",
                Enabled = false,
                Name = "Public store. Delete account"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "UpdateKnowledgebaseCategory",
                Enabled = true,
                Name = "Update knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "CreateKnowledgebaseCategory",
                Enabled = true,
                Name = "Create knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "DeleteKnowledgebaseCategory",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "CreateKnowledgebaseArticle",
                Enabled = true,
                Name = "Create knowledgebase article"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "UpdateKnowledgebaseArticle",
                Enabled = true,
                Name = "Update knowledgebase article"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "DeleteKnowledgebaseArticle",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "AddNewContactAttribute",
                Enabled = true,
                Name = "Add a new contact attribute"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "EditContactAttribute",
                Enabled = true,
                Name = "Edit a contact attribute"
            });
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "DeleteContactAttribute",
                Enabled = true,
                Name = "Delete a contact attribute"
            });

            #endregion

            #region MessageTemplates

            var emailAccount = _serviceProvider.GetRequiredService<IRepository<EmailAccount>>().Table.FirstOrDefault();
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
            _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion

            #region Install new Topics
            var defaultTopicTemplate = _serviceProvider.GetRequiredService<IRepository<TopicTemplate>>().Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                defaultTopicTemplate = _serviceProvider.GetRequiredService<IRepository<TopicTemplate>>().Table.FirstOrDefault();

            var knowledgebaseHomepageTopic = new Topic {
                SystemName = "KnowledgebaseHomePage",
                IncludeInSitemap = false,
                IsPasswordProtected = false,
                DisplayOrder = 1,
                Title = "",
                Body = "<p>Knowledgebase homepage. You can edit this in the admin site.</p>",
                TopicTemplateId = defaultTopicTemplate.Id
            };

            var topicService = _serviceProvider.GetRequiredService<ITopicService>();
            topicService.InsertTopic(knowledgebaseHomepageTopic);
            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            _serviceProvider.GetRequiredService<IPermissionService>().InstallPermissions(provider);

            #endregion

            #region Knowledge settings

            var knowledgesettings = _serviceProvider.GetRequiredService<KnowledgebaseSettings>();
            knowledgesettings.Enabled = false;
            knowledgesettings.AllowNotRegisteredUsersToLeaveComments = true;
            knowledgesettings.NotifyAboutNewArticleComments = false;
            _settingService.SaveSetting(knowledgesettings);

            #endregion

            #region Push notifications settings

            var pushNotificationSettings = _serviceProvider.GetRequiredService<PushNotificationsSettings>();
            pushNotificationSettings.Enabled = false;
            pushNotificationSettings.AllowGuestNotifications = true;
            _settingService.SaveSetting(pushNotificationSettings);

            #endregion

            #region Knowledge table

            _serviceProvider.GetRequiredService<IRepository<KnowledgebaseArticle>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseArticle>((Builders<KnowledgebaseArticle>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));
            _serviceProvider.GetRequiredService<IRepository<KnowledgebaseCategory>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseCategory>((Builders<KnowledgebaseCategory>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));

            #endregion

            #region Update gift card

            IRepository<GiftCard> _giftCardRepository = _serviceProvider.GetRequiredService<IRepository<GiftCard>>();
            foreach (var gift in _giftCardRepository.Table)
            {
                gift.GiftCardCouponCode = gift.GiftCardCouponCode.ToLowerInvariant();
                _giftCardRepository.Update(gift);
            }

            #endregion

        }

        private void From420To430()
        {
            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();

            InstallStringResources("EN_420_430.nopres.xml");

            #region Settings

            var adminSearchSettings = _serviceProvider.GetRequiredService<AdminSearchSettings>();
            adminSearchSettings.BlogsDisplayOrder = 0;
            adminSearchSettings.CategoriesDisplayOrder = 0;
            adminSearchSettings.CustomersDisplayOrder = 0;
            adminSearchSettings.ManufacturersDisplayOrder = 0;
            adminSearchSettings.MaxSearchResultsCount = 10;
            adminSearchSettings.MinSearchTermLength = 3;
            adminSearchSettings.NewsDisplayOrder = 0;
            adminSearchSettings.OrdersDisplayOrder = 0;
            adminSearchSettings.ProductsDisplayOrder = 0;
            adminSearchSettings.SearchInBlogs = true;
            adminSearchSettings.SearchInCategories = true;
            adminSearchSettings.SearchInCustomers = true;
            adminSearchSettings.SearchInManufacturers = true;
            adminSearchSettings.SearchInNews = true;
            adminSearchSettings.SearchInOrders = true;
            adminSearchSettings.SearchInProducts = true;
            adminSearchSettings.SearchInTopics = true;
            adminSearchSettings.TopicsDisplayOrder = 0;
            adminSearchSettings.SearchInMenu = true;
            adminSearchSettings.MenuDisplayOrder = -1;
            _settingService.SaveSetting(adminSearchSettings);

            var customerSettings = _serviceProvider.GetRequiredService<CustomerSettings>();
            customerSettings.HideNotesTab = true;
            _settingService.SaveSetting(customerSettings);

            #endregion

            #region Emails

            var emailAccount = _serviceProvider.GetRequiredService<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                {
                    Name = "Knowledgebase.ArticleComment",
                    Subject = "%Store.Name%. New article comment.",
                    Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new article comment has been created for article \"%Article.ArticleTitle%\".</p>",
                    IsActive = true,
                    EmailAccountId = emailAccount.Id,
                },
                new MessageTemplate
                {
                    Name = "Customer.NewCustomerNote",
                    Subject = "New customer note has been added",
                    Body = "<p><br />Hello %Customer.FullName%, <br />New customer note has been added to your account:<br />\"%Customer.NewTitleText%\".<br /></p>",
                    IsActive = true,
                    EmailAccountId = emailAccount.Id,
                },
            };
            _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().Insert(messageTemplates);

            #endregion

            #region Activity log
            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            _activityLogTypeRepository.Insert(new ActivityLogType {
                SystemKeyword = "PublicStore.AddArticleComment",
                Enabled = false,
                Name = "Public store. Add article comment"
            });
            #endregion

            #region Knowledgebase settings

            var knowledgebaseSettings = _serviceProvider.GetRequiredService<KnowledgebaseSettings>();
            knowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments = true;
            knowledgebaseSettings.NotifyAboutNewArticleComments = false;
            _settingService.SaveSetting(knowledgebaseSettings);
            #endregion

            #region Customer Personalize Product

            var _customerProductRepository = _serviceProvider.GetRequiredService<IRepository<CustomerProduct>>();
            _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "CustomerProduct", Unique = false }));
            _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "CustomerProduct_Unique", Unique = true }));

            #endregion

            #region Update customer

            var dbContext = _serviceProvider.GetRequiredService<IMongoDatabase>();
            var customerRepository = _serviceProvider.GetRequiredService<IRepository<Customer>>();

            var builderCustomer = Builders<Customer>.Filter;
            var filterCustomer = builderCustomer.Eq("IsNewsItem", true) | builderCustomer.Eq("IsHasOrders", true) | builderCustomer.Eq("IsHasBlogComments", true) | builderCustomer.Eq("IsHasArticleComments", true)
                 | builderCustomer.Eq("IsHasProductReview", true) | builderCustomer.Eq("IsHasProductReviewH", true) | builderCustomer.Eq("IsHasVendorReview", true)
                 | builderCustomer.Eq("IsHasVendorReviewH", true) | builderCustomer.Eq("IsHasPoolVoting", true) | builderCustomer.Eq("IsHasForumPost", true) | builderCustomer.Eq("IsHasForumTopic", true);

            var updateCustomer = Builders<Customer>.Update
               .Set(x => x.HasContributions, true);

            var resultUpdate = customerRepository.Collection.UpdateOneAsync(filterCustomer, updateCustomer).Result;

            var removeFields = Builders<object>.Update
               .Unset("IsNewsItem")
               .Unset("IsHasOrders")
               .Unset("IsHasBlogComments")
               .Unset("IsHasArticleComments")
               .Unset("IsHasProductReview")
               .Unset("IsHasProductReviewH")
               .Unset("IsHasVendorReview")
               .Unset("IsHasVendorReviewH")
               .Unset("IsHasPoolVoting")
               .Unset("IsHasForumPost")
               .Unset("IsHasForumTopic")
               .Unset("HasShoppingCartItems");

            var resultRemove = dbContext.GetCollection<object>(typeof(Customer).Name).UpdateMany(new BsonDocument(), removeFields);


            #endregion

            #region Update Return Request to the newest version

            var dBContext = _serviceProvider.GetRequiredService<IMongoDBContext>();
            var orderCollection = _serviceProvider.GetRequiredService<IRepository<Order>>();
            var newReturnRequestCollection = _serviceProvider.GetRequiredService<IRepository<ReturnRequest>>();
            var oldreturRequestCollection = dBContext.Database().GetCollection<OldReturnRequest>("ReturnRequest");

            foreach (var oldrr in oldreturRequestCollection.AsQueryable().ToList())
            {
                //prepare object
                var newrr = new ReturnRequest();
                newrr.ReturnNumber = oldrr.ReturnNumber;
                newrr.OrderId = oldrr.OrderId;
                newrr.ReturnRequestStatusId = oldrr.ReturnRequestStatusId;
                newrr.StaffNotes = oldrr.StaffNotes;
                newrr.UpdatedOnUtc = oldrr.UpdatedOnUtc;
                newrr.CreatedOnUtc = oldrr.CreatedOnUtc;
                newrr.CustomerComments = oldrr.CustomerComments;
                newrr.CustomerId = oldrr.CustomerId;
                var order = orderCollection.GetById(oldrr.OrderId);
                if (order != null)
                    newrr.OrderId = order.StoreId;
                var rrItem = new ReturnRequestItem();
                rrItem.OrderItemId = oldrr.OrderItemId;
                rrItem.Quantity = oldrr.Quantity;
                rrItem.ReasonForReturn = oldrr.ReasonForReturn;
                rrItem.RequestedAction = oldrr.RequestedAction;
                newrr.ReturnRequestItems.Add(rrItem);
                //remove old document
                var rrbuilder = Builders<OldReturnRequest>.Filter;
                var rrfilter = FilterDefinition<OldReturnRequest>.Empty;
                rrfilter = rrfilter & rrbuilder.Where(x => x.Id == oldrr.Id);
                var oldresult = oldreturRequestCollection.DeleteOne(rrfilter);
                //insert new document
                var newresult = newReturnRequestCollection.Insert(newrr);
            }
            #endregion

            #region Customer note

            //customer note
            _serviceProvider.GetRequiredService<IRepository<CustomerNote>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerNote>((Builders<CustomerNote>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId", Unique = false, Background = true }));

            #endregion

            #region Category index

            var _categoryRepository = _serviceProvider.GetRequiredService<IRepository<Category>>();
            _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.FeaturedProductsOnHomaPage).Ascending(x => x.Published).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "FeaturedProductsOnHomaPage_DisplayOrder_1", Unique = false }));

            #endregion
        }

        private void From430To440()
        {
            #region Install String resources
            InstallStringResources("430_440.nopres.xml");
            #endregion

            #region Permisions
            IPermissionProvider provider = new StandardPermissionProvider();
            _serviceProvider.GetRequiredService<IPermissionService>().InstallPermissions(provider);
            #endregion
            #region Update tags on the products

            var productTagService = _serviceProvider.GetRequiredService<IProductTagService>();
            var productRepository = _serviceProvider.GetRequiredService<IRepository<Product>>();

            foreach (var tag in productTagService.GetAllProductTags().Result)
            {
                var builder = Builders<Product>.Filter;
                var filter = new BsonDocument();
                filter.Add(new BsonElement("ProductTags", tag.Id));
                var update = Builders<Product>.Update
                    .Set(x => x.ProductTags.ElementAt(-1), tag.Name);
                var result = productRepository.Collection.UpdateMany(filter, update);

                tag.SeName = SeoExtensions.GetSeName(tag.Name, false, false);
                productTagService.UpdateProductTag(tag);
            }

            #endregion

            #region User api

            _serviceProvider.GetRequiredService<IRepository<UserApi>>().Collection.Indexes.CreateOne(new CreateIndexModel<UserApi>((Builders<UserApi>.IndexKeys.Ascending(x => x.Email)), new CreateIndexOptions() { Name = "Email", Unique = true, Background = true }));

            #endregion

            #region Update message templates (tokens)

            string ReplaceValue(string value)
            {
                string Evaluator(Match match)
                {
                    return $"{{{{{match.Value.Replace("%", "")}}}}}";
                }
                MatchEvaluator evaluator = new MatchEvaluator(Evaluator);
                return Regex.Replace(value, @"%([A-Za-z0-9_.]*?)%", new MatchEvaluator(Evaluator));
            }

            string orderProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Order.Products.txt"));
            string shipmentProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Shipment.Products.txt"));

            var messagetemplateService = _serviceProvider.GetRequiredService<Grand.Services.Messages.IMessageTemplateService>();
            var messagetemplates = messagetemplateService.GetAllMessageTemplates(string.Empty).Result;
            foreach (var messagetemplate in messagetemplates)
            {
                messagetemplate.Subject = ReplaceValue(messagetemplate.Subject);
                if (messagetemplate.Body.Contains("%Order.Product(s)%"))
                {
                    messagetemplate.Body = messagetemplate.Body.Replace("%Order.Product(s)%", orderProducts);
                }
                if (messagetemplate.Body.Contains("%Shipment.Product(s)%"))
                {
                    messagetemplate.Body = messagetemplate.Body.Replace("%Shipment.Product(s)%", shipmentProducts);
                }
                messagetemplate.Body = ReplaceValue(messagetemplate.Body);
                messagetemplateService.UpdateMessageTemplate(messagetemplate);
            }
            #endregion

            #region Insert message template

            var eaGeneral = _serviceProvider.GetRequiredService<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                {
                    Name = "AuctionExpired.StoreOwnerNotification",
                    Subject = "Your auction to product {{Product.Name}}  has expired.",
                    Body = "Hello, <br> Your auction to product {{Product.Name}} has expired without bid.",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                }
            };
            _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().Insert(messageTemplates);
            #endregion

        }

        private void From440To450()
        {
            #region Install String resources
            InstallStringResources("EN_440_450.nopres.xml");
            #endregion

            #region Update task
            var tasks = _serviceProvider.GetRequiredService<IRepository<ScheduleTask>>();
            foreach (var task in tasks.Table)
            {
                if (task.TimeInterval == 0)
                {
                    task.TimeInterval = 1440;
                    tasks.Update(task);
                }
                if(task.Type == "Grand.Services.Tasks.ClearLogScheduleTask")
                {
                    task.Type = "Grand.Services.Tasks.ClearLogScheduleTask, Grand.Services";
                    tasks.Update(task);
                }
            }
            #endregion
        }

        private void InstallStringResources(string filenames)
        {
            //'English' language            
            var language = _serviceProvider.GetRequiredService<IRepository<Language>>().Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/Upgrade"), "*" + filenames, SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
                localizationService.ImportResourcesFromXmlInstall(language, localesXml);
            }
        }

        /// <summary>
        /// Used to convert from 4.20 to 4.30
        /// </summary>
        class OldReturnRequest : BaseEntity
        {
            public int ReturnNumber { get; set; }
            public string StoreId { get; set; }
            public string OrderId { get; set; }
            public string OrderItemId { get; set; }
            public string CustomerId { get; set; }
            public int Quantity { get; set; }
            public string ReasonForReturn { get; set; }
            public string RequestedAction { get; set; }
            public string CustomerComments { get; set; }
            public string StaffNotes { get; set; }
            public int ReturnRequestStatusId { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public DateTime UpdatedOnUtc { get; set; }
        }
    }
}