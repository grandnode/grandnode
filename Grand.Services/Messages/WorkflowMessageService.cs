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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens, 
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddCustomerNoteTokens(tokens, customerNote);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = string.Format("{0} {1}", customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName), customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);
            if(customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order paid notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            string attachmentFilePath = null, string attachmentFileName = null)
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order completed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCompletedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderNoteTokens(tokens, orderNote);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            tokens.Add(new Token("EmailAFriend.PersonalMessage", personalMessage, true));
            tokens.Add(new Token("EmailAFriend.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            tokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            tokens.Add(new Token("Wishlist.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            tokens.Add(new Token("AskQuestion.Message", message, true));
            tokens.Add(new Token("AskQuestion.Email", customerEmail));
            tokens.Add(new Token("AskQuestion.FullName", fullName));
            tokens.Add(new Token("AskQuestion.Phone", phone));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
                var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);
                //Replace subject and body tokens 
                var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
                var bodyReplaced = _tokenizer.Replace(body, tokens, true);

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
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            string toEmail = customer.IsGuest() ? 
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, order);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumTopic);
            _messageTokenProvider.AddForumTokens(tokens, forum);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
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
            if (messageTemplate == null )
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddForumPostTokens(tokens, forumPost);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumTopic,
                friendlyForumTopicPageIndex, forumPost.Id);
            _messageTokenProvider.AddForumTokens(tokens, forum);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
          
            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
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
            if (messageTemplate == null )
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddPrivateMessageTokens(tokens, privateMessage);
            var tocustomer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(privateMessage.ToCustomerId);
            if (tocustomer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, tocustomer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
           
            var toEmail = tocustomer.Email;
            var toName = tocustomer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddVendorTokens(tokens, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddVendorTokens(tokens, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddGiftCardTokens(tokens, giftCard);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
            var toEmail = giftCard.RecipientEmail;
            var toName = giftCard.RecipientName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductReviewTokens(tokens, productReview);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddVendorReviewTokens(tokens, vendorReview);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddVendorTokens(tokens, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendQuantityBelowStoreOwnerNotification(Product product,  string languageId)
        {
            if (product== null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("QuantityBelow.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            _messageTokenProvider.AddAttributeCombinationTokens(tokens, combination, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            tokens.Add(new Token("VatValidationResult.Name", vatName));
            tokens.Add(new Token("VatValidationResult.Address", vatAddress));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);


            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddBlogCommentTokens(store.Id, tokens, blogComment);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(blogComment.CustomerId);
            if (customer != null && customer.IsRegistered())
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddArticleCommentTokens(store.Id, tokens, articleComment);

            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(articleComment.CustomerId);
            if (customer != null && customer.IsRegistered())
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsCommentTokens(store.Id, tokens, newsComment);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(newsComment.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(subscription.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            _messageTokenProvider.AddBackInStockTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
            
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            tokens.Add(new Token("ContactUs.SenderEmail", senderEmail));
            tokens.Add(new Token("ContactUs.SenderName", senderName));
            tokens.Add(new Token("ContactUs.Body", body, true));
            tokens.Add(new Token("ContactUs.AttributeDescription", attrInfo, true));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

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



            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            tokens.Add(new Token("ContactUs.SenderEmail", senderEmail));
            tokens.Add(new Token("ContactUs.SenderName", senderName));
            tokens.Add(new Token("ContactUs.Body", body, true));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

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

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                replyToEmailAddress: senderEmail,
                replyToName: senderName);
        }

        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, string languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
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

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
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
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
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
        /// <param name="tokens">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendTestEmail(string messageTemplateId, string sendToEmail, 
            List<Token> tokens, string languageId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(cartItem.ProductId);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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
            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

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
                languageId, tokens,
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

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Auction notification

        public virtual int SendAuctionEndedCustomerNotificationWin(Product product, string languageId, Bid bid)
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

            var messageTemplate = GetActiveMessageTemplate("AuctionEnded.CustomerNotificationWin", storeId);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>();
            _messageTokenProvider.AddAuctionTokens(tokens, product, bid);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            _messageTokenProvider.AddStoreTokens(tokens, _storeService.GetStoreById(storeId), emailAccount);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        public virtual int SendAuctionEndedCustomerNotificationLost(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string storeId = bid.StoreId;
            if (string.IsNullOrEmpty(storeId))
            {
                storeId = _storeContext.CurrentStore.Id;
            }

            var messageTemplate = GetActiveMessageTemplate("AuctionEnded.CustomerNotificationLost", storeId);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var tokensDefault = new List<Token>();
            var store = _storeService.GetStoreById(storeId);

            _messageTokenProvider.AddAuctionTokens(tokensDefault, product, bid);
            _messageTokenProvider.AddProductTokens(tokensDefault, product, languageId);
            _messageTokenProvider.AddStoreTokens(tokensDefault, store, emailAccount);

            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var bids = EngineContext.Current.Resolve<IAuctionService>().GetBidsByProductId(bid.ProductId).Where(x=>x.CustomerId != bid.CustomerId).GroupBy(x=>x.CustomerId);
            foreach (var item in bids)
            {
                var customer = customerService.GetCustomerById(item.Key);

                if (string.IsNullOrEmpty(languageId))
                {
                    languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                }

                var tokens = new List<Token>();
                tokens.AddRange(tokensDefault);
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = customer.GetFullName();
                SendNotification(messageTemplate, emailAccount,
                    languageId, tokens,
                    toEmail, toName);
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
            var tokensDefault = new List<Token>();
            var store = _storeService.GetStoreById(storeId);

            _messageTokenProvider.AddProductTokens(tokensDefault, product, languageId);
            _messageTokenProvider.AddStoreTokens(tokensDefault, store, emailAccount);

            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var bids = EngineContext.Current.Resolve<IAuctionService>().GetBidsByProductId(product.Id).Where(x=>x.CustomerId != customerId).GroupBy(x => x.CustomerId);
            foreach (var item in bids)
            {
                var customer = customerService.GetCustomerById(item.Key);
                if (customer != null)
                {
                    if (string.IsNullOrEmpty(languageId))
                    {
                        languageId = customer.GetAttribute<string>(SystemCustomerAttributeNames.LanguageId);
                    }

                    var tokens = new List<Token>();
                    tokens.AddRange(tokensDefault);
                    _messageTokenProvider.AddCustomerTokens(tokens, customer);

                    //event notification
                    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                    var toEmail = customer.Email;
                    var toName = customer.GetFullName();
                    SendNotification(messageTemplate, emailAccount,
                        languageId, tokens,
                        toEmail, toName);
                }
            }

            return 0;
        }
        public virtual int SendAuctionEndedStoreOwnerNotification(Product product, string languageId, Bid bid)
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

            var messageTemplate = GetActiveMessageTemplate("AuctionEnded.StoreOwnerNotification", storeId);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>();
            _messageTokenProvider.AddAuctionTokens(tokens, product, bid);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddStoreTokens(tokens, _storeService.GetStoreById(storeId), emailAccount);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
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

            var tokens = new List<Token>();
            _messageTokenProvider.AddAuctionTokens(tokens, product, bid);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddStoreTokens(tokens, _storeService.GetStoreById(storeId), emailAccount);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }
        #endregion

        #endregion
    }
}
