using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Vendors;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddStoreTokens(LiquidObject liquidObject, Store store, Language language, EmailAccount emailAccount);
        Task AddOrderTokens(LiquidObject liquidObject, Order order, Customer customer, Store store, OrderNote orderNote = null, string vendorId = "", decimal refundedAmount = 0);
        Task AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, Order order, Store store, Language language);
        void AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment);
        Task AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Order orderItem, Language language);
        void AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard);
        void AddCustomerTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, CustomerNote customerNote = null);
        Task AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, string personalMessage = "", string customerEmail = "");
        Task AddVendorTokens(LiquidObject liquidObject, Vendor vendor, Language language);
        void AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription, Store store);
        void AddProductReviewTokens(LiquidObject liquidObject, Product product, ProductReview productReview);
        void AddVendorReviewTokens(LiquidObject liquidObject, Vendor vendor, VendorReview VendorReview);
        void AddBlogCommentTokens(LiquidObject liquidObject, BlogPost blogPost, BlogComment blogComment, Store store, Language language);
        void AddArticleCommentTokens(LiquidObject liquidObject, KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, Store store, Language language);
        void AddNewsCommentTokens(LiquidObject liquidObject, NewsItem newsItem, NewsComment newsComment, Store store, Language language);
        void AddProductTokens(LiquidObject liquidObject, Product product, Language language, Store store);
        Task AddAttributeCombinationTokens(LiquidObject liquidObject, Customer customer, Product product, ProductAttributeCombination combination);
        void AddForumTokens(LiquidObject liquidObject, Customer customer, Store store, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "");
        void AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage);
        void AddBackInStockTokens(LiquidObject liquidObject, Product product, BackInStockSubscription subscription, Store store, Language language);
        Task AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid);
        string[] GetListOfCampaignAllowedTokens();
        string[] GetListOfAllowedTokens();
        string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule);
    }
}