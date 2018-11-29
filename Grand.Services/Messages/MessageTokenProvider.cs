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
using Grand.Core.Infrastructure;
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
            var allowedTokens = LiquidExtensions.GetTokens(typeof(LiquidAttributeCombination),
                typeof(LiquidAuction),
                typeof(LiquidBackInStockSubscription),
                typeof(LiquidBlogComment),
                typeof(LiquidCustomer),
                typeof(LiquidExtensions),
                typeof(LiquidForum),
                typeof(LiquidGiftCard),
                typeof(LiquidKnowledgebase),
                typeof(LiquidNewsComment),
                typeof(LiquidNewsLetterSubscription),
                typeof(LiquidOrder),
                typeof(LiquidPrivateMessage),
                typeof(LiquidProduct),
                typeof(LiquidProductReview),
                typeof(LiquidReturnRequest),
                typeof(LiquidShipment),
                typeof(LiquidShoppingCart),
                typeof(LiquidStore),
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
            var liquidStore = EngineContext.Current.Resolve<LiquidStore>();
            liquidStore.SetProperties(store, emailAccount);
            liquidObject.Store = liquidStore;
        }

        public void AddOrderTokens(LiquidObject liquidObject, Order order, string languageId,
            string vendorId = "", OrderNote orderNote = null, decimal refundedAmount = 0)
        {
            var liquidOrder = EngineContext.Current.Resolve<LiquidOrder>();
            liquidOrder.SetProperties(order, languageId, vendorId, orderNote, refundedAmount);
            liquidObject.Order = liquidOrder;
        }

        public void AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, string languageId)
        {
            var liquidShipment = EngineContext.Current.Resolve<LiquidShipment>();
            liquidShipment.SetProperties(shipment, languageId);
            liquidObject.Shipment = liquidShipment;
        }

        public void AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment)
        {
            var liquidRecurringPayment = EngineContext.Current.Resolve<LiquidRecurringPayment>();
            liquidRecurringPayment.SetProperties(recurringPayment);
            liquidObject.RecurringPayment = liquidRecurringPayment;
        }

        public void AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Order order)
        {
            var liquidReturnRequest = EngineContext.Current.Resolve<LiquidReturnRequest>();
            liquidReturnRequest.SetProperties(returnRequest, order);
            liquidObject.ReturnRequest = liquidReturnRequest;
        }

        public void AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard)
        {
            var liquidGiftCart = EngineContext.Current.Resolve<LiquidGiftCard>();
            liquidGiftCart.SetProperties(giftCard);
            liquidObject.GiftCard = liquidGiftCart;

        }

        public void AddCustomerTokens(LiquidObject liquidObject, Customer customer, CustomerNote customerNote = null)
        {
            var liquidCustomer = EngineContext.Current.Resolve<LiquidCustomer>();
            liquidCustomer.SetProperties(customer, customerNote);
            liquidObject.Customer = liquidCustomer;
        }

        public void AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, string personalMessage = "", string customerEmail = "")
        {
            var liquidShoppingCart = EngineContext.Current.Resolve<LiquidShoppingCart>();
            liquidShoppingCart.SetProperties(customer, personalMessage, customerEmail);
            liquidObject.ShoppingCart = liquidShoppingCart;
        }

        public void AddVendorTokens(LiquidObject liquidObject, Vendor vendor)
        {
            var liquidVendor = EngineContext.Current.Resolve<LiquidVendor>();
            liquidVendor.SetProperties(vendor);
            liquidObject.Vendor = liquidVendor;
        }

        public void AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription)
        {
            var liquidNewsletterSubscription = EngineContext.Current.Resolve<LiquidNewsLetterSubscription>();
            liquidNewsletterSubscription.SetProperties(subscription);
            liquidObject.NewsletterSubscription = liquidNewsletterSubscription;
        }

        public void AddProductReviewTokens(LiquidObject liquidObject, ProductReview productReview)
        {
            var liquidProductReview = EngineContext.Current.Resolve<LiquidProductReview>();
            liquidProductReview.SetProperties(productReview);
            liquidObject.ProductReview = liquidProductReview;
        }

        public void AddVendorReviewTokens(LiquidObject liquidObject, VendorReview vendorReview)
        {
            var liquidVendorReview = EngineContext.Current.Resolve<LiquidVendorReview>();
            liquidVendorReview.SetProperties(vendorReview);
            liquidObject.VendorReview = liquidVendorReview;
        }

        public void AddBlogCommentTokens(string storeId, LiquidObject liquidObject, BlogComment blogComment)
        {
            var liquidBlogComment = EngineContext.Current.Resolve<LiquidBlogComment>();
            liquidBlogComment.SetProperties(blogComment, storeId);
            liquidObject.BlogComment = liquidBlogComment;
        }

        public void AddArticleCommentTokens(string storeId, LiquidObject liquidObject, KnowledgebaseArticleComment articleComment)
        {
            var liquidKnowledgebase = EngineContext.Current.Resolve<LiquidKnowledgebase>();
            liquidKnowledgebase.SetProperties(articleComment, storeId);
            liquidObject.Knowledgebase = liquidKnowledgebase;
        }

        public void AddNewsCommentTokens(string storeId, LiquidObject liquidObject, NewsComment newsComment)
        {
            var liquidNewsComment = EngineContext.Current.Resolve<LiquidNewsComment>();
            liquidNewsComment.SetProperties(newsComment, storeId);
            liquidObject.NewsComment = liquidNewsComment;
        }

        public void AddProductTokens(LiquidObject liquidObject, Product product, string languageId)
        {
            var liquidProduct = EngineContext.Current.Resolve<LiquidProduct>();
            liquidProduct.SetProperties(product, languageId);
            liquidObject.Product = liquidProduct;
        }

        public void AddAttributeCombinationTokens(LiquidObject liquidObject, ProductAttributeCombination combination, string languageId)
        {
            var liquidAttributeCombination = EngineContext.Current.Resolve<LiquidAttributeCombination>();
            liquidAttributeCombination.SetProperties(combination, languageId);
            liquidObject.AttributeCombination = liquidAttributeCombination;
        }

        public void AddForumTokens(LiquidObject liquidObject, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            var liquidForum = EngineContext.Current.Resolve<LiquidForum>();
            liquidForum.SetProperties(forum, forumTopic, forumPost, friendlyForumTopicPageIndex, appendedPostIdentifierAnchor);
            liquidObject.Forum = liquidForum;
        }

        public void AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage)
        {
            var liquidPrivateMessage = EngineContext.Current.Resolve<LiquidPrivateMessage>();
            liquidPrivateMessage.SetProperties(privateMessage);
            liquidObject.PrivateMessage = liquidPrivateMessage;
        }

        public void AddBackInStockTokens(LiquidObject liquidObject, BackInStockSubscription subscription)
        {
            var liquidBackInStockSubscription = EngineContext.Current.Resolve<LiquidBackInStockSubscription>();
            liquidBackInStockSubscription.SetProperties(subscription);
            liquidObject.BackInStockSubscription = liquidBackInStockSubscription;
        }

        public void AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid)
        {
            var liquidAuction = EngineContext.Current.Resolve<LiquidAuction>();
            liquidAuction.SetProperties(product, bid);
            liquidObject.Auction = liquidAuction;
        }

        #endregion
    }
}