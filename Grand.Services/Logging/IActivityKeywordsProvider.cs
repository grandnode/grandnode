using System.Collections.Generic;

namespace Grand.Services.Logging
{
    public partial interface IActivityKeywordsProvider
    {
        IList<string> GetCategorySystemKeywords();
        IList<string> GetCheckoutAttributeSystemKeywords();
        IList<string> GetContactAttributeSystemKeywords();
        IList<string> GetCustomerSystemKeywords();
        IList<string> GetCustomerRoleSystemKeywords();
        IList<string> GetDiscountSystemKeywords();
        IList<string> GetGiftCardSystemKeywords();
        IList<string> GetInteractiveFormSystemKeywords();
        IList<string> GetKnowledgebaseArticleSystemKeywords();
        IList<string> GetKnowledgebaseCategorySystemKeywords();
        IList<string> GetManufacturerSystemKeywords();
        IList<string> GetOrderSystemKeywords();
        IList<string> GetProductSystemKeywords();
        IList<string> GetProductAttributeSystemKeywords();
        IList<string> GetReturnRequestSystemKeywords();
        IList<string> GetSpecificationAttributeSystemKeywords();
        IList<string> GetTopicSystemKeywords();
    }
}
