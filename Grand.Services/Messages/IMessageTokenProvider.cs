using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Messages;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        Task AddStoreTokens(LiquidObject liquidObject, Store store, Language language, EmailAccount emailAccount);
        Task AddOrderTokens(LiquidObject liquidObject, Order order, Customer customer, Store store, OrderNote orderNote = null, Vendor vendor = null, decimal refundedAmount = 0);
        Task AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, Order order, Store store, Language language);
        Task AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment);
        Task AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Store store, Order orderItem, Language language, ReturnRequestNote returnRequestNote = null);
        Task AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard);
        Task AddCustomerTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, CustomerNote customerNote = null);
        Task AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, string personalMessage = "", string customerEmail = "");
        Task AddVendorTokens(LiquidObject liquidObject, Vendor vendor, Language language);
        Task AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription, Store store);
        Task AddProductReviewTokens(LiquidObject liquidObject, Product product, ProductReview productReview);
        Task AddVendorReviewTokens(LiquidObject liquidObject, Vendor vendor, VendorReview VendorReview);
        Task AddBlogCommentTokens(LiquidObject liquidObject, BlogPost blogPost, BlogComment blogComment, Store store, Language language);
        Task AddArticleCommentTokens(LiquidObject liquidObject, KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, Store store, Language language);
        Task AddNewsCommentTokens(LiquidObject liquidObject, NewsItem newsItem, NewsComment newsComment, Store store, Language language);
        Task AddProductTokens(LiquidObject liquidObject, Product product, Language language, Store store);
        Task AddAttributeCombinationTokens(LiquidObject liquidObject, Product product, ProductAttributeCombination combination);
        Task AddForumTokens(LiquidObject liquidObject, Customer customer, Store store, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "");
        Task AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage);
        Task AddBackInStockTokens(LiquidObject liquidObject, Product product, BackInStockSubscription subscription, Store store, Language language);
        Task AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid);
        string[] GetListOfCampaignAllowedTokens();
        string[] GetListOfAllowedTokens();
        string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule);
    }
}