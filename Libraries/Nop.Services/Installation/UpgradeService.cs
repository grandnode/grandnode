using MongoDB.Driver;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Installation
{
    public partial class UpgradeService : IUpgradeService
    {
        #region Fields

        private readonly IRepository<GrandNodeVersion> _versionRepository;
        private readonly IWebHelper _webHelper;

        private const string version_360 = "3.60";
        private const string version_370 = "3.70";
        private const string version_380 = "3.80";

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

            if (fromversion == version_370)
            {
                From370To380();
                fromversion = version_380;
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
                    LanguageId = 0,
                    IsActive = true,
                    Slug = seName
                });
                topic.SeName = seName;
                EngineContext.Current.Resolve<IRepository<Topic>>().Update(topic);
            }


            #endregion

            #region Settings

            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.AllowViewUnpublishedProductPage", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.PublishBackProductWhenCancellingOrders", Value = "false" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.NewProductsNumber", Value = "6" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.NewProductsEnabled", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.AjaxProcessAttributeChange", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CatalogSettings.DisplayTaxShippingInfoShoppingCart", Value = "false" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "CustomerSettings.DateOfBirthRequired", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "MediaSettings.VendorThumbPictureSize", Value = "450" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "StoreInformationSettings.HidePoweredByGrandNode", Value = "false" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "RewardPointsSettings.PointsAccumulatedForAllStores", Value = "true" });
            EngineContext.Current.Resolve<IRepository<Setting>>().Insert(new Setting() { Name = "VendorSettings.AllowCustomersToApplyForVendorAccount", Value = "true" });

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

            EngineContext.Current.Resolve<IRepository<ReturnRequestReason>>().Collection.Indexes.CreateOneAsync(Builders<ReturnRequestReason>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });
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

            EngineContext.Current.Resolve<IRepository<ReturnRequestAction>>().Collection.Indexes.CreateOneAsync(Builders<ReturnRequestAction>.IndexKeys.Ascending(x => x.Id), new CreateIndexOptions() { Name = "Id", Unique = true });

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

        private void From370To380()
        {
            #region Install String resources
                InstallStringResources("370_380.nopres.xml");
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