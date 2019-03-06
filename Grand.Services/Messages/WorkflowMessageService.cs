using Grand.Core;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Grand.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ILanguageService languageService,
            ITokenizer tokenizer,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IStoreContext storeContext,
            EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._languageService = languageService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._emailAccountSettings = emailAccountSettings;
            this._commonSettings = commonSettings;
            this._eventPublisher = eventPublisher;
            this._httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utilities

        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName, string storeId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, string languageId)
        {
            var emailAccounId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccounId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;

        }

        protected virtual string EnsureLanguageIsActive(string languageId, string storeId)
        {
            //load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                //load any language
                language = _languageService.GetAllLanguages().FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language.Id;
        }

        #endregion

        #region Methods

        #region Customer workflow

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerRegisteredNotificationMessage(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewCustomer.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerWelcomeMessage(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.WelcomeMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerEmailValidationMessage(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.EmailValidationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerPasswordRecoveryMessage(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.PasswordRecovery", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new customer note added notification to a customer
        /// </summary>
        /// <param name="customerNote">Customer note</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewCustomerNoteAddedCustomerNotification(CustomerNote customerNote, string languageId)
        {
            if (customerNote == null)
                throw new ArgumentNullException("customerNote");

            var messageTemplate = GetActiveMessageTemplate("Customer.NewCustomerNote", "");
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(customerNote.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = string.Format("{0} {1}", customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName), customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Order workflow

        /// <summary>
        /// Sends an order placed notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedVendorNotification(Order order, Vendor vendor, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId, vendorId: vendor.Id);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order paid notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order paid notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachments">Attachments ident</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName,
                attachments);
        }

        /// <summary>
        /// Sends an order paid notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidVendorNotification(Order order, Vendor vendor, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId, vendorId: vendor.Id);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order placed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName,
                attachments);
        }

        /// <summary>
        /// Sends a shipment sent notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentSentCustomerNotification(Shipment shipment, string languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("ShipmentSent.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(liquidObject, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a shipment delivered notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentDeliveredCustomerNotification(Shipment shipment, string languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("ShipmentDelivered.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(liquidObject, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order completed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachments">Attachments ident</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCompletedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderCompleted.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName,
                attachments);
        }

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCancelledCustomerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderCancelled.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order cancelled notification to an admin
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCancelledStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderCancelled.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// Sends an order refunded notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderRefunded.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId, refundedAmount: refundedAmount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }
        /// <summary>
        /// Sends an order refunded notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderRefunded.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId, refundedAmount: refundedAmount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new order note added notification to a customer
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewOrderNoteAddedCustomerNotification(OrderNote orderNote, string languageId)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(orderNote.OrderId);

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.NewOrderNote", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId, orderNote);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "Recurring payment cancelled" notification to a store owner
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, string languageId)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("RecurringPaymentCancelled.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(recurringPayment.InitialOrder.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, recurringPayment.InitialOrder, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddRecurringPaymentTokens(liquidObject, recurringPayment);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Newsletter workflow

        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription,
            string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.ActivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(liquidObject, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription,
            string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.DeactivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(liquidObject, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Send a message to a friend, ask question

        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductEmailAFriendMessage(Customer customer, string languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Service.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, store.Id);
            liquidObject.EmailAFriend = new LiquidEmailAFriend(personalMessage, customerEmail);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendWishlistEmailAFriendMessage(Customer customer, string languageId,
             string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Wishlist.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, personalMessage, customerEmail);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductQuestionMessage(Customer customer, string languageId,
            Product product, string customerEmail, string fullName, string phone, string message)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Service.AskQuestion", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, store.Id);
            liquidObject.AskQuestion = new LiquidAskQuestion(message, customerEmail, fullName, phone);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
                var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

                var subjectReplaced = LiquidExtensions.Render(liquidObject, subject);
                var bodyReplaced = LiquidExtensions.Render(liquidObject, body);

                var contactus = new ContactUs()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                    StoreId = _storeContext.CurrentStore.Id,
                    VendorId = "",
                    Email = customerEmail,
                    FullName = fullName,
                    Subject = subjectReplaced,
                    Enquiry = bodyReplaced,
                    EmailAccountId = emailAccount.Id,
                    IpAddress = EngineContext.Current.Resolve<IWebHelper>().GetCurrentIpAddress()
                };

                EngineContext.Current.Resolve<IContactUsService>().InsertContactUs(contactus);
            }

            var toEmail = emailAccount.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");
            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewReturnRequest.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(returnRequest.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");
            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("ReturnRequestStatusChanged.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(returnRequest.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            string toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends 'New Return Request' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewReturnRequestCustomerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewReturnRequest.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(returnRequest.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumTopicMessage(Customer customer,
            ForumTopic forumTopic, Forum forum, string languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }
            var store = _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Forums.NewForumTopic", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddForumTokens(liquidObject, forum, forumTopic);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumPostMessage(Customer customer,
            ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, string languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

            var store = _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Forums.NewForumPost", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddForumTokens(liquidObject, forum, forumTopic, forumPost, friendlyForumTopicPageIndex);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendPrivateMessageNotification(PrivateMessage privateMessage, string languageId)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            var store = _storeService.GetStoreById(privateMessage.StoreId) ?? _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Customer.NewPM", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddPrivateMessageTokens(liquidObject, privateMessage);
            var tocustomer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(privateMessage.ToCustomerId);
            if (tocustomer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, tocustomer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = tocustomer.Email;
            var toName = tocustomer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

        #endregion

        #region Misc
        /// <summary>
        /// Sends 'New vendor account submitted' message to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVendorAccountApplyStoreOwnerNotification(Customer customer, Vendor vendor, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("VendorAccountApply.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            _messageTokenProvider.AddVendorTokens(liquidObject, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends 'Vendor information changed' message to a store owner
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendVendorInformationChangeNotification(Vendor vendor, string languageId)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("VendorInformationChange.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddVendorTokens(liquidObject, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }


        /// <summary>
        /// Sends a gift card notification
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendGiftCardNotification(GiftCard giftCard, string languageId)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            Store store = null;
            var order = giftCard.PurchasedWithOrderItem != null ?
                EngineContext.Current.Resolve<IOrderService>().GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id) :
                null;
            if (order != null)
                store = _storeService.GetStoreById(order.StoreId);
            if (store == null)
                store = _storeContext.CurrentStore;

            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("GiftCard.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddGiftCardTokens(liquidObject, giftCard);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);
            var toEmail = giftCard.RecipientEmail;
            var toName = giftCard.RecipientName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        /// <param name="productReview">Product review</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductReviewNotificationMessage(ProductReview productReview,
            string languageId)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Product.ProductReview", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(productReview.CustomerId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddProductReviewTokens(liquidObject, productReview);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


        /// <summary>
        /// Sends a vendor review notification message to a store owner
        /// </summary>
        /// <param name="vendorReview">Vendor review</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendVendorReviewNotificationMessage(VendorReview vendorReview,
            string languageId)
        {
            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Vendor.VendorReview", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(vendorReview.CustomerId);
            var vendor = EngineContext.Current.Resolve<IVendorService>().GetVendorById(vendorReview.VendorId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddVendorReviewTokens(liquidObject, vendorReview);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddVendorTokens(liquidObject, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendQuantityBelowStoreOwnerNotification(Product product, string languageId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("QuantityBelow.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, store.Id);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="combination">Attribute combination</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendQuantityBelowStoreOwnerNotification(ProductAttributeCombination combination, string languageId)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("QuantityBelow.AttributeCombination.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(combination.ProductId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, store.Id);
            _messageTokenProvider.AddAttributeCombinationTokens(liquidObject, combination, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVatSubmittedStoreOwnerNotification(Customer customer,
            string vatName, string vatAddress, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewVATSubmitted.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            liquidObject.VatValidationResult = new LiquidVatValidationResult(vatName, vatAddress);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerDeleteStoreOwnerNotification(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("CustomerDelete.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);


            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBlogCommentNotificationMessage(BlogComment blogComment, string languageId)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogComment");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Blog.BlogComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddBlogCommentTokens(store.Id, liquidObject, blogComment);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(blogComment.CustomerId);
            if (customer != null && customer.IsRegistered())
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an article comment notification message to a store owner
        /// </summary>
        /// <param name="articleComment">Article comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendArticleCommentNotificationMessage(KnowledgebaseArticleComment articleComment, string languageId)
        {
            if (articleComment == null)
                throw new ArgumentNullException("articleComment");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Knowledgebase.ArticleComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddArticleCommentTokens(store.Id, liquidObject, articleComment);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(articleComment.CustomerId);
            if (customer != null && customer.IsRegistered())
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsCommentNotificationMessage(NewsComment newsComment, string languageId)
        {
            if (newsComment == null)
                throw new ArgumentNullException("newsComment");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("News.NewsComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddNewsCommentTokens(store.Id, liquidObject, newsComment);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(newsComment.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a 'Back in stock' notification message to a customer
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBackInStockNotification(BackInStockSubscription subscription, string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeService.GetStoreById(subscription.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.BackInStock", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(subscription.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            _messageTokenProvider.AddBackInStockTokens(liquidObject, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends "contact us" message
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendContactUsMessage(Customer customer, string languageId, string senderEmail,
            string senderName, string subject, string body, string attrInfo, string attrXml)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Service.ContactUs", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            string fromEmail;
            string fromName;
            senderName = WebUtility.HtmlEncode(senderName);
            senderEmail = WebUtility.HtmlEncode(senderEmail);
            //required for some SMTP servers
            if (_commonSettings.UseSystemEmailForContactUsForm)
            {
                fromEmail = emailAccount.Email;
                fromName = emailAccount.DisplayName;
            }
            else
            {
                fromEmail = senderEmail;
                fromName = senderName;
            }

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            liquidObject.ContactUs = new LiquidContactUs(senderEmail, senderName, body, attrInfo);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                var contactus = new ContactUs()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                    StoreId = _storeContext.CurrentStore.Id,
                    VendorId = "",
                    Email = senderEmail,
                    FullName = senderName,
                    Subject = String.IsNullOrEmpty(subject) ? "Contact Us (form)" : subject,
                    Enquiry = body,
                    EmailAccountId = emailAccount.Id,
                    IpAddress = EngineContext.Current.Resolve<IWebHelper>().GetCurrentIpAddress(),
                    ContactAttributeDescription = attrInfo,
                    ContactAttributesXml = attrXml
                };
                EngineContext.Current.Resolve<IContactUsService>().InsertContactUs(contactus);
            }



            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                replyToEmailAddress: senderEmail,
                replyToName: senderName);
        }

        /// <summary>
        /// Sends "contact vendor" message
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendContactVendorMessage(Customer customer, Vendor vendor, string languageId, string senderEmail,
            string senderName, string subject, string body)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Service.ContactVendor", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            string fromEmail;
            string fromName;
            senderName = WebUtility.HtmlEncode(senderName);
            senderEmail = WebUtility.HtmlEncode(senderEmail);

            //required for some SMTP servers
            if (_commonSettings.UseSystemEmailForContactUsForm)
            {
                fromEmail = emailAccount.Email;
                fromName = emailAccount.DisplayName;

            }
            else
            {
                fromEmail = senderEmail;
                fromName = senderName;
            }

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            liquidObject.ContactUs = new LiquidContactUs(senderEmail, senderName, body, "");

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                var contactus = new ContactUs()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                    StoreId = _storeContext.CurrentStore.Id,
                    VendorId = vendor.Id,
                    Email = senderEmail,
                    FullName = senderName,
                    Subject = String.IsNullOrEmpty(subject) ? "Contact Us (form)" : subject,
                    Enquiry = body,
                    EmailAccountId = emailAccount.Id,
                    IpAddress = EngineContext.Current.Resolve<IWebHelper>().GetCurrentIpAddress()
                };
                EngineContext.Current.Resolve<IContactUsService>().InsertContactUs(contactus);
            }

            return SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                replyToEmailAddress: senderEmail,
                replyToName: senderName);
        }

        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, string languageId, LiquidObject liquidObject,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            IEnumerable<string> attachedDownloads = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (String.IsNullOrEmpty(toEmailAddress))
                return 0;

            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);

            if (String.IsNullOrEmpty(subject))
                subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);

            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            var subjectReplaced = LiquidExtensions.Render(liquidObject, subject);
            var bodyReplaced = LiquidExtensions.Render(liquidObject, body);

            var attachments = new List<string>();
            if (attachedDownloads != null)
                attachments.AddRange(attachedDownloads);
            if (!string.IsNullOrEmpty(messageTemplate.AttachedDownloadId))
                attachments.Add(messageTemplate.AttachedDownloadId);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloads = attachments,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                     : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return 1;
        }

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="liquidObject">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendTestEmail(string messageTemplateId, string sendToEmail,
            LiquidObject liquidObject, string languageId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                sendToEmail, null);
        }

        #endregion

        #region Customer Action Event

        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="ShoppingCartItem">Item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerActionEvent_AddToCart_Notification(CustomerAction action, ShoppingCartItem cartItem, string languageId, Customer customer)
        {
            if (cartItem == null)
                throw new ArgumentNullException("cartItem");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(cartItem.ProductId);
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, store.Id);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="Order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerActionEvent_AddToOrder_Notification(CustomerAction action, Order order, Customer customer, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(liquidObject, order, languageId);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = string.Empty;
            var toName = string.Empty;

            if (order.BillingAddress != null)
            {
                toEmail = order.BillingAddress.Email;
                toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            }
            else
            {
                if (order.ShippingAddress != null)
                {
                    toEmail = order.ShippingAddress.Email;
                    toName = string.Format("{0} {1}", order.ShippingAddress.FirstName, order.ShippingAddress.LastName);
                }
            }

            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);

        }

        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerActionEvent_Notification(CustomerAction action, string languageId, Customer customer)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        #region Auction notification

        public virtual int SendAuctionEndedCustomerNotificationWin(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(bid.CustomerId);
            if (customer != null)
            {
                if (string.IsNullOrEmpty(languageId))
                {
                    languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                }

                string storeId = bid.StoreId;
                if (string.IsNullOrEmpty(storeId))
                {
                    storeId = _storeContext.CurrentStore.Id;
                }

                languageId = EnsureLanguageIsActive(languageId, storeId);

                var messageTemplate = GetActiveMessageTemplate("AuctionEnded.CustomerNotificationWin", storeId);
                if (messageTemplate == null)
                    return 0;

                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                LiquidObject liquidObject = new LiquidObject();
                _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
                _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, storeId);
                _messageTokenProvider.AddStoreTokens(liquidObject, _storeService.GetStoreById(storeId), emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

                var toEmail = customer.Email;
                var toName = customer.GetFullName();
                return SendNotification(messageTemplate, emailAccount,
                    languageId, liquidObject,
                    toEmail, toName);
            }
            return 0;
        }

        public virtual int SendAuctionEndedCustomerNotificationLost(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customerwin = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(bid.CustomerId);
            if (customerwin != null)
            {
                string storeId = bid.StoreId;
                if (string.IsNullOrEmpty(storeId))
                {
                    storeId = _storeContext.CurrentStore.Id;
                }

                var messageTemplate = GetActiveMessageTemplate("AuctionEnded.CustomerNotificationLost", storeId);
                if (messageTemplate == null)
                    return 0;

                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
                var store = _storeService.GetStoreById(storeId);

                LiquidObject liquidObject = new LiquidObject();
                _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, storeId);
                _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);

                var customerService = EngineContext.Current.Resolve<ICustomerService>();
                var bids = EngineContext.Current.Resolve<IAuctionService>().GetBidsByProductId(bid.ProductId).Where(x => x.CustomerId != bid.CustomerId).GroupBy(x => x.CustomerId);
                foreach (var item in bids)
                {
                    var customer = customerService.GetCustomerById(item.Key);

                    if (string.IsNullOrEmpty(languageId))
                    {
                        languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                    }

                    _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

                    //event notification
                    _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

                    var toEmail = customer.Email;
                    var toName = customer.GetFullName();
                    SendNotification(messageTemplate, emailAccount,
                        languageId, liquidObject,
                        toEmail, toName);
                }
            }
            return 0;
        }

        public virtual int SendAuctionEndedCustomerNotificationBin(Product product, string customerId, string languageId, string storeId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (string.IsNullOrEmpty(storeId))
            {
                storeId = _storeContext.CurrentStore.Id;
            }

            var messageTemplate = GetActiveMessageTemplate("AuctionEnded.CustomerNotificationBin", storeId);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var store = _storeService.GetStoreById(storeId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddProductTokens(liquidObject, product, languageId, storeId);
            _messageTokenProvider.AddStoreTokens(liquidObject, store, emailAccount);

            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var bids = EngineContext.Current.Resolve<IAuctionService>().GetBidsByProductId(product.Id).Where(x => x.CustomerId != customerId).GroupBy(x => x.CustomerId);
            foreach (var item in bids)
            {
                var customer = customerService.GetCustomerById(item.Key);
                if (customer != null)
                {
                    if (string.IsNullOrEmpty(languageId))
                    {
                        languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                    }

                    _messageTokenProvider.AddCustomerTokens(liquidObject, customer);

                    //event notification
                    _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

                    var toEmail = customer.Email;
                    var toName = customer.GetFullName();
                    SendNotification(messageTemplate, emailAccount,
                        languageId, liquidObject,
                        toEmail, toName);
                }
            }

            return 0;
        }
        public virtual int SendAuctionEndedStoreOwnerNotification(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            LiquidObject liquidObject = new LiquidObject();
            MessageTemplate messageTemplate = null;
            EmailAccount emailAccount = null;

            if (bid != null)
            {
                var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(bid.CustomerId);

                if (string.IsNullOrEmpty(languageId))
                {
                    languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                }

                string storeId = bid.StoreId;
                if (string.IsNullOrEmpty(storeId))
                {
                    storeId = _storeContext.CurrentStore.Id;
                }

                languageId = EnsureLanguageIsActive(languageId, storeId);

                messageTemplate = GetActiveMessageTemplate("AuctionEnded.StoreOwnerNotification", storeId);
                if (messageTemplate == null)
                    return 0;

                emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
                _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
                _messageTokenProvider.AddStoreTokens(liquidObject, _storeService.GetStoreById(storeId), emailAccount);
            }
            else
            {
                messageTemplate = GetActiveMessageTemplate("AuctionExpired.StoreOwnerNotification", "");
                if (messageTemplate == null)
                    return 0;

                emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
                _messageTokenProvider.AddProductTokens(liquidObject, product, "", "");
            }

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


        /// <summary>
        /// Send outbid notification to a customer
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product</param>
        /// <param name="Bid">Bid</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOutBidCustomerNotification(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(bid.CustomerId);

            if (string.IsNullOrEmpty(languageId))
            {
                languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
            }

            string storeId = bid.StoreId;
            if (string.IsNullOrEmpty(storeId))
            {
                storeId = _storeContext.CurrentStore.Id;
            }

            languageId = EnsureLanguageIsActive(languageId, storeId);

            var messageTemplate = GetActiveMessageTemplate("BidUp.CustomerNotification", storeId);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            LiquidObject liquidObject = new LiquidObject();
            _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
            _messageTokenProvider.AddCustomerTokens(liquidObject, customer);
            _messageTokenProvider.AddStoreTokens(liquidObject, _storeService.GetStoreById(storeId), emailAccount);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }
        #endregion

        #endregion
    }
}
