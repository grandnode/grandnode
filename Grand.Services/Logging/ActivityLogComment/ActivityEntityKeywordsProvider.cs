using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Logging.ActivityLogComment
{

    public class ActivityLogEntity
    {
        public ActivityLogEntityType EntityType { get; set; }

        public IList<string> EntityKeywords { get; set; }

        public string LinkPattern { get; set; }
    }

    public class ActivityEntityKeywordsProvider : IActivityEntityKeywordsProvider
    {
        private readonly IActivityKeywordsProvider _activityKeywordsProvider;
        private readonly IList<ActivityLogEntity> _activityEntities;

        public ActivityEntityKeywordsProvider(IActivityKeywordsProvider activityKeywordsProvider)
        {
            _activityKeywordsProvider = activityKeywordsProvider;
            _activityEntities = GetActivityEntities();
        }

        public ActivityLogEntity GetLogEntity(string activityKeyword)
        {
            return _activityEntities.FirstOrDefault(ae => ae.EntityKeywords.Contains(activityKeyword));
        }

        private IList<ActivityLogEntity> GetActivityEntities()
        {
            return new List<ActivityLogEntity> {
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Category,
                    EntityKeywords = _activityKeywordsProvider.GetCategorySystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Category + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.CheckoutAttribute,
                    EntityKeywords = _activityKeywordsProvider.GetCheckoutAttributeSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.CheckoutAttribute + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.ContactAttribute,
                    EntityKeywords = _activityKeywordsProvider.GetContactAttributeSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.ContactAttribute + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Customer,
                    EntityKeywords = _activityKeywordsProvider.GetCustomerSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Customer + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.CustomerRole,
                    EntityKeywords = _activityKeywordsProvider.GetCustomerRoleSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.CustomerRole + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Discount,
                    EntityKeywords = _activityKeywordsProvider.GetDiscountSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Discount + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.GiftCard,
                    EntityKeywords = _activityKeywordsProvider.GetGiftCardSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.GiftCard + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.InteractiveForm,
                    EntityKeywords = _activityKeywordsProvider.GetInteractiveFormSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.InteractiveForm + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.KnowledgebaseArticle,
                    EntityKeywords = _activityKeywordsProvider.GetKnowledgebaseArticleSystemKeywords(),
                    LinkPattern = "/Admin/Knowledgebase/EditArticle/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.KnowledgebaseCategory,
                    EntityKeywords = _activityKeywordsProvider.GetKnowledgebaseCategorySystemKeywords(),
                    LinkPattern = "/Admin/Knowledgebase/EditCategory/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Manufacturer,
                    EntityKeywords = _activityKeywordsProvider.GetManufacturerSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Manufacturer + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Order,
                    EntityKeywords = _activityKeywordsProvider.GetOrderSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Order + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Product,
                    EntityKeywords = _activityKeywordsProvider.GetProductSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Product + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.ProductAttribute,
                    EntityKeywords = _activityKeywordsProvider.GetProductAttributeSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.ProductAttribute + "/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.ReturnRequest,
                    EntityKeywords = _activityKeywordsProvider.GetReturnRequestSystemKeywords(),
                    LinkPattern = "/Admin/ReturnRequestDetails/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.SpecificationAttribute,
                    EntityKeywords = _activityKeywordsProvider.GetSpecificationAttributeSystemKeywords(),
                    LinkPattern = "/Admin/SpecificationAttribute/Edit/{0}"
               },
                new ActivityLogEntity {
                    EntityType = ActivityLogEntityType.Topic,
                    EntityKeywords = _activityKeywordsProvider.GetTopicSystemKeywords(),
                    LinkPattern = "/Admin/" + ActivityLogEntityType.Topic + "/Edit/{0}"
                },
            };
        }
    }
}
