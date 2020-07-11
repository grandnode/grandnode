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
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Events.Extensions;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public MessageTokenProvider(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = LiquidExtensions.GetTokens(typeof(LiquidStore),
                typeof(LiquidNewsLetterSubscription),
                typeof(LiquidShoppingCart),
                typeof(LiquidCustomer));

            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = LiquidExtensions.GetTokens(
                typeof(LiquidAskQuestion),
                typeof(LiquidAttributeCombination),
                typeof(LiquidAuctions),
                typeof(LiquidBackInStockSubscription),
                typeof(LiquidBlogComment),
                typeof(LiquidContactUs),
                typeof(LiquidCustomer),
                typeof(LiquidEmailAFriend),
                typeof(LiquidForums),
                typeof(LiquidGiftCard),
                typeof(LiquidKnowledgebase),
                typeof(LiquidNewsComment),
                typeof(LiquidNewsLetterSubscription),
                typeof(LiquidOrder),
                typeof(LiquidOrderItem),
                typeof(LiquidPrivateMessage),
                typeof(LiquidProduct),
                typeof(LiquidProductReview),
                typeof(RecurringPayment),
                typeof(LiquidReturnRequest),
                typeof(LiquidShipment),
                typeof(LiquidShipmentItem),
                typeof(LiquidShoppingCart),
                typeof(LiquidStore),
                typeof(LiquidVatValidationResult),
                typeof(LiquidVendor),
                typeof(LiquidVendorReview));

            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = LiquidExtensions.GetTokens(typeof(LiquidStore));

            if (rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidShoppingCart)));
            }

            if (rule == CustomerReminderRuleEnum.CompletedOrder || rule == CustomerReminderRuleEnum.UnpaidOrder)
            {
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidOrder)));
            }

            allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidCustomer)));

            return allowedTokens.ToArray();
        }

        public async Task AddStoreTokens(LiquidObject liquidObject, Store store, Language language, EmailAccount emailAccount)
        {
            var liquidStore = await _mediator.Send(new GetStoreTokensCommand() { Store = store, Language = language, EmailAccount = emailAccount });
            liquidObject.Store = liquidStore;
            await _mediator.EntityTokensAdded(store, liquidStore, liquidObject);
        }

        public async Task AddOrderTokens(LiquidObject liquidObject, Order order, Customer customer, Store store, OrderNote orderNote = null, Vendor vendor = null, decimal refundedAmount = 0)
        {
            var liquidOrder = await _mediator.Send(new GetOrderTokensCommand() {
                Order = order,
                Customer = customer,
                Vendor = vendor,
                Store = store,
                OrderNote = orderNote,
                RefundedAmount = refundedAmount
            });
            liquidObject.Order = liquidOrder;
            await _mediator.EntityTokensAdded(order, liquidOrder, liquidObject);
        }

        public async Task AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, Order order, Store store, Language language)
        {
            var liquidShipment = await _mediator.Send(new GetShipmentTokensCommand() { Shipment = shipment, Order = order, Store = store, Language = language });
            liquidObject.Shipment = liquidShipment;
            await _mediator.EntityTokensAdded(shipment, liquidShipment, liquidObject);
        }

        public async Task AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment)
        {
            var liquidRecurringPayment = new LiquidRecurringPayment(recurringPayment);
            liquidObject.RecurringPayment = liquidRecurringPayment;
            await _mediator.EntityTokensAdded(recurringPayment, liquidRecurringPayment, liquidObject);
        }

        public async Task AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Store store, Order order, Language language, ReturnRequestNote returnRequestNote = null)
        {
            var liquidReturnRequest = await _mediator.Send(new GetReturnRequestTokensCommand() { Order = order, Language = language, ReturnRequest = returnRequest, ReturnRequestNote = returnRequestNote, Store = store });
            liquidObject.ReturnRequest = liquidReturnRequest;

            await _mediator.EntityTokensAdded(returnRequest, liquidReturnRequest, liquidObject);
        }

        public async Task AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard)
        {
            var liquidGiftCart = await _mediator.Send(new GetGiftCardTokensCommand() { GiftCard = giftCard });
            liquidObject.GiftCard = liquidGiftCart;
            await _mediator.EntityTokensAdded(giftCard, liquidGiftCart, liquidObject);
        }

        public async Task AddCustomerTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, CustomerNote customerNote = null)
        {
            var liquidCustomer = new LiquidCustomer(customer, store, customerNote);
            liquidObject.Customer = liquidCustomer;

            await _mediator.EntityTokensAdded(customer, liquidCustomer, liquidObject);
            if (customerNote != null)
                await _mediator.EntityTokensAdded(customerNote, liquidCustomer, liquidObject);
        }

        public async Task AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, Store store, Language language,
            string personalMessage = "", string customerEmail = "")
        {
            var liquidShoppingCart = await _mediator.Send(new GetShoppingCartTokensCommand() {
                Customer = customer,
                CustomerEmail = customerEmail,
                Language = language,
                PersonalMessage = personalMessage,
                Store = store
            });
            liquidObject.ShoppingCart = liquidShoppingCart;

            await _mediator.EntityTokensAdded(customer, liquidShoppingCart, liquidObject);
        }

        public async Task AddVendorTokens(LiquidObject liquidObject, Vendor vendor, Language language)
        {
            var liquidVendor = await _mediator.Send(new GetVendorTokensCommand() { Vendor = vendor, Language = language });
            liquidObject.Vendor = liquidVendor;
            await _mediator.EntityTokensAdded(vendor, liquidVendor, liquidObject);
        }

        public async Task AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription, Store store)
        {
            var liquidNewsletterSubscription = new LiquidNewsLetterSubscription(subscription, store);
            liquidObject.NewsLetterSubscription = liquidNewsletterSubscription;
            await _mediator.EntityTokensAdded(subscription, liquidNewsletterSubscription, liquidObject);
        }

        public async Task AddProductReviewTokens(LiquidObject liquidObject, Product product, ProductReview productReview)
        {
            var liquidProductReview = new LiquidProductReview(product, productReview);
            liquidObject.ProductReview = liquidProductReview;
            await _mediator.EntityTokensAdded(productReview, liquidProductReview, liquidObject);
        }

        public async Task AddVendorReviewTokens(LiquidObject liquidObject, Vendor vendor, VendorReview vendorReview)
        {
            var liquidVendorReview = new LiquidVendorReview(vendor, vendorReview);
            liquidObject.VendorReview = liquidVendorReview;
            await _mediator.EntityTokensAdded(vendorReview, liquidVendorReview, liquidObject);
        }

        public async Task AddBlogCommentTokens(LiquidObject liquidObject, BlogPost blogPost, BlogComment blogComment, Store store, Language language)
        {
            var liquidBlogComment = new LiquidBlogComment(blogComment, blogPost, store, language);
            liquidObject.BlogComment = liquidBlogComment;
            await _mediator.EntityTokensAdded(blogComment, liquidBlogComment, liquidObject);
        }

        public async Task AddArticleCommentTokens(LiquidObject liquidObject, KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, Store store, Language language)
        {
            var liquidKnowledgebase = new LiquidKnowledgebase(article, articleComment, store, language);
            liquidObject.Knowledgebase = liquidKnowledgebase;
            await _mediator.EntityTokensAdded(articleComment, liquidKnowledgebase, liquidObject);
        }

        public async Task AddNewsCommentTokens(LiquidObject liquidObject, NewsItem newsItem, NewsComment newsComment, Store store, Language language)
        {
            var liquidNewsComment = new LiquidNewsComment(newsItem, newsComment, store, language);
            liquidObject.NewsComment = liquidNewsComment;
            await _mediator.EntityTokensAdded(newsComment, liquidNewsComment, liquidObject);
        }

        public async Task AddProductTokens(LiquidObject liquidObject, Product product, Language language, Store store)
        {
            var liquidProduct = new LiquidProduct(product, language, store);
            liquidObject.Product = liquidProduct;
            await _mediator.EntityTokensAdded(product, liquidProduct, liquidObject);
        }

        public async Task AddAttributeCombinationTokens(LiquidObject liquidObject, Product product, ProductAttributeCombination combination)
        {
            var liquidAttributeCombination = await _mediator.Send(new GetAttributeCombinationTokensCommand() { Product = product, Combination = combination });
            liquidObject.AttributeCombination = liquidAttributeCombination;
            await _mediator.EntityTokensAdded(combination, liquidAttributeCombination, liquidObject);
        }

        public async Task AddForumTokens(LiquidObject liquidObject, Customer customer, Store store, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            var liquidForum = new LiquidForums(forum, forumTopic, forumPost, customer, store, friendlyForumTopicPageIndex, appendedPostIdentifierAnchor);
            liquidObject.Forums = liquidForum;
            await _mediator.EntityTokensAdded(forum, liquidForum, liquidObject);
            await _mediator.EntityTokensAdded(forumTopic, liquidForum, liquidObject);
            await _mediator.EntityTokensAdded(forumPost, liquidForum, liquidObject);
        }

        public async Task AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage)
        {
            var liquidPrivateMessage = new LiquidPrivateMessage(privateMessage);
            liquidObject.PrivateMessage = liquidPrivateMessage;

            await _mediator.EntityTokensAdded(privateMessage, liquidPrivateMessage, liquidObject);
        }

        public async Task AddBackInStockTokens(LiquidObject liquidObject, Product product, BackInStockSubscription subscription, Store store, Language language)
        {
            var liquidBackInStockSubscription = new LiquidBackInStockSubscription(product, subscription, store, language);
            liquidObject.BackInStockSubscription = liquidBackInStockSubscription;
            await _mediator.EntityTokensAdded(subscription, liquidBackInStockSubscription, liquidObject);
        }

        public async Task AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid)
        {
            var liquidAuctions = await _mediator.Send(new GetAuctionTokensCommand() { Product = product, Bid = bid });
            liquidObject.Auctions = liquidAuctions;
            await _mediator.EntityTokensAdded(bid, liquidAuctions, liquidObject);
        }

        #endregion
    }
}