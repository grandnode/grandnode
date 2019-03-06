using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Stores;

namespace Grand.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IOrderService orderService,
            IPaymentService paymentService,
            IStoreService storeService,
            IStoreContext storeContext,
            IProductAttributeParser productAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICustomerAttributeFormatter customerAttributeFormatter,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            TaxSettings taxSettings,
            CurrencySettings currencySettings,
            ShippingSettings shippingSettings,
            StoreInformationSettings storeInformationSettings,
            MediaSettings mediaSettings,
            IEventPublisher eventPublisher)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._downloadService = downloadService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._productAttributeParser = productAttributeParser;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._customerAttributeFormatter = customerAttributeFormatter;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._shippingSettings = shippingSettings;
            this._templatesSettings = templatesSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._storeInformationSettings = storeInformationSettings;
            this._mediaSettings = mediaSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Utilities

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
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidStore)));
            }

            if (rule == CustomerReminderRuleEnum.CompletedOrder || rule == CustomerReminderRuleEnum.UnpaidOrder)
            {
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidOrder)));
            }

            allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidCustomer)));

            return allowedTokens.ToArray();
        }

        public void AddStoreTokens(LiquidObject liquidObject, Store store, EmailAccount emailAccount)
        {
            var liquidStore = new LiquidStore(store, emailAccount);
            liquidObject.Store = liquidStore;

            _eventPublisher.EntityTokensAdded(store, liquidStore, liquidObject);
        }

        public void AddOrderTokens(LiquidObject liquidObject, Order order, string languageId, OrderNote orderNote = null, string vendorId = "", decimal refundedAmount = 0)
        {
            var liquidOrder = new LiquidOrder(order, languageId, orderNote, vendorId, refundedAmount);
            liquidObject.Order = liquidOrder;

            _eventPublisher.EntityTokensAdded(order, liquidOrder, liquidObject);
        }

        public void AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, string languageId)
        {
            var liquidShipment = new LiquidShipment(shipment, languageId);
            liquidObject.Shipment = liquidShipment;

            _eventPublisher.EntityTokensAdded(shipment, liquidShipment, liquidObject);
        }

        public void AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment)
        {
            var liquidRecurringPayment = new LiquidRecurringPayment(recurringPayment);
            liquidObject.RecurringPayment = liquidRecurringPayment;

            _eventPublisher.EntityTokensAdded(recurringPayment, liquidRecurringPayment, liquidObject);
        }

        public void AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Order order)
        {
            var liquidReturnRequest = new LiquidReturnRequest(returnRequest, order);
            liquidObject.ReturnRequest = liquidReturnRequest;

            _eventPublisher.EntityTokensAdded(returnRequest, liquidReturnRequest, liquidObject);
        }

        public void AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard)
        {
            var liquidGiftCart = new LiquidGiftCard(giftCard);
            liquidObject.GiftCard = liquidGiftCart;

            _eventPublisher.EntityTokensAdded(giftCard, liquidGiftCart, liquidObject);
        }

        public void AddCustomerTokens(LiquidObject liquidObject, Customer customer, CustomerNote customerNote = null)
        {
            var liquidCustomer = new LiquidCustomer(customer, customerNote);
            liquidObject.Customer = liquidCustomer;

            _eventPublisher.EntityTokensAdded(customer, liquidCustomer, liquidObject);
            _eventPublisher.EntityTokensAdded(customerNote, liquidCustomer, liquidObject);
        }

        public void AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, string personalMessage = "", string customerEmail = "")
        {
            var liquidShoppingCart = new LiquidShoppingCart(customer, personalMessage, customerEmail);
            liquidObject.ShoppingCart = liquidShoppingCart;

            _eventPublisher.EntityTokensAdded(customer, liquidShoppingCart, liquidObject);
        }

        public void AddVendorTokens(LiquidObject liquidObject, Vendor vendor)
        {
            var liquidVendor = new LiquidVendor(vendor);
            liquidObject.Vendor = liquidVendor;

            _eventPublisher.EntityTokensAdded(vendor, liquidVendor, liquidObject);
        }

        public void AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription)
        {
            var liquidNewsletterSubscription = new LiquidNewsLetterSubscription(subscription);
            liquidObject.NewsLetterSubscription = liquidNewsletterSubscription;

            _eventPublisher.EntityTokensAdded(subscription, liquidNewsletterSubscription, liquidObject);
        }

        public void AddProductReviewTokens(LiquidObject liquidObject, ProductReview productReview)
        {
            var liquidProductReview = new LiquidProductReview(productReview);
            liquidObject.ProductReview = liquidProductReview;

            _eventPublisher.EntityTokensAdded(productReview, liquidProductReview, liquidObject);
        }

        public void AddVendorReviewTokens(LiquidObject liquidObject, VendorReview vendorReview)
        {
            var liquidVendorReview = new LiquidVendorReview(vendorReview);
            liquidObject.VendorReview = liquidVendorReview;

            _eventPublisher.EntityTokensAdded(vendorReview, liquidVendorReview, liquidObject);
        }

        public void AddBlogCommentTokens(string storeId, LiquidObject liquidObject, BlogComment blogComment)
        {
            var liquidBlogComment = new LiquidBlogComment(blogComment, storeId);
            liquidObject.BlogComment = liquidBlogComment;

            _eventPublisher.EntityTokensAdded(blogComment, liquidBlogComment, liquidObject);
        }

        public void AddArticleCommentTokens(string storeId, LiquidObject liquidObject, KnowledgebaseArticleComment articleComment)
        {
            var liquidKnowledgebase = new LiquidKnowledgebase(articleComment, storeId);
            liquidObject.Knowledgebase = liquidKnowledgebase;

            _eventPublisher.EntityTokensAdded(articleComment, liquidKnowledgebase, liquidObject);
        }

        public void AddNewsCommentTokens(string storeId, LiquidObject liquidObject, NewsComment newsComment)
        {
            var liquidNewsComment = new LiquidNewsComment(newsComment, storeId);
            liquidObject.NewsComment = liquidNewsComment;

            _eventPublisher.EntityTokensAdded(newsComment, liquidNewsComment, liquidObject);
        }

        public void AddProductTokens(LiquidObject liquidObject, Product product, string languageId, string storeId)
        {
            var liquidProduct = new LiquidProduct(product, languageId, storeId);
            liquidObject.Product = liquidProduct;

            _eventPublisher.EntityTokensAdded(product, liquidProduct, liquidObject);
        }

        public void AddAttributeCombinationTokens(LiquidObject liquidObject, ProductAttributeCombination combination, string languageId)
        {
            var liquidAttributeCombination = new LiquidAttributeCombination(combination, languageId);
            liquidObject.AttributeCombination = liquidAttributeCombination;

            _eventPublisher.EntityTokensAdded(combination, liquidAttributeCombination, liquidObject);
        }

        public void AddForumTokens(LiquidObject liquidObject, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            var liquidForum = new LiquidForums(forum, forumTopic, forumPost, friendlyForumTopicPageIndex, appendedPostIdentifierAnchor);
            liquidObject.Forums = liquidForum;

            _eventPublisher.EntityTokensAdded(forum, liquidForum, liquidObject);
            _eventPublisher.EntityTokensAdded(forumTopic, liquidForum, liquidObject);
            _eventPublisher.EntityTokensAdded(forumPost, liquidForum, liquidObject);
        }

        public void AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage)
        {
            var liquidPrivateMessage = new LiquidPrivateMessage(privateMessage);
            liquidObject.PrivateMessage = liquidPrivateMessage;

            _eventPublisher.EntityTokensAdded(privateMessage, liquidPrivateMessage, liquidObject);
        }

        public void AddBackInStockTokens(LiquidObject liquidObject, BackInStockSubscription subscription)
        {
            var liquidBackInStockSubscription = new LiquidBackInStockSubscription(subscription);
            liquidObject.BackInStockSubscription = liquidBackInStockSubscription;

            _eventPublisher.EntityTokensAdded(subscription, liquidBackInStockSubscription, liquidObject);
        }

        public void AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid)
        {
            var liquidAuctions = new LiquidAuctions(product, bid);
            liquidObject.Auctions = liquidAuctions;

            _eventPublisher.EntityTokensAdded(bid, liquidAuctions, liquidObject);
        }

        #endregion
    }
}