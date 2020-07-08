using Grand.Core;
using Grand.Domain;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
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
using Grand.Services.Commands.Models.Common;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Events.Extensions;
using Grand.Services.Localization;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Queries.Models.Catalog;
using Grand.Services.Queries.Models.Customers;
using Grand.Services.Stores;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly IMediator _mediator;

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CommonSettings _commonSettings;

        #endregion

        #region Ctor

        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ILanguageService languageService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IMediator mediator,
            EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings)
        {
            _messageTemplateService = messageTemplateService;
            _queuedEmailService = queuedEmailService;
            _languageService = languageService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _storeService = storeService;
            _emailAccountSettings = emailAccountSettings;
            _commonSettings = commonSettings;
            _mediator = mediator;
        }

        #endregion

        #region Utilities

        protected virtual async Task<Store> GetStore(string storeId)
        {
            return await _storeService.GetStoreById(storeId) ?? (await _storeService.GetAllStores()).FirstOrDefault();
        }

        protected virtual async Task<MessageTemplate> GetMessageTemplate(string messageTemplateName, string storeId)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, string languageId)
        {
            var emailAccounId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            var emailAccount = await _emailAccountService.GetEmailAccountById(emailAccounId);
            if (emailAccount == null)
                emailAccount = await _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccounts()).FirstOrDefault();
            return emailAccount;

        }

        protected virtual async Task<Language> EnsureLanguageIsActive(string languageId, string storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguages(storeId: storeId)).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguages()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language;
        }

        #endregion

        #region Methods

        #region Customer workflow

        /// <summary>
        /// Send a message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="templateName">Message template name</param>
        /// <param name="toEmailAccount">Send email to email account</param>
        /// <param name="customerNote">Customer note</param>
        /// <returns>Queued email identifier</returns>
        protected virtual async Task<int> SendCustomerMessage(Customer customer, Store store, string languageId, string templateName, bool toEmailAccount = false, CustomerNote customerNote = null)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate(templateName, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language, customerNote);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = toEmailAccount ? emailAccount.Email : customer.Email;
            var toName = toEmailAccount ? emailAccount.DisplayName : customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store identifier</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerRegisteredNotificationMessage(Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerRegistered, true);
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerWelcomeMessage(Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerWelcome);
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerEmailValidationMessage(Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerEmailValidation);
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerPasswordRecoveryMessage(Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerPasswordRecovery);
        }

        /// <summary>
        /// Sends a new customer note added notification to a customer
        /// </summary>
        /// <param name="customerNote">Customer note</param>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewCustomerNoteAddedCustomerNotification(CustomerNote customerNote, Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerNewCustomerNote, customerNote: customerNote);
        }

        /// <summary>
        /// Send an email token validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="store">Store instance</param>
        /// <param name="token">Token</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerEmailTokenValidationMessage(Customer customer, Store store, string languageId)
        {
            return await SendCustomerMessage(customer, store, languageId, MessageTemplateNames.CustomerEmailTokenValidationMessage);
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
        public virtual async Task<int> SendOrderPlacedVendorNotification(Order order, Vendor vendor, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            LiquidObject liquidObject = new LiquidObject();
            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            var messageTemplate = await GetMessageTemplate("OrderPlaced.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, vendor: vendor);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);


            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendOrderPlacedStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderPlaced.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);

            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order paid notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendOrderPaidStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderPaid.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);

            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOrderPaidCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderPaid.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);

            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOrderPaidVendorNotification(Order order, Vendor vendor, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderPaid.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, vendor: vendor);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOrderPlacedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderPlaced.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
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
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendShipmentSentCustomerNotification(Shipment shipment, Order order)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(order.CustomerLanguageId, store.Id);

            var messageTemplate = await GetMessageTemplate("ShipmentSent.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddShipmentTokens(liquidObject, shipment, order, store, language);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
                language.Id, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a shipment delivered notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendShipmentDeliveredCustomerNotification(Shipment shipment, Order order)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(order.CustomerLanguageId, store.Id);

            var messageTemplate = await GetMessageTemplate("ShipmentDelivered.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddShipmentTokens(liquidObject, shipment, order, store, language);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);

            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
                language.Id, liquidObject,
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
        public virtual async Task<int> SendOrderCompletedCustomerNotification(Order order, string languageId,
            string attachmentFilePath = null, string attachmentFileName = null, IEnumerable<string> attachments = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderCompleted.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOrderCancelledCustomerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderCancelled.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order cancelled notification to an admin
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendOrderCancelledStoreOwnerNotification(Order order, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderCancelled.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order cancel notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendOrderCancelledVendorNotification(Order order, Vendor vendor, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderCancelled.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, vendor: vendor);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// Sends an order refunded notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderRefunded.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, refundedAmount: refundedAmount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("OrderRefunded.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, refundedAmount: refundedAmount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new order note added notification to a customer
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderNote">Order note</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewOrderNoteAddedCustomerNotification(Order order, OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(order.CustomerLanguageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Customer.NewOrderNote", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = order.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store, orderNote);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return await SendNotification(messageTemplate, emailAccount,
                language.Id, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "Recurring payment cancelled" notification to a store owner
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, string languageId)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var store = await GetStore(recurringPayment.InitialOrder.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("RecurringPaymentCancelled.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = recurringPayment.InitialOrder.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, recurringPayment.InitialOrder, customer, store);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddRecurringPaymentTokens(liquidObject, recurringPayment);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription,
            string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = await GetStore(subscription.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("NewsLetterSubscription.ActivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddNewsLetterSubscriptionTokens(liquidObject, subscription, store);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = subscription.Email;
            var toName = "";
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription,
            string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = await GetStore(subscription.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("NewsLetterSubscription.DeactivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddNewsLetterSubscriptionTokens(liquidObject, subscription, store);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = subscription.Email;
            var toName = "";
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

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
        public virtual async Task<int> SendProductEmailAFriendMessage(Customer customer, Store store, string languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Service.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
            liquidObject.EmailAFriend = new LiquidEmailAFriend(personalMessage, customerEmail);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = friendsEmail;
            var toName = "";
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

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
        public virtual async Task<int> SendWishlistEmailAFriendMessage(Customer customer, Store store, string languageId,
             string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Wishlist.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, store, language, personalMessage, customerEmail);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = friendsEmail;
            var toName = "";
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


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
        public virtual async Task<int> SendProductQuestionMessage(Customer customer, Store store, string languageId,
            Product product, string customerEmail, string fullName, string phone, string message)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Service.AskQuestion", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
            liquidObject.AskQuestion = new LiquidAskQuestion(message, customerEmail, fullName, phone);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
                var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

                var subjectReplaced = LiquidExtensions.Render(liquidObject, subject);
                var bodyReplaced = LiquidExtensions.Render(liquidObject, body);

                await _mediator.Send(new InsertContactUsCommand() {
                    CustomerId = customer.Id,
                    StoreId = store.Id,
                    VendorId = product.VendorId,
                    Email = customerEmail,
                    Enquiry = bodyReplaced,
                    FullName = fullName,
                    Subject = subjectReplaced,
                    EmailAccountId = emailAccount.Id
                });
            }

            var toEmail = emailAccount.Email;
            var toName = "";

            if (!string.IsNullOrEmpty(product?.VendorId))
            {
                var vendor = await _mediator.Send(new GetVendorByIdQuery() { Id = product.VendorId });
                if (vendor != null)
                {
                    toEmail = vendor.Email;
                    toName = vendor.Name;
                }
            }
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName, replyToEmailAddress: customerEmail);
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
        public virtual async Task<int> SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("NewReturnRequest.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = returnRequest.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, store, order, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            if (!string.IsNullOrEmpty(returnRequest.VendorId))
            {
                var vendor = await _mediator.Send(new GetVendorByIdQuery() { Id = returnRequest.VendorId });
                if (vendor != null)
                {
                    var vendorEmail = vendor.Email;
                    var vendorName = vendor.Name;
                    await SendNotification(messageTemplate, emailAccount,
                        languageId, liquidObject,
                        vendorEmail, vendorName);
                }
            }
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("ReturnRequestStatusChanged.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = returnRequest.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, store, order, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            string toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendNewReturnRequestCustomerNotification(ReturnRequest returnRequest, Order order, string languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("NewReturnRequest.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = returnRequest.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, store, order, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new return request note added notification to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="returnRequestNote">Return request note</param>
        /// <param name="order">Order</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewReturnRequestNoteAddedCustomerNotification(ReturnRequest returnRequest, ReturnRequestNote returnRequestNote, Order order)
        {
            if (returnRequestNote == null)
                throw new ArgumentNullException("returnRequestNote");

            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(order.CustomerLanguageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Customer.NewReturnRequestNote", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = returnRequest.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddReturnRequestTokens(liquidObject, returnRequest, store, order, language, returnRequestNote);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.IsGuest() ?
                order.BillingAddress.Email :
                customer.Email;
            var toName = customer.IsGuest() ?
                order.BillingAddress.FirstName :
                customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
                language.Id, liquidObject,
                toEmail, toName);
        }

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
        public virtual async Task<int> SendNewForumTopicMessage(Customer customer, Customer topicauthor,
            ForumTopic forumTopic, Forum forum, string languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

            var store = await GetStore(customer.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Forums.NewForumTopic", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddForumTokens(liquidObject, customer, store, forum, forumTopic);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, topicauthor, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

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
        public virtual async Task<int> SendNewForumPostMessage(Customer customer,
            Customer postauthor,
            ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, string languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

            var store = await GetStore(customer.StoreId);

            var language = await EnsureLanguageIsActive(languageId, store.Id);
            var messageTemplate = await GetMessageTemplate("Forums.NewForumPost", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddForumTokens(liquidObject, postauthor, store, forum, forumTopic, forumPost, friendlyForumTopicPageIndex);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendPrivateMessageNotification(PrivateMessage privateMessage, string languageId)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            var store = await GetStore(privateMessage.StoreId);

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Customer.NewPM", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddPrivateMessageTokens(liquidObject, privateMessage);

            var tocustomer = await _mediator.Send(new GetCustomerByIdQuery() { Id = privateMessage.ToCustomerId });

            if (tocustomer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, tocustomer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = tocustomer.Email;
            var toName = tocustomer.GetFullName();

            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }

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
        public virtual async Task<int> SendNewVendorAccountApplyStoreOwnerNotification(Customer customer, Vendor vendor, Store store, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("VendorAccountApply.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddVendorTokens(liquidObject, vendor, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends 'Vendor information changed' message to a store owner
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendVendorInformationChangeNotification(Vendor vendor, Store store, string languageId)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("VendorInformationChange.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddVendorTokens(liquidObject, vendor, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName);
        }


        /// <summary>
        /// Sends a gift card notification
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendGiftCardNotification(GiftCard giftCard, Order order, string languageId)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            Store store = null;
            if (order != null)
                store = await _storeService.GetStoreById(order.StoreId);
            if (store == null)
                store = (await _storeService.GetAllStores()).FirstOrDefault();

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("GiftCard.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddGiftCardTokens(liquidObject, giftCard);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);
            var toEmail = giftCard.RecipientEmail;
            var toName = giftCard.RecipientName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productReview">Product review</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendProductReviewNotificationMessage(Product product, ProductReview productReview,
            Store store, string languageId)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Product.ProductReview", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = productReview.CustomerId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddProductReviewTokens(liquidObject, product, productReview);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }


        /// <summary>
        /// Sends a vendor review notification message to a store owner
        /// </summary>
        /// <param name="vendorReview">Vendor review</param>
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendVendorReviewNotificationMessage(VendorReview vendorReview, Store store,
            string languageId)
        {
            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Vendor.VendorReview", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);
            //customer
            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = vendorReview.CustomerId });
            //vendor
            var vendor = await _mediator.Send(new GetVendorByIdQuery() { Id = vendorReview.VendorId });

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddVendorReviewTokens(liquidObject, vendor, vendorReview);

            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddVendorTokens(liquidObject, vendor, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendQuantityBelowStoreOwnerNotification(Product product, string languageId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var store = await GetStore("");
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("QuantityBelow.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="combination">Attribute combination</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendQuantityBelowStoreOwnerNotification(Product product, ProductAttributeCombination combination, string languageId)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var store = await GetStore("");
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("QuantityBelow.AttributeCombination.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
            await _messageTokenProvider.AddAttributeCombinationTokens(liquidObject, product, combination);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewVatSubmittedStoreOwnerNotification(Customer customer, Store store,
            string vatName, string vatAddress, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("NewVATSubmitted.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            liquidObject.VatValidationResult = new LiquidVatValidationResult(vatName, vatAddress);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendCustomerDeleteStoreOwnerNotification(Customer customer, string languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = await GetStore("");
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("CustomerDelete.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);


            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendBlogCommentNotificationMessage(BlogPost blogPost, BlogComment blogComment, string languageId)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogComment");

            var store = await GetStore(blogComment.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Blog.BlogComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddBlogCommentTokens(liquidObject, blogPost, blogComment, store, language);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = blogComment.CustomerId });
            if (customer != null && customer.IsRegistered())
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an article comment notification message to a store owner
        /// </summary>
        /// <param name="articleComment">Article comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendArticleCommentNotificationMessage(KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, string languageId)
        {
            if (articleComment == null)
                throw new ArgumentNullException("articleComment");

            var store = await GetStore("");
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Knowledgebase.ArticleComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddArticleCommentTokens(liquidObject, article, articleComment, store, language);

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = articleComment.CustomerId });
            if (customer != null && customer.IsRegistered())
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNewsCommentNotificationMessage(NewsItem newsItem, NewsComment newsComment, string languageId)
        {
            if (newsComment == null)
                throw new ArgumentNullException("newsComment");

            var store = await GetStore(newsComment.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("News.NewsComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddNewsCommentTokens(liquidObject, newsItem, newsComment, store, language);
            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = newsComment.CustomerId });
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a 'Back in stock' notification message to a customer
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendBackInStockNotification(Customer customer, Product product, BackInStockSubscription subscription, string languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = await GetStore(subscription.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Customer.BackInStock", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            if (customer != null)
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            await _messageTokenProvider.AddBackInStockTokens(liquidObject, product, subscription, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

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
        public virtual async Task<int> SendContactUsMessage(Customer customer, Store store, string languageId, string senderEmail,
            string senderName, string subject, string body, string attrInfo, string attrXml)
        {
            var language = await EnsureLanguageIsActive(languageId, store.Id);
            var messageTemplate = await GetMessageTemplate("Service.ContactUs", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

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
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            liquidObject.ContactUs = new LiquidContactUs(senderEmail, senderName, body, attrInfo);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                await _mediator.Send(new InsertContactUsCommand() {
                    CustomerId = customer.Id,
                    StoreId = store.Id,
                    VendorId = "",
                    Email = senderEmail,
                    Enquiry = body,
                    FullName = senderName,
                    Subject = string.IsNullOrEmpty(subject) ? "Contact Us (form)" : subject,
                    ContactAttributeDescription = attrInfo,
                    ContactAttributesXml = attrXml,
                    EmailAccountId = emailAccount.Id
                });
            }
            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName,
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
        /// <param name="store">Store</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendContactVendorMessage(Customer customer, Store store, Vendor vendor, string languageId, string senderEmail,
            string senderName, string subject, string body)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("Service.ContactVendor", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

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
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            liquidObject.ContactUs = new LiquidContactUs(senderEmail, senderName, body, "");

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = vendor.Email;
            var toName = vendor.Name;

            //store in database
            if (_commonSettings.StoreInDatabaseContactUsForm)
            {
                await _mediator.Send(new InsertContactUsCommand() {
                    CustomerId = customer.Id,
                    StoreId = store.Id,
                    VendorId = vendor.Id,
                    Email = senderEmail,
                    Enquiry = body,
                    FullName = senderName,
                    Subject = String.IsNullOrEmpty(subject) ? "Contact Us (form)" : subject,
                    EmailAccountId = emailAccount.Id
                });
            }

            return await SendNotification(messageTemplate, emailAccount, languageId, liquidObject, toEmail, toName,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                replyToEmailAddress: senderEmail,
                replyToName: senderName);
        }

        #region Customer Action Event

        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="ShoppingCartItem">Item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerActionEvent_AddToCart_Notification(CustomerAction action, ShoppingCartItem cartItem, string languageId, Customer customer)
        {
            if (cartItem == null)
                throw new ArgumentNullException("cartItem");

            var store = await GetStore(cartItem.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);

            //product
            var product = await _mediator.Send(new GetProductByIdQuery() { Id = cartItem.ProductId });
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendCustomerActionEvent_AddToOrder_Notification(CustomerAction action, Order order, Customer customer, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, store);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

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

            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);

        }

        #region Auction notification

        public virtual async Task<int> SendAuctionEndedCustomerNotificationWin(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = bid.CustomerId });
            if (customer != null)
            {
                if (string.IsNullOrEmpty(languageId))
                {
                    languageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
                }

                var store = await GetStore(bid.StoreId);
                var language = await EnsureLanguageIsActive(languageId, store.Id);

                var messageTemplate = await GetMessageTemplate("AuctionEnded.CustomerNotificationWin", store.Id);
                if (messageTemplate == null)
                    return 0;

                var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

                LiquidObject liquidObject = new LiquidObject();
                await _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
                await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
                await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);

                //event notification
                await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

                var toEmail = customer.Email;
                var toName = customer.GetFullName();
                return await SendNotification(messageTemplate, emailAccount,
                    languageId, liquidObject,
                    toEmail, toName);
            }
            return 0;
        }

        public virtual async Task<int> SendAuctionEndedCustomerNotificationLost(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customerwin = await _mediator.Send(new GetCustomerByIdQuery() { Id = bid.CustomerId });
            if (customerwin != null)
            {
                var store = await GetStore(bid.StoreId);

                var language = await EnsureLanguageIsActive(languageId, store.Id);

                var messageTemplate = await GetMessageTemplate("AuctionEnded.CustomerNotificationLost", store.Id);
                if (messageTemplate == null)
                    return 0;

                var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

                LiquidObject liquidObject = new LiquidObject();
                await _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
                await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);

                var bids = (await _mediator.Send(new GetBidsByProductIdQuery() { ProductId = bid.ProductId })).Where(x => x.CustomerId != bid.CustomerId).GroupBy(x => x.CustomerId);
                foreach (var item in bids)
                {
                    var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = item.Key });
                    if (string.IsNullOrEmpty(languageId))
                    {
                        languageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
                    }

                    await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

                    //event notification
                    await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

                    var toEmail = customer.Email;
                    var toName = customer.GetFullName();
                    await SendNotification(messageTemplate, emailAccount,
                        languageId, liquidObject,
                        toEmail, toName);
                }
            }
            return 0;
        }

        public virtual async Task<int> SendAuctionEndedCustomerNotificationBin(Product product, string customerId, string languageId, string storeId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var store = await GetStore(storeId);

            var messageTemplate = await GetMessageTemplate("AuctionEnded.CustomerNotificationBin", storeId);
            if (messageTemplate == null)
                return 0;

            var language = await EnsureLanguageIsActive(languageId, store.Id);
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);

            var bids = (await _mediator.Send(new GetBidsByProductIdQuery() { ProductId = product.Id })).Where(x => x.CustomerId != customerId).GroupBy(x => x.CustomerId);
            foreach (var item in bids)
            {
                var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = item.Key });
                if (customer != null)
                {
                    if (string.IsNullOrEmpty(languageId))
                    {
                        languageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
                    }

                    await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

                    //event notification
                    await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

                    var toEmail = customer.Email;
                    var toName = customer.GetFullName();
                    await SendNotification(messageTemplate, emailAccount,
                        languageId, liquidObject,
                        toEmail, toName);
                }
            }

            return 0;
        }
        public virtual async Task<int> SendAuctionEndedStoreOwnerNotification(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            LiquidObject liquidObject = new LiquidObject();
            MessageTemplate messageTemplate = null;
            EmailAccount emailAccount = null;

            if (bid != null)
            {
                var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = bid.CustomerId });
                if (string.IsNullOrEmpty(languageId))
                {
                    languageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
                }

                var store = await GetStore(bid.StoreId);

                var language = await EnsureLanguageIsActive(languageId, store.Id);

                messageTemplate = await GetMessageTemplate("AuctionEnded.StoreOwnerNotification", store.Id);
                if (messageTemplate == null)
                    return 0;

                emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);
                await _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
                await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            }
            else
            {
                var store = (await _storeService.GetAllStores()).FirstOrDefault();
                var language = await EnsureLanguageIsActive(languageId, store.Id);
                messageTemplate = await GetMessageTemplate("AuctionExpired.StoreOwnerNotification", "");
                if (messageTemplate == null)
                    return 0;

                emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);
                await _messageTokenProvider.AddProductTokens(liquidObject, product, language, store);
            }

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return await SendNotification(messageTemplate, emailAccount,
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
        public virtual async Task<int> SendOutBidCustomerNotification(Product product, string languageId, Bid bid)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var customer = await _mediator.Send(new GetCustomerByIdQuery() { Id = bid.CustomerId });
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
            }

            var store = await GetStore(bid.StoreId);

            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate("BidUp.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddAuctionTokens(liquidObject, product, bid);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }
        #endregion



        /// <summary>
        /// Sends a customer action event - Add to cart notification to a customer
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendCustomerActionEvent_Notification(CustomerAction action, string languageId, Customer customer)
        {
            var store = await GetStore(customer.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(action.MessageTemplateId);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            if (!String.IsNullOrEmpty(toEmail))
                toEmail = emailAccount.Email;

            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName);
        }

        #endregion

        public virtual async Task<int> SendNotification(MessageTemplate messageTemplate,
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
            var email = new QueuedEmail {
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

            await _queuedEmailService.InsertQueuedEmail(email);
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
        public virtual async Task<int> SendTestEmail(string messageTemplateId, string sendToEmail,
            LiquidObject liquidObject, string languageId)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");
            var language = await EnsureLanguageIsActive(languageId, "");

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);

            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                sendToEmail, null);
        }

        #endregion

        #endregion
    }
}
