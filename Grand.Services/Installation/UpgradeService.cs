using Grand.Core;
using Grand.Domain;
using Grand.Domain.Admin;
using Grand.Domain.AdminSearch;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Logging;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Domain.PushNotifications;
using Grand.Domain.Security;
using Grand.Domain.Seo;
using Grand.Domain.Shipping;
using Grand.Domain.Tasks;
using Grand.Domain.Topics;
using Grand.Services.Admin;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Security;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

#pragma warning disable CS0618

namespace Grand.Services.Installation
{
    public partial class UpgradeService : IUpgradeService
    {
        #region Fields
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly IRepository<GrandNodeVersion> _versionRepository;

        private const string version_400 = "4.00";
        private const string version_410 = "4.10";
        private const string version_420 = "4.20";
        private const string version_430 = "4.30";
        private const string version_440 = "4.40";
        private const string version_450 = "4.50";
        private const string version_460 = "4.60";
        private const string version_470 = "4.70";
        private const string version_480 = "4.80";
        private const string version_490 = "4.90";

        #endregion

        #region Ctor
        public UpgradeService(IServiceProvider serviceProvider,
            IMediator mediator,
            IRepository<GrandNodeVersion> versionRepository)
        {
            _serviceProvider = serviceProvider;
            _mediator = mediator;
            _versionRepository = versionRepository;
        }
        #endregion

        public virtual string DatabaseVersion()
        {
            var version = version_400;
            var databaseversion = _versionRepository.Table.FirstOrDefault();
            if (databaseversion != null)
                version = databaseversion.DataBaseVersion;
            return version;
        }
        public virtual async Task UpgradeData(string fromversion, string toversion)
        {
            if (fromversion == version_400)
            {
                await From400To410();
                fromversion = version_410;
            }
            if (fromversion == version_410)
            {
                await From410To420();
                fromversion = version_420;
            }
            if (fromversion == version_420)
            {
                await From420To430();
                fromversion = version_430;
            }
            if (fromversion == version_430)
            {
                await From430To440();
                fromversion = version_440;
            }
            if (fromversion == version_440)
            {
                await From440To450();
                fromversion = version_450;
            }
            if (fromversion == version_450)
            {
                await From450To460();
                fromversion = version_460;
            }
            if (fromversion == version_460)
            {
                await From460To470();
                fromversion = version_470;
            }
            if (fromversion == version_470)
            {
                await From470To480();
                fromversion = version_480;
            }
            if (fromversion == version_480)
            {
                await From480To490();
                fromversion = version_490;
            }
            if (fromversion == toversion)
            {
                var databaseversion = _versionRepository.Table.FirstOrDefault();
                if (databaseversion != null)
                {
                    databaseversion.DataBaseVersion = GrandVersion.SupportedDBVersion;
                    await _versionRepository.UpdateAsync(databaseversion);
                }
                else
                {
                    databaseversion = new GrandNodeVersion {
                        DataBaseVersion = GrandVersion.SupportedDBVersion
                    };
                    await _versionRepository.InsertAsync(databaseversion);
                }
            }
        }

        private async Task From400To410()
        {
            #region Install String resources
            await InstallStringResources("EN_400_410.xml");
            #endregion

            #region Install product reservation
            await _serviceProvider.GetRequiredService<IRepository<ProductReservation>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<ProductReservation>((Builders<ProductReservation>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.Date)), new CreateIndexOptions() { Name = "ProductReservation", Unique = false }));
            #endregion

            #region Security settings
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var securitySettings = _serviceProvider.GetRequiredService<SecuritySettings>();
            securitySettings.AllowNonAsciiCharInHeaders = true;
            await settingService.SaveSetting(securitySettings, x => x.AllowNonAsciiCharInHeaders, "", false);
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
            await _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().InsertAsync(messageTemplates);

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
            await keepliveTask.InsertAsync(endtask);

            var _keepAliveScheduleTask = keepliveTask.Table.Where(x => x.Type == "Grand.Services.Tasks.KeepAliveScheduleTask").FirstOrDefault();
            if (_keepAliveScheduleTask != null)
                keepliveTask.Delete(_keepAliveScheduleTask);

            #endregion

