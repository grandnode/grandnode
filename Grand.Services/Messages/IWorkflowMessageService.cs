using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Messages;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IWorkflowMessageService
    {
        #region Customer workflow

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerRegisteredNotificationMessage(Customer customer, Store store, string languageId);

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerWelcomeMessage(Customer customer, Store store, string languageId);

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerEmailValidationMessage(Customer customer, Store store, string languageId);

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerPasswordRecoveryMessage(Customer customer, Store store, string languageId);

        /// <summary>
        /// Sends a new customer note added notification to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="customerNote">Customer note</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewCustomerNoteAddedCustomerNotification(CustomerNote customerNote, Customer customer, Store store, string languageId);

        /// <summary>
        /// Send an email token validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerEmailTokenValidationMessage(Customer customer, Store store, string languageId);

        #endregion

        #region Order workflow

        /// <summary>
        /// Sends an order placed notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPlacedVendorNotification(Order order, Vendor vendor, string languageId);

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPlacedStoreOwnerNotification(Order order, string languageId);

        /// <summary>
        /// Sends an order paid notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPaidStoreOwnerNotification(Order order, string languageId);

        /// <summary>
        /// Sends an order paid notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachments">Attachments ident</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPaidCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null);

        /// <summary>
        /// Sends an order paid notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPaidVendorNotification(Order order, Vendor vendor, string languageId);

        /// <summary>
        /// Sends an order placed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachments">Attachments ident</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderPlacedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null);

        /// <summary>
        /// Sends a shipment sent notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendShipmentSentCustomerNotification(Shipment shipment, Order order);

        /// <summary>
        /// Sends a shipment delivered notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendShipmentDeliveredCustomerNotification(Shipment shipment, Order order);

        /// <summary>
        /// Sends an order completed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachments">Attachments ident</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderCompletedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null);

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderCancelledCustomerNotification(Order order, string languageId);

        /// <summary>
        /// Sends an order cancelled notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderCancelledStoreOwnerNotification(Order order, string languageId);

        /// <summary>
        /// Sends an order cancel notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderCancelledVendorNotification(Order order, Vendor vendor, string languageId);

        /// <summary>
        /// Sends an order refunded notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, string languageId);

        /// <summary>
        /// Sends an order refunded notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, string languageId);

        /// <summary>
        /// Sends a new order note added notification to a customer
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderNote">Order note</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewOrderNoteAddedCustomerNotification(Order order, OrderNote orderNote);

        /// <summary>
        /// Sends a "Recurring payment cancelled" notification to a store owner
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, string languageId);

        #endregion

        #region Newsletter workflow

        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription,
            string languageId);

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription,
            string languageId);

        #endregion

        #region Send a message to a friend, ask question

        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendProductEmailAFriendMessage(Customer customer, Store store, string languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage);

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendWishlistEmailAFriendMessage(Customer customer, Store store, string languageId,
             string customerEmail, string friendsEmail, string personalMessage);


        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendProductQuestionMessage(Customer customer, Store store, string languageId,
            Product product, string customerEmail, string fullName, string phone, string message);

        #endregion

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, Order order, string languageId);


        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="order">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, Order order, string languageId);

        /// <summary>
        /// Sends 'New Return Request' message to a customer
        /// </summary>
        /// <param name="returnRequest"></param>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task<int> SendNewReturnRequestCustomerNotification(ReturnRequest returnRequest, Order order, string languageId);

        /// <summary>
        /// Sends a new return request note added notification to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="returnRequestNote">Return request note</param>
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewReturnRequestNoteAddedCustomerNotification(ReturnRequest returnRequest, ReturnRequestNote returnRequestNote, Order order);

        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="topicauthor">Topic author</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewForumTopicMessage(Customer customer, Customer topicauthor,
            ForumTopic forumTopic, Forum forum, string languageId);

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="customer">Post author</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewForumPostMessage(Customer customer, Customer postauthor,
            ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex,
            string languageId);

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendPrivateMessageNotification(PrivateMessage privateMessage, string languageId);

        #endregion

        #region Misc

        /// <summary>
        /// Sends 'New vendor account submitted' message to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vendor">Vendor</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewVendorAccountApplyStoreOwnerNotification(Customer customer, Vendor vendor, Store store, string languageId);


        /// <summary>
        /// Sends 'Vendor information change' message to a store owner
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendVendorInformationChangeNotification(Vendor vendor, Store store, string languageId);

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productReview">Product review</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendProductReviewNotificationMessage(Product product, ProductReview productReview, Store store, string languageId);

        /// <summary>
        /// Sends a vendor review notification message to a store owner
        /// </summary>
        /// <param name="vendorReview">Vendor review</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendVendorReviewNotificationMessage(VendorReview vendorReview, Store store, string languageId);

        /// <summary>
        /// Sends a gift card notification
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendGiftCardNotification(GiftCard giftCard, Order order, string languageId);


        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendQuantityBelowStoreOwnerNotification(Product product, string languageId);

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="combination">Attribute combination</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendQuantityBelowStoreOwnerNotification(Product product, ProductAttributeCombination combination, string languageId);

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewVatSubmittedStoreOwnerNotification(Customer customer, Store store, string vatName, string vatAddress, string languageId);

        /// <summary>
        /// Sends a "customer delete" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerDeleteStoreOwnerNotification(Customer customer, string languageId);

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendBlogCommentNotificationMessage(BlogPost blogPost, BlogComment blogComment, string languageId);

        /// <summary>
        /// Sends an article comment notification message to a store owner
        /// </summary>
        /// <param name="articleComment">Article comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendArticleCommentNotificationMessage(KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, string languageId);

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNewsCommentNotificationMessage(NewsItem newsItem, NewsComment newsComment, string languageId);

        /// <summary>
        /// Sends a 'Back in stock' notification message to a customer
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendBackInStockNotification(Customer customer, Product product, BackInStockSubscription subscription, string languageId);


        /// <summary>
        /// Sends "contact us" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <param name="attrInfo">Attr info</param>
        /// <param name="attrXml">Attr xml</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendContactUsMessage(Customer customer, Store store, string languageId, string senderEmail, string senderName, string subject, string body, string attrInfo, string attrXml);

        /// <summary>
        /// Sends "contact vendor" message
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendContactVendorMessage(Customer customer, Store store, Vendor vendor, string languageId, string senderEmail, string senderName, string subject, string body);

        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="ShoppingCartItem">Item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerActionEvent_AddToCart_Notification(CustomerAction action, ShoppingCartItem cartItem, string languageId, Customer customer);


        /// <summary>
        /// Sends a customer action event - Add order notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="Order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerActionEvent_AddToOrder_Notification(CustomerAction action, Order order, Customer customer, string languageId);


        /// <summary>
        /// Sends a customer action event 
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendCustomerActionEvent_Notification(CustomerAction action, string languageId, Customer customer);

        /// <summary>
        /// Sends auction ended notification to a customer (winner)
        /// </summary>
        /// <param name="product">Auction</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="Bid">Bid</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendAuctionEndedCustomerNotificationWin(Product product, string languageId, Bid bid);

        /// <summary>
        /// Sends auction ended notification to a customer (loser)
        /// </summary>
        /// <param name="product">Auction</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="Bid">Bid</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendAuctionEndedCustomerNotificationLost(Product product, string languageId, Bid bid);

        /// <summary>
        /// Sends auction ended notification to a customer (loser - bin)
        /// </summary>
        /// <param name="product">Auction</param>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendAuctionEndedCustomerNotificationBin(Product product, string customerId, string languageId, string storeId);

        /// <summary>
        /// Sends auction ended notification to a store owner
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Auction</param>
        /// <param name="Bid">Bid</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendAuctionEndedStoreOwnerNotification(Product product, string languageId, Bid bid);

        /// <summary>
        /// Send outbid notification to a customer
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product</param>
        /// <param name="Bid">Bid</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendOutBidCustomerNotification(Product product, string languageId, Bid bid);

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="liquidObject">LiquidObject</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="attachedDownloads">Attached downloads ident</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, string languageId, LiquidObject liquidObject,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            IEnumerable<string> attachedDownloads = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null);

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        Task<int> SendTestEmail(string messageTemplateId, string sendToEmail, LiquidObject liquidObject, string languageId);

        #endregion
    }
}