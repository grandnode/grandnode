using Grand.Core.Configuration;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Vendors;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace Grand.Core.Infrastructure.MongoDB
{
    public static class MongoDBMapperConfiguration
    {

        /// <summary>
        /// Register MongoDB mappings
        /// </summary>
        /// <param name="config">Config</param>
        public static void RegisterMongoDBMappings(GrandConfig config)
        {
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            BsonSerializer.RegisterSerializer(typeof(DateTime), new BsonUtcDateTimeSerializer());

            //global set an equivalent of [BsonIgnoreExtraElements] for every Domain Model
            var cp = new ConventionPack();
            cp.Add(new IgnoreExtraElementsConvention(true));
            ConventionRegistry.Register("ApplicationConventions", cp, t => true);

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

            BsonClassMap.RegisterClassMap<ProductAttributeCombination>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<ProductAttributeMapping>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeControlType);
            });

            BsonClassMap.RegisterClassMap<ProductAttributeValue>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductAttributeMappingId);
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeValueType);
            });

            BsonClassMap.RegisterClassMap<ProductCategory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<ProductManufacturer>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<ProductPicture>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<ProductSpecificationAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
                cm.UnmapMember(c => c.AttributeType);
            });

            BsonClassMap.RegisterClassMap<ProductTag>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<ProductWarehouseInventory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<RelatedProduct>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId1);
            });

            BsonClassMap.RegisterClassMap<BundleProduct>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductBundleId);
            });

            BsonClassMap.RegisterClassMap<TierPrice>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ProductId);
            });

            BsonClassMap.RegisterClassMap<Address>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerId);
            });

            BsonClassMap.RegisterClassMap<Customer>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.PasswordFormat);
            });
            BsonClassMap.RegisterClassMap<ShoppingCartItem>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ShoppingCartType);
            });
            BsonClassMap.RegisterClassMap<CustomerAction>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Condition);
                cm.UnmapMember(c => c.ReactionType);
            });

            BsonClassMap.RegisterClassMap<CustomerAction.ActionCondition>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerActionConditionType);
                cm.UnmapMember(c => c.Condition);
            });

            BsonClassMap.RegisterClassMap<CustomerAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });

            BsonClassMap.RegisterClassMap<CustomerHistoryPassword>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.PasswordFormat);
            });

            BsonClassMap.RegisterClassMap<CustomerReminder>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Condition);
                cm.UnmapMember(c => c.ReminderRule);
            });

            BsonClassMap.RegisterClassMap<CustomerReminder.ReminderCondition>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ConditionType);
                cm.UnmapMember(c => c.Condition);
            });

            BsonClassMap.RegisterClassMap<CustomerReminderHistory>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ReminderRule);
                cm.UnmapMember(c => c.HistoryStatus);
            });

            BsonClassMap.RegisterClassMap<CustomerRole>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerId);
            });

            BsonClassMap.RegisterClassMap<Discount>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DiscountType);
                cm.UnmapMember(c => c.DiscountLimitation);
            });

            BsonClassMap.RegisterClassMap<ForumTopic>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ForumTopicType);
            });

            BsonClassMap.RegisterClassMap<Log>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.LogLevel);
            });

            BsonClassMap.RegisterClassMap<Download>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DownloadBinary);
            });

            BsonClassMap.RegisterClassMap<Campaign>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.CustomerHasOrdersCondition);
                cm.UnmapMember(c => c.CustomerHasShoppingCartCondition);
            });

            BsonClassMap.RegisterClassMap<EmailAccount>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.FriendlyName);
            });

            BsonClassMap.RegisterClassMap<InteractiveForm.FormAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });

            BsonClassMap.RegisterClassMap<MessageTemplate>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.DelayPeriod);
            });

            BsonClassMap.RegisterClassMap<QueuedEmail>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.Priority);
            });

            BsonClassMap.RegisterClassMap<CheckoutAttribute>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.AttributeControlType);
            });

            BsonClassMap.RegisterClassMap<GiftCard>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.GiftCardType);
            });

            BsonClassMap.RegisterClassMap<Order>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.OrderStatus);
                cm.UnmapMember(c => c.PaymentStatus);
                cm.UnmapMember(c => c.ShippingStatus);
                cm.UnmapMember(c => c.CustomerTaxDisplayType);
                cm.UnmapMember(c => c.TaxRatesDictionary);
            });

            BsonClassMap.RegisterClassMap<ShipmentItem>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.ShipmentId);
            });

            BsonClassMap.RegisterClassMap<VendorNote>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.VendorId);
            });

            BsonClassMap.RegisterClassMap<Currency>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.RoundingType);
            });
        }
    }
}
