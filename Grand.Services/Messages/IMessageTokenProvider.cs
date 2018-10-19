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
using System.Collections.Generic;

namespace Grand.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount);
        void AddOrderTokens(IList<Token> tokens, Order order, string languageId, string vendorId = "");
        void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount);
        void AddShipmentTokens(IList<Token> tokens, Shipment shipment, string languageId);
        void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote);
        void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment);
        void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, Order orderItem);
        void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard);
        void AddCustomerTokens(IList<Token> tokens, Customer customer);
        void AddCustomerNoteTokens(IList<Token> tokens, CustomerNote customerNote);
        void AddShoppingCartTokens(IList<Token> tokens, Customer customer);
        void AddRecommendedProductsTokens(IList<Token> tokens, Customer customer);
        void AddRecentlyViewedProductsTokens(IList<Token> tokens, Customer customer);
        void AddVendorTokens(IList<Token> tokens, Vendor vendor);
        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);
        void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview);
        void AddVendorReviewTokens(IList<Token> tokens, VendorReview VendorReview);
        void AddBlogCommentTokens(string storeId, IList<Token> tokens, BlogComment blogComment);
        void AddArticleCommentTokens(string storeId, IList<Token> tokens, KnowledgebaseArticleComment articleComment);
        void AddNewsCommentTokens(string storeId, IList<Token> tokens, NewsComment newsComment);
        void AddProductTokens(IList<Token> tokens, Product product, string languageId);
        void AddAttributeCombinationTokens(IList<Token> tokens, ProductAttributeCombination combination, string languageId);
        void AddForumTokens(IList<Token> tokens, Forum forum);
        void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "");
        void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost);
        void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage);
        void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription);
        void AddAuctionTokens(IList<Token> tokens, Product product, Bid bid);
        string[] GetListOfCampaignAllowedTokens();
        string[] GetListOfAllowedTokens();
        string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule);
    }
}