            #region Insert activities

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                SystemKeyword = "PublicStore.AddNewBid",
                Enabled = false,
                Name = "Public store. Add new bid"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                SystemKeyword = "DeleteBid",
                Enabled = false,
                Name = "Delete bid"
            });


            #endregion

            #region Index bid

            await _serviceProvider.GetRequiredService<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Ascending(x => x.CustomerId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductCustomer", Unique = false }));
            await _serviceProvider.GetRequiredService<IRepository<Bid>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<Bid>((Builders<Bid>.IndexKeys.Ascending(x => x.ProductId).Descending(x => x.Date)), new CreateIndexOptions() { Name = "ProductDate", Unique = false }));

            #endregion

        }

        private async Task From410To420()
        {
            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();

            #region Install String resources
            await InstallStringResources("EN_410_420.xml");
            #endregion

            #region Update string resources

            var _localeStringResource = _serviceProvider.GetRequiredService<IRepository<LocaleStringResource>>();

            await _localeStringResource.Collection.Find(new BsonDocument()).ForEachAsync(async (e) =>
            {
                e.ResourceName = e.ResourceName.ToLowerInvariant();
                await _localeStringResource.UpdateAsync(e);
            });

            #endregion

            #region ActivityLog

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                SystemKeyword = "PublicStore.DeleteAccount",
                Enabled = false,
                Name = "Public store. Delete account"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "UpdateKnowledgebaseCategory",
                Enabled = true,
                Name = "Update knowledgebase category"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "CreateKnowledgebaseCategory",
                Enabled = true,
                Name = "Create knowledgebase category"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "DeleteKnowledgebaseCategory",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "CreateKnowledgebaseArticle",
                Enabled = true,
                Name = "Create knowledgebase article"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "UpdateKnowledgebaseArticle",
                Enabled = true,
                Name = "Update knowledgebase article"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "DeleteKnowledgebaseArticle",
                Enabled = true,
                Name = "Delete knowledgebase category"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "AddNewContactAttribute",
                Enabled = true,
                Name = "Add a new contact attribute"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "EditContactAttribute",
                Enabled = true,
                Name = "Edit a contact attribute"
            });
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
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
            await _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().InsertAsync(messageTemplates);
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
            await topicService.InsertTopic(knowledgebaseHomepageTopic);
            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            await _mediator.Send(new InstallPermissionsCommand() { PermissionProvider = provider });

            #endregion

            #region Knowledge settings

            var knowledgesettings = _serviceProvider.GetRequiredService<KnowledgebaseSettings>();
            knowledgesettings.Enabled = false;
            knowledgesettings.AllowNotRegisteredUsersToLeaveComments = true;
            knowledgesettings.NotifyAboutNewArticleComments = false;
            await _settingService.SaveSetting(knowledgesettings);

            #endregion

            #region Push notifications settings

            var pushNotificationSettings = _serviceProvider.GetRequiredService<PushNotificationsSettings>();
            pushNotificationSettings.Enabled = false;
            pushNotificationSettings.AllowGuestNotifications = true;
            await _settingService.SaveSetting(pushNotificationSettings);

            #endregion

            #region Knowledge table

            await _serviceProvider.GetRequiredService<IRepository<KnowledgebaseArticle>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseArticle>((Builders<KnowledgebaseArticle>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));
            await _serviceProvider.GetRequiredService<IRepository<KnowledgebaseCategory>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<KnowledgebaseCategory>((Builders<KnowledgebaseCategory>.IndexKeys.Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "DisplayOrder", Unique = false }));

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

        private async Task From420To430()
        {
            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();

            await InstallStringResources("EN_420_430.xml");

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
            await _settingService.SaveSetting(adminSearchSettings);

            var customerSettings = _serviceProvider.GetRequiredService<CustomerSettings>();
            customerSettings.HideNotesTab = true;
            await _settingService.SaveSetting(customerSettings);

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
            await _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().InsertAsync(messageTemplates);

            #endregion

            #region Activity log
            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            await _activityLogTypeRepository.InsertAsync(new ActivityLogType {
                SystemKeyword = "PublicStore.AddArticleComment",
                Enabled = false,
                Name = "Public store. Add article comment"
            });
            #endregion

            #region Knowledgebase settings

            var knowledgebaseSettings = _serviceProvider.GetRequiredService<KnowledgebaseSettings>();
            knowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments = true;
            knowledgebaseSettings.NotifyAboutNewArticleComments = false;
            await _settingService.SaveSetting(knowledgebaseSettings);
            #endregion

            #region Customer Personalize Product

            var _customerProductRepository = _serviceProvider.GetRequiredService<IRepository<CustomerProduct>>();
            await _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "CustomerProduct", Unique = false }));
            await _customerProductRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerProduct>((Builders<CustomerProduct>.IndexKeys.Ascending(x => x.CustomerId).Ascending(x => x.ProductId)), new CreateIndexOptions() { Name = "CustomerProduct_Unique", Unique = true }));

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

            await customerRepository.Collection.UpdateOneAsync(filterCustomer, updateCustomer);

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

            await dbContext.GetCollection<object>(typeof(Customer).Name).UpdateManyAsync(new BsonDocument(), removeFields);


            #endregion

            #region Update Return Request to the newest version

            var dBContext = _serviceProvider.GetRequiredService<IMongoDBContext>();
            var orderCollection = _serviceProvider.GetRequiredService<IRepository<Order>>();
            var newReturnRequestCollection = _serviceProvider.GetRequiredService<IRepository<ReturnRequest>>();
            var oldreturRequestCollection = dBContext.Database().GetCollection<OldReturnRequest>("ReturnRequest");

            foreach (var oldrr in oldreturRequestCollection.AsQueryable().ToList())
            {
                //prepare object
                var newrr = new ReturnRequest {
                    ReturnNumber = oldrr.ReturnNumber,
                    OrderId = oldrr.OrderId,
                    ReturnRequestStatusId = oldrr.ReturnRequestStatusId,
                    StaffNotes = oldrr.StaffNotes,
                    UpdatedOnUtc = oldrr.UpdatedOnUtc,
                    CreatedOnUtc = oldrr.CreatedOnUtc,
                    CustomerComments = oldrr.CustomerComments,
                    CustomerId = oldrr.CustomerId
                };
                var order = orderCollection.GetById(oldrr.OrderId);
                if (order != null)
                    newrr.OrderId = order.StoreId;
                var rrItem = new ReturnRequestItem {
                    OrderItemId = oldrr.OrderItemId,
                    Quantity = oldrr.Quantity,
                    ReasonForReturn = oldrr.ReasonForReturn,
                    RequestedAction = oldrr.RequestedAction
                };
                newrr.ReturnRequestItems.Add(rrItem);
                //remove old document
                var rrbuilder = Builders<OldReturnRequest>.Filter;
                var rrfilter = FilterDefinition<OldReturnRequest>.Empty;
                rrfilter = rrfilter & rrbuilder.Where(x => x.Id == oldrr.Id);
                await oldreturRequestCollection.DeleteOneAsync(rrfilter);
                //insert new document
                await newReturnRequestCollection.InsertAsync(newrr);
            }
            #endregion

            #region Customer note

            //customer note
            await _serviceProvider.GetRequiredService<IRepository<CustomerNote>>().Collection.Indexes.CreateOneAsync(new CreateIndexModel<CustomerNote>((Builders<CustomerNote>.IndexKeys.Ascending(x => x.CustomerId).Descending(x => x.CreatedOnUtc)), new CreateIndexOptions() { Name = "CustomerId", Unique = false, Background = true }));

            #endregion

            #region Category index

            var _categoryRepository = _serviceProvider.GetRequiredService<IRepository<Category>>();
            await _categoryRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<Category>((Builders<Category>.IndexKeys.Ascending(x => x.FeaturedProductsOnHomaPage).Ascending(x => x.Published).Ascending(x => x.DisplayOrder)), new CreateIndexOptions() { Name = "FeaturedProductsOnHomaPage_DisplayOrder_1", Unique = false }));

            #endregion
        }

        private async Task From430To440()
        {
            #region Install String resources
            await InstallStringResources("430_440.xml");
            #endregion

            #region Permisions
            IPermissionProvider provider = new StandardPermissionProvider();
            await _mediator.Send(new InstallPermissionsCommand() { PermissionProvider = provider });
            #endregion

            #region Update tags on the products

            var productTagService = _serviceProvider.GetRequiredService<IProductTagService>();
            var productRepository = _serviceProvider.GetRequiredService<IRepository<Product>>();

            foreach (var tag in await productTagService.GetAllProductTags())
            {
                var builder = Builders<Product>.Filter;
                var filter = new BsonDocument {
                    new BsonElement("ProductTags", tag.Id)
                };
                var update = Builders<Product>.Update
                    .Set(x => x.ProductTags.ElementAt(-1), tag.Name);

                await productRepository.Collection.UpdateManyAsync(filter, update);

                tag.SeName = SeoExtensions.GenerateSlug(tag.Name, false, false);
                await productTagService.UpdateProductTag(tag);
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
                var evaluator = new MatchEvaluator(Evaluator);
                return Regex.Replace(value, @"%([A-Za-z0-9_.]*?)%", new MatchEvaluator(Evaluator));
            }

            var orderProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Order.Products.txt"));
            var shipmentProducts = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Upgrade/Shipment.Products.txt"));

            var messagetemplateService = _serviceProvider.GetRequiredService<Grand.Services.Messages.IMessageTemplateService>();
            var messagetemplates = await messagetemplateService.GetAllMessageTemplates(string.Empty);
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
                await messagetemplateService.UpdateMessageTemplate(messagetemplate);
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
            await _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().InsertAsync(messageTemplates);
            #endregion

        }

        private async Task From440To450()
        {
            #region Install String resources
            await InstallStringResources("EN_440_450.xml");
            #endregion

            #region Update task
            var tasks = _serviceProvider.GetRequiredService<IRepository<ScheduleTask>>();
            foreach (var task in tasks.Table)
            {
                if (task.TimeInterval == 0)
                {
                    task.TimeInterval = 1440;
                    await tasks.UpdateAsync(task);
                }
                if (task.Type == "Grand.Services.Tasks.ClearLogScheduleTask")
                {
                    task.Type = "Grand.Services.Tasks.ClearLogScheduleTask, Grand.Services";
                    await tasks.UpdateAsync(task);
                }
            }
            #endregion

            #region Update shipments - storeId

            var shipments = _serviceProvider.GetRequiredService<IRepository<Shipment>>();
            var orders = _serviceProvider.GetRequiredService<IRepository<Order>>();
            foreach (var shipment in shipments.Table)
            {
                var order = orders.Table.Where(x => x.Id == shipment.OrderId).FirstOrDefault();
                if (order != null)
                {
                    shipment.StoreId = order.StoreId;
                    await shipments.UpdateAsync(shipment);
                }
            }
            #endregion

            #region Update topics - rename fields

            var renameFields = Builders<object>.Update
                .Rename("IncludeInFooterColumn1", "IncludeInFooterRow1")
                .Rename("IncludeInFooterColumn2", "IncludeInFooterRow2")
                .Rename("IncludeInFooterColumn3", "IncludeInFooterRow3");

            var dbContext = _serviceProvider.GetRequiredService<IMongoDatabase>();
            await dbContext.GetCollection<object>(typeof(Topic).Name).UpdateManyAsync(new BsonDocument(), renameFields);

            #endregion

            #region Update order - primary currency code

            var pc = await _serviceProvider.GetRequiredService<ICurrencyService>().GetPrimaryStoreCurrency();
            var updateOrder = Builders<Order>.Update
               .Set(x => x.PrimaryCurrencyCode, pc.CurrencyCode);

            var orderRepository = _serviceProvider.GetRequiredService<IRepository<Order>>();

            await orderRepository.Collection.UpdateManyAsync(new BsonDocument(), updateOrder);

            #endregion

            #region Insert new system customer role - staff

            var crStaff = new CustomerRole {
                Name = "Staff",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Staff,
            };
            await _serviceProvider.GetRequiredService<IRepository<CustomerRole>>().InsertAsync(crStaff);

            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            await _mediator.Send(new InstallNewPermissionsCommand() { PermissionProvider = provider });

            #endregion
        }
        private async Task From450To460()
        {

            #region Install String resources

            await InstallStringResources("EN_450_460.xml");

            #endregion

            #region Add new customer action - paid order

            var customerActionType = _serviceProvider.GetRequiredService<IRepository<CustomerActionType>>();
            await customerActionType.InsertAsync(
            new CustomerActionType() {
                Name = "Paid order",
                SystemKeyword = "PaidOrder",
                Enabled = false,
                ConditionType = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13 }
            });

            #endregion

            #region Permisions

            IPermissionProvider provider = new StandardPermissionProvider();
            await _mediator.Send(new InstallNewPermissionsCommand() { PermissionProvider = provider });
            #endregion

            #region Activity Log Type

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "AddNewDocument",
                    Enabled = false,
                    Name = "Add a new document"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "AddNewDocumentType",
                    Enabled = false,
                    Name = "Add a new document type"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "EditDocument",
                    Enabled = false,
                    Name = "Edit document"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "EditDocumentType",
                    Enabled = false,
                    Name = "Edit document type"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "DeleteDocument",
                    Enabled = false,
                    Name = "Delete document"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "DeleteDocumentType",
                    Enabled = false,
                    Name = "Delete document type"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "PublicStore.ViewCourse",
                    Enabled = false,
                    Name = "Public store. View a course"
                });
            await _activityLogTypeRepository.InsertAsync(
                new ActivityLogType {
                    SystemKeyword = "PublicStore.ViewLesson",
                    Enabled = false,
                    Name = "Public store. View a lesson"
                });
            #endregion

            #region Update customer settings

            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var customerSettings = _serviceProvider.GetRequiredService<CustomerSettings>();
            customerSettings.HideDocumentsTab = true;
            customerSettings.HideReviewsTab = false;
            customerSettings.HideCoursesTab = true;
            await _settingService.SaveSetting(customerSettings);

            #endregion

            #region Update catalog settings

            var catalogSettings = _serviceProvider.GetRequiredService<CatalogSettings>();
            catalogSettings.PeriodBestsellers = 6;
            await _settingService.SaveSetting(catalogSettings);

            #endregion

            #region Update topics

            IRepository<Topic> _topicRepository = _serviceProvider.GetRequiredService<IRepository<Topic>>();
            foreach (var topic in _topicRepository.Table)
            {
                topic.Published = true;
                _topicRepository.Update(topic);
            }

            #endregion

            #region Update url seo to lowercase

            IRepository<UrlRecord> _urlRecordRepository = _serviceProvider.GetRequiredService<IRepository<UrlRecord>>();
            foreach (var urlrecord in _urlRecordRepository.Table)
            {
                urlrecord.Slug = urlrecord.Slug.ToLowerInvariant();
                _urlRecordRepository.Update(urlrecord);
            }

            #endregion

            #region Update product - rename fields

            var renameFields = Builders<object>.Update
                .Rename("IsTelecommunicationsOrBroadcastingOrElectronicServices", "IsTele");

            var dbContext = _serviceProvider.GetRequiredService<IMongoDatabase>();
            await dbContext.GetCollection<object>(typeof(Product).Name).UpdateManyAsync(new BsonDocument(), renameFields);

            #endregion

        }

        private async Task From460To470()
        {
            #region Install String resources
            await InstallStringResources("EN_460_470.xml");
            #endregion

            #region MessageTemplates

            var emailAccount = _serviceProvider.GetRequiredService<IRepository<EmailAccount>>().Table.FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("Default email account cannot be loaded");
            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                {
                    Name = "Customer.EmailTokenValidationMessage",
                    Subject = "{{Store.Name}} - Email Verification Code",
                    Body = "Hello {{Customer.FullName}}, <br /><br />\r\n Enter this 6 digit code on the sign in page to confirm your identity:<br /><br /> \r\n <b>{{Customer.Token}}</b><br /><br />\r\n Yours securely, <br /> \r\n Team",
                    IsActive = true,
                    EmailAccountId = emailAccount.Id,
                },
                new MessageTemplate
                {
                    Name = "OrderCancelled.VendorNotification",
                    Subject = "{{Store.Name}}. Order #{{Order.OrderNumber}} cancelled",
                    Body = "<p><a href=\"{{Store.URL}}\">{{Store.Name}}</a> <br /><br />Order #{{Order.OrderNumber}} has been cancelled. <br /><br />Order Number: {{Order.OrderNumber}}<br />   Date Ordered: {{Order.CreatedOn}} <br /><br /> ",
                    IsActive = false,
                    EmailAccountId = emailAccount.Id,
                },

            };

            await _serviceProvider.GetRequiredService<IRepository<MessageTemplate>>().InsertAsync(messageTemplates);
            #endregion

            #region Update store

            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            foreach (var store in await storeService.GetAllStores())
            {
                store.Shortcut = "Store";
                await storeService.UpdateStore(store);
            }

            #endregion

            #region Update specification - sename field

            var specification = _serviceProvider.GetRequiredService<IRepository<SpecificationAttribute>>();

            foreach (var specificationAttribute in specification.Table.ToList())
            {
                specificationAttribute.SeName = SeoExtensions.GenerateSlug(specificationAttribute.Name, false, false);
                specificationAttribute.SpecificationAttributeOptions.ToList().ForEach(x =>
                {
                    x.SeName = SeoExtensions.GenerateSlug(x.Name, false, false);
                });
                await specification.UpdateAsync(specificationAttribute);
            }

            #endregion

            #region Update product attributes - sename field

            var attributes = _serviceProvider.GetRequiredService<IRepository<ProductAttribute>>();
            foreach (var attribute in attributes.Table.ToList())
            {
                attribute.SeName = SeoExtensions.GenerateSlug(attribute.Name, false, false);
                await attributes.UpdateAsync(attribute);
            }

            #endregion

            #region Update blog category - sename field

            var blogcategories = _serviceProvider.GetRequiredService<IRepository<BlogCategory>>();

            foreach (var category in blogcategories.Table.ToList())
            {
                category.SeName = SeoExtensions.GenerateSlug(category.Name, false, false);
                await blogcategories.UpdateAsync(category);
            }

            #endregion

            #region Update media settings

            var settingsService = _serviceProvider.GetRequiredService<ISettingService>();
            var storeInDB = settingsService.GetSettingByKey("Media.Images.StoreInDB", true);
            await settingsService.SetSetting("MediaSettings.StoreInDb", storeInDB);

            #endregion
        }

        private async Task From470To480()
        {
            #region Install String resources

            await InstallStringResources("EN_470_480.xml");

            #endregion

            #region Update customer settings

            var _settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var customerSettings = _serviceProvider.GetRequiredService<CustomerSettings>();
            customerSettings.HideSubAccountsTab = true;
            await _settingService.SaveSetting(customerSettings);

            #endregion

            #region Update permissions - Actions

            IPermissionProvider provider = new StandardPermissionProvider();
            //install new permissions
            await _mediator.Send(new InstallNewPermissionsCommand() { PermissionProvider = provider });

            var permissions = provider.GetPermissions();
            var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
            foreach (var permission in permissions)
            {
                var p = await permissionService.GetPermissionRecordBySystemName(permission.SystemName);
                if (p != null)
                {
                    p.Actions = permission.Actions;
                    await permissionService.UpdatePermissionRecord(p);
                }
            }

            #endregion

            #region Update cancel order Scheduled Task

            var tasks = _serviceProvider.GetRequiredService<IRepository<ScheduleTask>>();
            var cancelOrderTask = new ScheduleTask {
                ScheduleTaskName = "Cancel unpaid and pending orders",
                Type = "Grand.Services.Tasks.CancelOrderScheduledTask, Grand.Services",
                Enabled = false,
                StopOnError = false,
                TimeInterval = 1440
            };
            await tasks.InsertAsync(cancelOrderTask);

            #endregion
        }

        private async Task From480To490()
        {
            var dBContext = _serviceProvider.GetRequiredService<IMongoDBContext>();

            #region Install String resources

            await InstallStringResources("EN_480_490.xml");

            #endregion

            #region Insert activities

            var _activityLogTypeRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLogType>>();
            if (!_activityLogTypeRepository.Table.Where(x => x.SystemKeyword == "CustomerAdmin.UpdateCartCustomer").Any())
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                    SystemKeyword = "CustomerAdmin.UpdateCartCustomer",
                    Enabled = true,
                    Name = "Update shopping cart"
                });
            }
            if (!_activityLogTypeRepository.Table.Where(x => x.SystemKeyword == "AddNewSalesEmployee").Any())
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                    SystemKeyword = "AddNewSalesEmployee",
                    Enabled = true,
                    Name = "Add a sales employee"
                });
            }
            if (!_activityLogTypeRepository.Table.Where(x => x.SystemKeyword == "EditSalesEmployee").Any())
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                    SystemKeyword = "EditSalesEmployee",
                    Enabled = true,
                    Name = "Edit a sales employee"
                });
            }
            if (!_activityLogTypeRepository.Table.Where(x => x.SystemKeyword == "DeleteSalesEmployee").Any())
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                    SystemKeyword = "DeleteSalesEmployee",
                    Enabled = true,
                    Name = "Delete a sales employee"
                });
            }
            if (!_activityLogTypeRepository.Table.Where(x => x.SystemKeyword == "AddRewardPoints").Any())
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType() {
                    SystemKeyword = "AddRewardPoints",
                    Enabled = true,
                    Name = "Assign new reward points"
                });
            }
            #endregion

            #region Upgrade orders

            var orderRepository = _serviceProvider.GetRequiredService<IRepository<Order>>();
            var query = orderRepository.Table.Where(x => x.CurrencyRate != 1 && (x.Rate != 1 || x.Rate != 0)).ToList();

            //upgrade Currency value
            foreach (var order in query)
            {
                var rate = order.CurrencyRate;
                order.Rate = rate;

                if (order.OrderSubtotalInclTax > 0)
                    order.OrderSubtotalInclTax = Math.Round(order.OrderSubtotalInclTax * rate, 2);

                if (order.OrderSubtotalExclTax > 0)
                    order.OrderSubtotalExclTax = Math.Round(order.OrderSubtotalExclTax * rate, 2);

                if (order.OrderSubTotalDiscountInclTax > 0)
                    order.OrderSubTotalDiscountInclTax = Math.Round(order.OrderSubTotalDiscountInclTax * rate, 2);

                if (order.OrderSubTotalDiscountExclTax > 0)
                    order.OrderSubTotalDiscountExclTax = Math.Round(order.OrderSubTotalDiscountExclTax * rate, 2);

                if (order.OrderShippingInclTax > 0)
                    order.OrderShippingInclTax = Math.Round(order.OrderShippingInclTax * rate, 2);

                if (order.OrderShippingExclTax > 0)
                    order.OrderShippingExclTax = Math.Round(order.OrderShippingExclTax * rate, 2);

                if (order.PaymentMethodAdditionalFeeInclTax > 0)
                    order.PaymentMethodAdditionalFeeInclTax = Math.Round(order.PaymentMethodAdditionalFeeInclTax * rate, 2);

                if (order.PaymentMethodAdditionalFeeExclTax > 0)
                    order.PaymentMethodAdditionalFeeExclTax = Math.Round(order.PaymentMethodAdditionalFeeExclTax * rate, 2);

                if (order.OrderTax > 0)
                    order.OrderTax = Math.Round(order.OrderTax * rate, 2);

                if (order.OrderDiscount > 0)
                    order.OrderDiscount = Math.Round(order.OrderDiscount * rate, 2);

                if (order.OrderTotal > 0)
                    order.OrderTotal = Math.Round(order.OrderTotal * rate, 2);

                if (order.RefundedAmount > 0)
                    order.RefundedAmount = Math.Round(order.RefundedAmount * rate, 2);

                foreach (var orderItems in order.OrderItems)
                {
                    if (orderItems.UnitPriceWithoutDiscInclTax > 0)
                        orderItems.UnitPriceWithoutDiscInclTax = Math.Round(orderItems.UnitPriceWithoutDiscInclTax * rate, 2);
                    if (orderItems.UnitPriceWithoutDiscExclTax > 0)
                        orderItems.UnitPriceWithoutDiscExclTax = Math.Round(orderItems.UnitPriceWithoutDiscExclTax * rate, 2);
                    if (orderItems.UnitPriceInclTax > 0)
                        orderItems.UnitPriceInclTax = Math.Round(orderItems.UnitPriceInclTax * rate, 2);
                    if (orderItems.UnitPriceExclTax > 0)
                        orderItems.UnitPriceExclTax = Math.Round(orderItems.UnitPriceExclTax * rate, 2);
                    if (orderItems.PriceInclTax > 0)
                        orderItems.PriceInclTax = Math.Round(orderItems.PriceInclTax * rate, 2);
                    if (orderItems.PriceExclTax > 0)
                        orderItems.PriceExclTax = Math.Round(orderItems.PriceExclTax * rate, 2);
                    if (orderItems.DiscountAmountInclTax > 0)
                        orderItems.DiscountAmountInclTax = Math.Round(orderItems.DiscountAmountInclTax * rate, 2);
                    if (orderItems.DiscountAmountExclTax > 0)
                        orderItems.DiscountAmountExclTax = Math.Round(orderItems.DiscountAmountExclTax * rate, 2);
                }

                await orderRepository.UpdateAsync(order);
            }

            //upgrade Taxes on the order
            var orderTaxRepository = dBContext.Database().GetCollection<OldOrders>("Order");
            await orderTaxRepository.Find(new BsonDocument()).ForEachAsync(async (o) =>
            {
                if (!o.OrderTaxes.Any())
                {
                    var taxes = o.ParseTaxRates(o.TaxRates);
                    foreach (var item in taxes)
                    {
                        o.OrderTaxes.Add(new OrderTax() {
                            Percent = item.Key,
                            Amount = Math.Round(item.Value * o.CurrencyRate, 4)
                        });
                    }
                    await orderTaxRepository.ReplaceOneAsync(x => x.Id == o.Id, o);
                }
            });

            #endregion

            #region Insert new system customer role - sales manager

            var crSalesManager = new CustomerRole {
                Name = "Sales manager",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.SalesManager,
            };
            await _serviceProvider.GetRequiredService<IRepository<CustomerRole>>().InsertAsync(crSalesManager);

            #endregion

            #region Upgrade Git cards - update CurrencyCode

            var pc = await _serviceProvider.GetRequiredService<ICurrencyService>().GetPrimaryStoreCurrency();

            var giftCardsRepository = dBContext.Database().GetCollection<GiftCard>("GiftCard");
            await giftCardsRepository.Find(new BsonDocument()).ForEachAsync(async (o) =>
            {
                o.CurrencyCode = pc.CurrencyCode;
                await giftCardsRepository.ReplaceOneAsync(x => x.Id == o.Id, o);
            });

            #endregion

            #region Upgrade Address attributes field / customer attributes

            static List<CustomAttribute> ParseAddressCustomAttributes(string attributesXml)
            {
                var customAttribute = new List<CustomAttribute>();

                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(attributesXml);

                    var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/AddressAttribute");
                    foreach (XmlNode node1 in nodeList1)
                    {
                        if (node1.Attributes != null && node1.Attributes["ID"] != null)
                        {
                            var key = node1.Attributes["ID"].InnerText.Trim();

                            var nodeList2 = node1.SelectNodes(@"AddressAttributeValue/Value");
                            foreach (XmlNode node2 in nodeList2)
                            {
                                var value = node2.InnerText.Trim();
                                customAttribute.Add(new CustomAttribute() { Key = key, Value = value });
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.Write(exc.ToString());
                }
                return customAttribute;
            }

            static List<CustomAttribute> ParseCustomerCustomAttributes(string attributesXml)
            {
                var customAttribute = new List<CustomAttribute>();

                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(attributesXml);

                    var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CustomerAttribute");
                    foreach (XmlNode node1 in nodeList1)
                    {
                        if (node1.Attributes != null && node1.Attributes["ID"] != null)
                        {
                            var key = node1.Attributes["ID"].InnerText.Trim();

                            var nodeList2 = node1.SelectNodes(@"CustomerAttributeValue/Value");
                            foreach (XmlNode node2 in nodeList2)
                            {
                                var value = node2.InnerText.Trim();
                                customAttribute.Add(new CustomAttribute() { Key = key, Value = value });
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.Write(exc.ToString());
                }
                return customAttribute;
            }

            static List<CustomAttribute> ParseProductCustomAttributes(string attributesXml)
            {
                var customAttribute = new List<CustomAttribute>();

                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(attributesXml);

                    var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                    foreach (XmlNode node1 in nodeList1)
                    {
                        if (node1.Attributes != null && node1.Attributes["ID"] != null)
                        {
                            var key = node1.Attributes["ID"].InnerText.Trim();

                            var nodeList2 = node1.SelectNodes(@"ProductAttributeValue/Value");
                            foreach (XmlNode node2 in nodeList2)
                            {
                                var value = node2.InnerText.Trim();
                                customAttribute.Add(new CustomAttribute() { Key = key, Value = value });
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.Write(exc.ToString());
                }
                return customAttribute;
            }

            //upgrade customer data - billingaddress/shippingaddress - Addresses
            var customerRepository = dBContext.Database().GetCollection<Customer>("Customer");

            await customerRepository.Find(new BsonDocument()).ForEachAsync(async (c) =>
            {
                var update = false;

                if (!string.IsNullOrEmpty(c.BillingAddress?.CustomAttributes))
                {
                    c.BillingAddress.Attributes = ParseAddressCustomAttributes(c.BillingAddress.CustomAttributes);
                    update = true;
                }

                if (!string.IsNullOrEmpty(c.ShippingAddress?.CustomAttributes))
                {
                    c.ShippingAddress.Attributes = ParseAddressCustomAttributes(c.ShippingAddress.CustomAttributes);
                    update = true;
                }

                if (c.Addresses.Where(x => !string.IsNullOrEmpty(x.CustomAttributes)).Any())
                {
                    foreach (var address in c.Addresses.Where(x => !string.IsNullOrEmpty(x.CustomAttributes)))
                    {
                        address.Attributes = ParseAddressCustomAttributes(address.CustomAttributes);
                        update = true;
                    }
                }
                if (c.GenericAttributes.Where(x => x.Key == "CustomCustomerAttributes").Any())
                {
                    var value = c.GenericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes").Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        c.Attributes = ParseCustomerCustomAttributes(value);
                        update = true;
                    }
                }
                if (c.ShoppingCartItems.Where(x => !string.IsNullOrEmpty(x.AttributesXml)).Any())
                {
                    foreach (var sc in c.ShoppingCartItems.Where(x => !string.IsNullOrEmpty(x.AttributesXml)))
                    {
                        sc.Attributes = ParseProductCustomAttributes(sc.AttributesXml);
                        update = true;
                    }
                }
                if (update)
                    await customerRepository.ReplaceOneAsync(x => x.Id == c.Id, c);

            });

            //upgrade order data - billingaddress/shippingaddress - CustomAttributes / AttributeXML for items
            var orderAttributesRepository = dBContext.Database().GetCollection<Order>("Order");
            await orderAttributesRepository.Find(new BsonDocument()).ForEachAsync(async (o) =>
            {
                if (!string.IsNullOrEmpty(o.BillingAddress?.CustomAttributes) || !string.IsNullOrEmpty(o.ShippingAddress?.CustomAttributes))
                {
                    if (!string.IsNullOrEmpty(o.BillingAddress?.CustomAttributes))
                        o.BillingAddress.Attributes = ParseAddressCustomAttributes(o.BillingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(o.ShippingAddress?.CustomAttributes))
                        o.ShippingAddress.Attributes = ParseAddressCustomAttributes(o.ShippingAddress.CustomAttributes);

                    await orderAttributesRepository.ReplaceOneAsync(x => x.Id == o.Id, o);
                }
                if (o.OrderItems.Where(x => !string.IsNullOrEmpty(x.AttributesXml)).Any())
                {
                    foreach (var item in o.OrderItems.Where(x => !string.IsNullOrEmpty(x.AttributesXml)))
                    {
                        item.Attributes = ParseProductCustomAttributes(item.AttributesXml);
                    }
                    await orderAttributesRepository.ReplaceOneAsync(x => x.Id == o.Id, o);
                }
            });

            //update shipment items
            var shipmentAttributesRepository = dBContext.Database().GetCollection<Shipment>("Shipment");
            await shipmentAttributesRepository.Find(new BsonDocument()).ForEachAsync(async (o) =>
            {
                if (o.ShipmentItems.Where(x => !string.IsNullOrEmpty(x.AttributeXML)).Any())
                {
                    foreach (var item in o.ShipmentItems.Where(x => !string.IsNullOrEmpty(x.AttributeXML)))
                    {
                        item.Attributes = ParseProductCustomAttributes(item.AttributeXML);
                    }
                    await shipmentAttributesRepository.ReplaceOneAsync(x => x.Id == o.Id, o);
                }
            });

            //update products
            var products = _serviceProvider.GetRequiredService<IRepository<Product>>();
            //combination
            var productAttributeCombinations = products.Table.Where(x => x.ProductAttributeCombinations.Any()).ToList();
            foreach (var product in productAttributeCombinations)
            {
                foreach (var item in product.ProductAttributeCombinations)
                {
                    item.Attributes = ParseProductCustomAttributes(item.AttributesXml);
                }
                await products.UpdateAsync(product);
            }
            //attributes condition
            var productAttributeConditions = products.Table.Where(x => x.ProductAttributeMappings.Any()).ToList();
            foreach (var product in productAttributeConditions)
            {
                var update = false;
                foreach (var item in product.ProductAttributeMappings)
                {
                    item.ConditionAttribute = ParseProductCustomAttributes(item.ConditionAttributeXml);
                    update = true;
                }

                if(update)
                    await products.UpdateAsync(product);
            }

            #endregion

            #region Admin menu

            var adminRepository = _serviceProvider.GetRequiredService<IRepository<AdminSiteMap>>();
            await adminRepository.InsertManyAsync(StandardAdminSiteMap.SiteMap);

            #endregion
        }

        private async Task InstallStringResources(string filenames)
        {
            //'English' language            
            var langRepository = _serviceProvider.GetRequiredService<IRepository<Language>>();
            var language = langRepository.Table.FirstOrDefault(l => l.Name == "English");

            if (language == null)
                language = langRepository.Table.FirstOrDefault();

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/Upgrade"), "*" + filenames, SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
                await localizationService.ImportResourcesFromXmlInstall(language, localesXml);
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

        class OldOrders : Order
        {
            public string TaxRates { get; set; }
        }
    }
}