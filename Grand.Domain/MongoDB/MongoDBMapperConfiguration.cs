using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Documents;
using Grand.Domain.Forums;
using Grand.Domain.Logging;
using Grand.Domain.Media;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Vendors;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace Grand.Domain.MongoDB
{
    public static class MongoDBMapperConfiguration
    {

        /// <summary>
        /// Register MongoDB mappings
        /// </summary>
        public static void RegisterMongoDBMappings()
        {
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            BsonSerializer.RegisterSerializer(typeof(DateTime), new BsonUtcDateTimeSerializer());

            //global set an equivalent of [BsonIgnoreExtraElements] for every Domain Model
            var cp = new ConventionPack();
            cp.Add(new IgnoreExtraElementsConvention(true));
            ConventionRegistry.Register("ApplicationConventions", cp, t => true);

            RegisterClassProduct();
            RegisterClassProductAttributeCombination();
            RegisterClassProductAttributeMapping();
            RegisterClassProductAttributeValue();
            RegisterClassProductCategory();
            RegisterClassProductManufacturer();
            RegisterClassProductPicture();
            RegisterClassProductSpecificationAttribute();
            RegisterClassProductTag();
            RegisterClassProductWarehouseInventory();
            RegisterClassRelatedProduct();
            RegisterClassBundleProduct();
            RegisterClassTierPrice();
            RegisterClassAddress();
            RegisterClassCustomer();
            RegisterClassShoppingCartItem();
            RegisterClassCustomerAction();
            RegisterClassActionCondition();
            RegisterClassCustomerAttribute();
            RegisterClassCustomerHistoryPassword();
            RegisterClassCustomerReminder();
            RegisterClassReminderCondition();
            RegisterClassCustomerReminderHistory();
            RegisterClassCustomerRole();
            RegisterClassDiscount();
            RegisterClassForumTopic();
            RegisterClassLog();
            RegisterClassDownload();
            RegisterClassCampaign();
            RegisterClassEmailAccount();
            RegisterClassFormAttribute();
            RegisterClassMessageTemplate();
            RegisterClassQueuedEmail();
            RegisterClassCheckoutAttribute();
            RegisterClassGiftCard();
            RegisterClassOrder();
            RegisterClassShipmentItem();
            RegisterClassVendorNote();
            RegisterClassCurrency();
            RegisterClassDocument();
            RegisterClassOrderTag();
        }

        private static void RegisterClassProduct()
        {
            BsonClassMap.RegisterClassMap<Product>(cm =>
            {
                cm.AutoMap();
                //ignore these Fields, an equivalent of [BsonIgnore]
                cm.UnmapMember(c => c.ProductType);
                cm.UnmapMember(c => c.IntervalUnitType);
                cm.UnmapMember(c => c.BackorderMode);
                cm.UnmapMember(c => c.DownloadActivationType);
                cm.UnmapMember(c => c.GiftCardType);
                cm.UnmapMember(c => c.LowStockActivity);
                cm.UnmapMember(c => c.ManageInventoryMethod);
                cm.UnmapMember(c => c.RecurringCyclePeriod);
            });
        }
        private static void RegisterClassProductAttributeCombination()
        {
            BsonClassMap.RegisterClassMap<ProductAttributeCombination>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassProductAttributeMapping()
        {
            BsonClassMap.RegisterClassMap<ProductAttributeMapping>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeControlType);
            });
        }
        private static void RegisterClassProductAttributeValue()
        {
            BsonClassMap.RegisterClassMap<ProductAttributeValue>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductAttributeMappingId);
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeValueType);
            });
        }
        private static void RegisterClassProductCategory()
        {
            BsonClassMap.RegisterClassMap<ProductCategory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassProductManufacturer()
        {
            BsonClassMap.RegisterClassMap<ProductManufacturer>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassProductPicture()
        {
            BsonClassMap.RegisterClassMap<ProductPicture>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

        }
        private static void RegisterClassProductSpecificationAttribute()
        {
            BsonClassMap.RegisterClassMap<ProductSpecificationAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeType);
            });
        }
        private static void RegisterClassProductTag()
        {
            BsonClassMap.RegisterClassMap<ProductTag>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassProductWarehouseInventory()
        {
            BsonClassMap.RegisterClassMap<ProductWarehouseInventory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassRelatedProduct()
        {
            BsonClassMap.RegisterClassMap<RelatedProduct>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId1);
            });
        }
        private static void RegisterClassBundleProduct()
        {
            BsonClassMap.RegisterClassMap<BundleProduct>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductBundleId);
            });
        }
        private static void RegisterClassTierPrice()
        {
            BsonClassMap.RegisterClassMap<TierPrice>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });
        }
        private static void RegisterClassAddress()
        {
            BsonClassMap.RegisterClassMap<Address>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerId);
            });
        }
        private static void RegisterClassCustomer()
        {
            BsonClassMap.RegisterClassMap<Customer>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.PasswordFormat);
            });
        }
        private static void RegisterClassShoppingCartItem()
        {
            BsonClassMap.RegisterClassMap<ShoppingCartItem>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ShoppingCartType);
            });
        }
        private static void RegisterClassCustomerAction()
        {
            BsonClassMap.RegisterClassMap<CustomerAction>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Condition);
                cm.UnmapMember(c => c.ReactionType);
            });
        }
        private static void RegisterClassActionCondition()
        {
            BsonClassMap.RegisterClassMap<CustomerAction.ActionCondition>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerActionConditionType);
                cm.UnmapMember(c => c.Condition);
            });
        }
        private static void RegisterClassCustomerAttribute()
        {
            BsonClassMap.RegisterClassMap<CustomerAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });
        }
        private static void RegisterClassCustomerHistoryPassword()
        {
            BsonClassMap.RegisterClassMap<CustomerHistoryPassword>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.PasswordFormat);
            });
        }
        private static void RegisterClassCustomerReminder()
        {
            BsonClassMap.RegisterClassMap<CustomerReminder>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Condition);
                cm.UnmapMember(c => c.ReminderRule);
            });
        }
        private static void RegisterClassReminderCondition()
        {
            BsonClassMap.RegisterClassMap<CustomerReminder.ReminderCondition>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ConditionType);
                cm.UnmapMember(c => c.Condition);
            });
        }
        private static void RegisterClassCustomerReminderHistory()
        {
            BsonClassMap.RegisterClassMap<CustomerReminderHistory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ReminderRule);
                cm.UnmapMember(c => c.HistoryStatus);
            });
        }
        private static void RegisterClassCustomerRole()
        {
            BsonClassMap.RegisterClassMap<CustomerRole>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerId);
            });
        }
        private static void RegisterClassDiscount()
        {
            BsonClassMap.RegisterClassMap<Discount>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DiscountType);
                cm.UnmapMember(c => c.DiscountLimitation);
            });
        }
        private static void RegisterClassForumTopic()
        {
            BsonClassMap.RegisterClassMap<ForumTopic>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ForumTopicType);
            });
        }
        private static void RegisterClassLog()
        {
            BsonClassMap.RegisterClassMap<Log>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.LogLevel);
            });
        }
        private static void RegisterClassDownload()
        {
            BsonClassMap.RegisterClassMap<Download>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DownloadBinary);
            });
        }
        private static void RegisterClassCampaign()
        {
            BsonClassMap.RegisterClassMap<Campaign>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerHasOrdersCondition);
                cm.UnmapMember(c => c.CustomerHasShoppingCartCondition);
            });
        }
        private static void RegisterClassEmailAccount()
        {
            BsonClassMap.RegisterClassMap<EmailAccount>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.FriendlyName);
            });
        }
        private static void RegisterClassFormAttribute()
        {
            BsonClassMap.RegisterClassMap<InteractiveForm.FormAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });
        }
        private static void RegisterClassMessageTemplate()
        {
            BsonClassMap.RegisterClassMap<MessageTemplate>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DelayPeriod);
            });
        }
        private static void RegisterClassQueuedEmail()
        {
            BsonClassMap.RegisterClassMap<QueuedEmail>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Priority);
            });
        }
        private static void RegisterClassCheckoutAttribute()
        {
            BsonClassMap.RegisterClassMap<CheckoutAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });
        }
        private static void RegisterClassGiftCard()
        {
            BsonClassMap.RegisterClassMap<GiftCard>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.GiftCardType);
            });
        }
        private static void RegisterClassOrder()
        {
            BsonClassMap.RegisterClassMap<Order>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.OrderStatus);
                cm.UnmapMember(c => c.PaymentStatus);
                cm.UnmapMember(c => c.ShippingStatus);
                cm.UnmapMember(c => c.CustomerTaxDisplayType);
                cm.UnmapMember(c => c.TaxRatesDictionary);
            });
        }
        private static void RegisterClassShipmentItem()
        {
            BsonClassMap.RegisterClassMap<ShipmentItem>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ShipmentId);
            });
        }
        private static void RegisterClassVendorNote()
        {
            BsonClassMap.RegisterClassMap<VendorNote>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.VendorId);
            });
        }
        private static void RegisterClassCurrency()
        {
            BsonClassMap.RegisterClassMap<Currency>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.RoundingType);
            });
        }
        private static void RegisterClassDocument()
        {
            BsonClassMap.RegisterClassMap<Document>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DocumentStatus);
                cm.UnmapMember(c => c.Reference);
            });
        }

        private static void RegisterClassOrderTag()
        {
            BsonClassMap.RegisterClassMap<OrderTag>(cm =>
            {
                cm.AutoMap();
            });
        }
    }
}
