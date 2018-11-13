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
using System;
using System.Collections.Generic;

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

        public void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            throw new NotImplementedException();
        }

        public void AddOrderTokens(IList<Token> tokens, Order order, string languageId, string vendorId = "")
        {
            throw new NotImplementedException();
        }

        public void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount)
        {
            throw new NotImplementedException();
        }

        public void AddShipmentTokens(IList<Token> tokens, Shipment shipment, string languageId)
        {
            throw new NotImplementedException();
        }

        public void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote)
        {
            throw new NotImplementedException();
        }

        public void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment)
        {
            throw new NotImplementedException();
        }

        public void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, Order orderItem)
        {
            throw new NotImplementedException();
        }

        public void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard)
        {
            throw new NotImplementedException();
        }

        public void AddCustomerTokens(IList<Token> tokens, Customer customer)
        {
            throw new NotImplementedException();
        }

        public void AddCustomerNoteTokens(IList<Token> tokens, CustomerNote customerNote)
        {
            throw new NotImplementedException();
        }

        public void AddShoppingCartTokens(IList<Token> tokens, Customer customer)
        {
            throw new NotImplementedException();
        }

        public void AddRecommendedProductsTokens(IList<Token> tokens, Customer customer)
        {
            throw new NotImplementedException();
        }

        public void AddRecentlyViewedProductsTokens(IList<Token> tokens, Customer customer)
        {
            throw new NotImplementedException();
        }

        public void AddVendorTokens(IList<Token> tokens, Vendor vendor)
        {
            throw new NotImplementedException();
        }

        public void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        {
            throw new NotImplementedException();
        }

        public void AddVendorReviewTokens(IList<Token> tokens, VendorReview VendorReview)
        {
            throw new NotImplementedException();
        }

        public void AddBlogCommentTokens(string storeId, IList<Token> tokens, BlogComment blogComment)
        {
            throw new NotImplementedException();
        }

        public void AddArticleCommentTokens(string storeId, IList<Token> tokens, KnowledgebaseArticleComment articleComment)
        {
            throw new NotImplementedException();
        }

        public void AddNewsCommentTokens(string storeId, IList<Token> tokens, NewsComment newsComment)
        {
            throw new NotImplementedException();
        }

        public void AddProductTokens(IList<Token> tokens, Product product, string languageId)
        {
            throw new NotImplementedException();
        }

        public void AddAttributeCombinationTokens(IList<Token> tokens, ProductAttributeCombination combination, string languageId)
        {
            throw new NotImplementedException();
        }

        public void AddForumTokens(IList<Token> tokens, Forum forum)
        {
            throw new NotImplementedException();
        }

        public void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic, int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            throw new NotImplementedException();
        }

        public void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        {
            throw new NotImplementedException();
        }

        public void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        {
            throw new NotImplementedException();
        }

        public void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public void AddAuctionTokens(IList<Token> tokens, Product product, Bid bid)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}