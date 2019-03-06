using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Vendors;
using Grand.Services.Messages.DotLiquidDrops;

namespace Grand.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddStoreTokens(LiquidObject liquidObject, Store store, EmailAccount emailAccount);
        void AddOrderTokens(LiquidObject liquidObject, Order order, string languageId, OrderNote orderNote = null, string vendorId = "", decimal refundedAmount = 0);
        void AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, string languageId);
        void AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment);
        void AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Order orderItem);
        void AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard);
        void AddCustomerTokens(LiquidObject liquidObject, Customer customer, CustomerNote customerNote = null);
        void AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, string personalMessage = "", string customerEmail = "");
        void AddVendorTokens(LiquidObject liquidObject, Vendor vendor);
        void AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription);
        void AddProductReviewTokens(LiquidObject liquidObject, ProductReview productReview);
        void AddVendorReviewTokens(LiquidObject liquidObject, VendorReview VendorReview);
        void AddBlogCommentTokens(string storeId, LiquidObject liquidObject, BlogComment blogComment);
        void AddArticleCommentTokens(string storeId, LiquidObject liquidObject, KnowledgebaseArticleComment articleComment);
        void AddNewsCommentTokens(string storeId, LiquidObject liquidObject, NewsComment newsComment);
        void AddProductTokens(LiquidObject liquidObject, Product product, string languageId, string storeId);
        void AddAttributeCombinationTokens(LiquidObject liquidObject, ProductAttributeCombination combination, string languageId);
        void AddForumTokens(LiquidObject liquidObject, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "");
        void AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage);
        void AddBackInStockTokens(LiquidObject liquidObject, BackInStockSubscription subscription);
        void AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid);
        string[] GetListOfCampaignAllowedTokens();
        string[] GetListOfAllowedTokens();
        string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule);
    }
}