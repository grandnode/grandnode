using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Grand.Services.Messages;
using Grand.Core.Domain.Messages;
using Grand.Core;
using Grand.Services.Stores;
using Grand.Services.Catalog;
using Grand.Services.Logging;
using Grand.Services.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Services.Helpers;

namespace Grand.Services.Customers
{
    public partial class CustomerReminderService : ICustomerReminderService
    {
        #region Fields

        private readonly IRepository<CustomerReminder> _customerReminderRepository;
        private readonly IRepository<CustomerReminderHistory> _customerReminderHistoryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly CustomerSettings _customerSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly IProductService _productService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public CustomerReminderService(
            IRepository<CustomerReminder> customerReminderRepository,
            IRepository<CustomerReminderHistory> customerReminderHistoryRepository,
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            CustomerSettings customerSettings,
            IEventPublisher eventPublisher,
            ITokenizer tokenizer,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IProductService productService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            this._customerReminderRepository = customerReminderRepository;
            this._customerReminderHistoryRepository = customerReminderHistoryRepository;
            this._customerRepository = customerRepository;
            this._orderRepository = orderRepository;
            this._customerSettings = customerSettings;
            this._eventPublisher = eventPublisher;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._queuedEmailService = queuedEmailService;
            this._storeService = storeService;
            this._customerAttributeParser = customerAttributeParser;
            this._productService = productService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Utilities

        protected bool SendEmail(Customer customer, CustomerReminder customerReminder, string reminderlevelId)
        {
            var reminderLevel = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId);
            var emailAccount = _emailAccountService.GetEmailAccountById(reminderLevel.EmailAccountId);
            var store = customer.ShoppingCartItems.Count > 0 ? _storeService.GetStoreById(customer.ShoppingCartItems.FirstOrDefault().StoreId) : _storeService.GetAllStores().FirstOrDefault();

            //retrieve message template data
            var bcc = reminderLevel.BccEmailAddresses;
            var subject = reminderLevel.Subject;
            var body = reminderLevel.Body;

            var tokens = new List<Token>();

            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddShoppingCartTokens(tokens, customer);
            _messageTokenProvider.AddRecommendedProductsTokens(tokens, customer);
            _messageTokenProvider.AddRecentlyViewedProductsTokens(tokens, customer);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            //limit name length
            var toName = CommonHelper.EnsureMaximumLength(customer.GetFullName(), 300);
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = toName,
                ReplyTo = string.Empty,
                ReplyToName = string.Empty,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = "",
                AttachmentFileName = "",
                AttachedDownloadId = "",
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
            };

            _queuedEmailService.InsertQueuedEmail(email);
            //activity log
            _customerActivityService.InsertActivity(string.Format("CustomerReminder.{0}", customerReminder.ReminderRule.ToString()), customer.Id, _localizationService.GetResource(string.Format("ActivityLog.{0}", customerReminder.ReminderRule.ToString())), customer, customerReminder.Name);

            return true;
        }
        protected bool SendEmail(Customer customer, Order order, CustomerReminder customerReminder, string reminderlevelId)
        {

            var reminderLevel = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId);
            var emailAccount = _emailAccountService.GetEmailAccountById(reminderLevel.EmailAccountId);
            var store = customer.ShoppingCartItems.Count > 0 ? _storeService.GetStoreById(customer.ShoppingCartItems.FirstOrDefault().StoreId) : _storeService.GetAllStores().FirstOrDefault();

            //retrieve message template data
            var bcc = reminderLevel.BccEmailAddresses;
            var subject = reminderLevel.Subject;
            var body = reminderLevel.Body;

            var tokens = new List<Token>();

            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddShoppingCartTokens(tokens, customer);
            _messageTokenProvider.AddRecommendedProductsTokens(tokens, customer);
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            //limit name length
            var toName = CommonHelper.EnsureMaximumLength(customer.GetFullName(), 300);
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = toName,
                ReplyTo = string.Empty,
                ReplyToName = string.Empty,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = "",
                AttachmentFileName = "",
                AttachedDownloadId = "",
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
            };

            _queuedEmailService.InsertQueuedEmail(email);
            //activity log
            _customerActivityService.InsertActivity(string.Format("CustomerReminder.{0}", customerReminder.ReminderRule.ToString()), customer.Id, _localizationService.GetResource(string.Format("ActivityLog.{0}", customerReminder.ReminderRule.ToString())), customer, customerReminder.Name);

            return true;
        }


        #region Conditions
        protected bool CheckConditions(CustomerReminder customerReminder, Customer customer)
        {
            if (customerReminder.Conditions.Count == 0)
                return true;


            bool cond = false;
            foreach (var item in customerReminder.Conditions)
            {
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Category)
                {
                    cond = ConditionCategory(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == Core.Domain.Orders.ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Product)
                {
                    cond = ConditionProducts(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == Core.Domain.Orders.ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Manufacturer)
                {
                    cond = ConditionManufacturer(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == Core.Domain.Orders.ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerTag)
                {
                    cond = ConditionCustomerTag(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerRole)
                {
                    cond = ConditionCustomerRole(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerRegisterField)
                {
                    cond = ConditionCustomerRegister(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomCustomerAttribute)
                {
                    cond = ConditionCustomerAttribute(item, customer);
                }
            }

            return cond;
        }
        protected bool CheckConditions(CustomerReminder customerReminder, Customer customer, Order order)
        {
            if (customerReminder.Conditions.Count == 0)
                return true;


            bool cond = false;
            foreach (var item in customerReminder.Conditions)
            {
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Category)
                {
                    cond = ConditionCategory(item, order.OrderItems.Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Product)
                {
                    cond = ConditionProducts(item, order.OrderItems.Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Manufacturer)
                {
                    cond = ConditionManufacturer(item, order.OrderItems.Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerTag)
                {
                    cond = ConditionCustomerTag(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerRole)
                {
                    cond = ConditionCustomerRole(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomerRegisterField)
                {
                    cond = ConditionCustomerRegister(item, customer);
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.CustomCustomerAttribute)
                {
                    cond = ConditionCustomerAttribute(item, customer);
                }
            }

            return cond;
        }
        protected bool ConditionCategory(CustomerReminder.ReminderCondition condition, ICollection<string> products)
        {
            bool cond = false;
            if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var item in condition.Categories)
                {
                    foreach (var product in products)
                    {
                        var pr = _productService.GetProductById(product);
                        if (pr != null)
                        {
                            if (pr.ProductCategories.Where(x => x.CategoryId == item).Count() == 0)
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
            {
                foreach (var item in condition.Categories)
                {
                    foreach (var product in products)
                    {
                        var pr = _productService.GetProductById(product);
                        if (pr != null)
                        {
                            if (pr.ProductCategories.Where(x => x.CategoryId == item).Count() > 0)
                                return true;
                        }
                    }
                }
            }

            return cond;
        }
        protected bool ConditionManufacturer(CustomerReminder.ReminderCondition condition, ICollection<string> products)
        {
            bool cond = false;
            if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var item in condition.Manufacturers)
                {
                    foreach (var product in products)
                    {
                        var pr = _productService.GetProductById(product);
                        if (pr != null)
                        {
                            if (pr.ProductManufacturers.Where(x => x.ManufacturerId == item).Count() == 0)
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
            {
                foreach (var item in condition.Manufacturers)
                {
                    foreach (var product in products)
                    {
                        var pr = _productService.GetProductById(product);
                        if (pr != null)
                        {
                            if (pr.ProductManufacturers.Where(x => x.ManufacturerId == item).Count() > 0)
                                return true;
                        }
                    }
                }
            }

            return cond;
        }
        protected bool ConditionProducts(CustomerReminder.ReminderCondition condition, ICollection<string> products)
        {
            bool cond = true;
            if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
            {
                cond = products.ContainsAll(condition.Products);
            }
            if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
            {
                cond = products.ContainsAny(condition.Products);
            }

            return cond;
        }
        protected bool ConditionCustomerRole(CustomerReminder.ReminderCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var customerRoles = customer.CustomerRoles;
                if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
                {
                    cond = customerRoles.Select(x => x.Id).ContainsAll(condition.CustomerRoles);
                }
                if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
                {
                    cond = customerRoles.Select(x => x.Id).ContainsAny(condition.CustomerRoles);
                }
            }
            return cond;
        }
        protected bool ConditionCustomerTag(CustomerReminder.ReminderCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var customerTags = customer.CustomerTags;
                if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
                {
                    cond = customerTags.Select(x => x).ContainsAll(condition.CustomerTags);
                }
                if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
                {
                    cond = customerTags.Select(x => x).ContainsAny(condition.CustomerTags);
                }
            }
            return cond;
        }
        protected bool ConditionCustomerRegister(CustomerReminder.ReminderCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
                {
                    cond = true;
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (customer.GenericAttributes.Where(x => x.Key == item.RegisterField && x.Value == item.RegisterValue).Count() == 0)
                            cond = false;
                    }
                }
                if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
                {
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (customer.GenericAttributes.Where(x => x.Key == item.RegisterField && x.Value == item.RegisterValue).Count() > 0)
                            cond = true;
                    }
                }
            }
            return cond;
        }
        protected bool ConditionCustomerAttribute(CustomerReminder.ReminderCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
                {
                    var customCustomerAttributes = customer.GenericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes");
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
                            cond = true;
                            foreach (var item in condition.CustomCustomerAttributes)
                            {
                                var _fields = item.RegisterField.Split(':');
                                if (_fields.Count() > 1)
                                {
                                    if (selectedValues.Where(x => x.CustomerAttributeId == _fields.FirstOrDefault() && x.Id == _fields.LastOrDefault()).Count() == 0)
                                        cond = false;
                                }
                                else
                                    cond = false;
                            }
                        }
                    }
                }
                if (condition.Condition == CustomerReminderConditionEnum.OneOfThem)
                {

                    var customCustomerAttributes = customer.GenericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes");
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
                            foreach (var item in condition.CustomCustomerAttributes)
                            {
                                var _fields = item.RegisterField.Split(':');
                                if (_fields.Count() > 1)
                                {
                                    if (selectedValues.Where(x => x.CustomerAttributeId == _fields.FirstOrDefault() && x.Id == _fields.LastOrDefault()).Count() > 0)
                                        cond = true;
                                }
                            }
                        }
                    }
                }
            }
            return cond;
        }
        #endregion

        #region History

        protected void UpdateHistory(Customer customer, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            if (history != null)
            {
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });
                if (customerReminder.Levels.Max(x => x.Level) ==
                    customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level)
                {
                    history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
                    history.EndDate = DateTime.UtcNow;
                }
                _customerReminderHistoryRepository.Update(history);
            }
            else
            {
                history = new CustomerReminderHistory();
                history.CustomerId = customer.Id;
                history.Status = (int)CustomerReminderHistoryStatusEnum.Started;
                history.StartDate = DateTime.UtcNow;
                history.CustomerReminderId = customerReminder.Id;
                history.ReminderRuleId = customerReminder.ReminderRuleId;
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });

                _customerReminderHistoryRepository.Insert(history);
            }

        }

        protected void UpdateHistory(Order order, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            if (history != null)
            {
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });
                if (customerReminder.Levels.Max(x => x.Level) ==
                    customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level)
                {
                    history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
                    history.EndDate = DateTime.UtcNow;
                }
                _customerReminderHistoryRepository.Update(history);
            }
            else
            {
                history = new CustomerReminderHistory();
                history.BaseOrderId = order.Id;
                history.CustomerId = order.CustomerId;
                history.Status = (int)CustomerReminderHistoryStatusEnum.Started;
                history.StartDate = DateTime.UtcNow;
                history.CustomerReminderId = customerReminder.Id;
                history.ReminderRuleId = customerReminder.ReminderRuleId;
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });

                _customerReminderHistoryRepository.Insert(history);
            }

        }
        protected void CloseHistoryReminder(CustomerReminder customerReminder, CustomerReminderHistory history)
        {
            history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
            history.EndDate = DateTime.UtcNow;
            _customerReminderHistoryRepository.Update(history);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets customer reminder
        /// </summary>
        /// <param name="id">Customer reminder identifier</param>
        /// <returns>Customer reminder</returns>
        public virtual CustomerReminder GetCustomerReminderById(string id)
        {
            return _customerReminderRepository.GetById(id);
        }


        /// <summary>
        /// Gets all customer reminders
        /// </summary>
        /// <returns>Customer reminders</returns>
        public virtual IList<CustomerReminder> GetCustomerReminders()
        {
            var query = from p in _customerReminderRepository.Table
                        orderby p.DisplayOrder
                        select p;
            return query.ToList();
        }

        /// <summary>
        /// Inserts a customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual void InsertCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Insert(customerReminder);

            //event notification
            _eventPublisher.EntityInserted(customerReminder);

        }

        /// <summary>
        /// Delete a customer reminder
        /// </summary>
        /// <param name="customerReminder">Customer reminder</param>
        public virtual void DeleteCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Delete(customerReminder);

            //event notification
            _eventPublisher.EntityDeleted(customerReminder);

        }

        /// <summary>
        /// Updates the customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual void UpdateCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Update(customerReminder);

            //event notification
            _eventPublisher.EntityUpdated(customerReminder);
        }



        public IPagedList<SerializeCustomerReminderHistory> GetAllCustomerReminderHistory(string customerReminderId, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = from h in _customerReminderHistoryRepository.Table
                        from l in h.Levels
                        select new SerializeCustomerReminderHistory()
                        { CustomerId = h.CustomerId, Id = h.Id, CustomerReminderId = h.CustomerReminderId, Level = l.Level, SendDate = l.SendDate, OrderId = h.OrderId };

            query = from p in query
                    where p.CustomerReminderId == customerReminderId
                    select p;

            var history = new PagedList<SerializeCustomerReminderHistory>(query, pageIndex, pageSize);
            return history;
        }

        #endregion

        #region Tasks

        public virtual void Task_AbandonedCart(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.AbandonedCart
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.AbandonedCart
                                    select cr).ToList();
            }

            foreach (var reminder in customerReminder)
            {
                var customers = from cu in _customerRepository.Table
                                where cu.HasShoppingCartItems && cu.LastUpdateCartDateUtc > reminder.LastUpdateDate
                                && (!String.IsNullOrEmpty(cu.Email))
                                select cu;

                foreach (var customer in customers)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                    where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                    select hc).ToList();
                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {

                                    if (DateTime.UtcNow > customer.LastUpdateCartDateUtc.Value.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                                    {
                                        if (CheckConditions(reminder, customer))
                                        {
                                            var send = SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                UpdateHistory(customer, reminder, level.Id, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {

                            if (DateTime.UtcNow > customer.LastUpdateCartDateUtc.Value.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                            {
                                if (CheckConditions(reminder, customer))
                                {
                                    var send = SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Task_RegisteredCustomer(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.RegisteredCustomer
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.RegisteredCustomer
                                    select cr).ToList();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = from cu in _customerRepository.Table
                                where cu.CreatedOnUtc > reminder.LastUpdateDate
                                && (!String.IsNullOrEmpty(cu.Email))
                                && !cu.IsSystemAccount
                                select cu;

                foreach (var customer in customers)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                    where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                    select hc).ToList();
                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {

                                    if (DateTime.UtcNow > customer.CreatedOnUtc.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                                    {
                                        if (CheckConditions(reminder, customer))
                                        {
                                            var send = SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                UpdateHistory(customer, reminder, level.Id, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {

                            if (DateTime.UtcNow > customer.CreatedOnUtc.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                            {
                                if (CheckConditions(reminder, customer))
                                {
                                    var send = SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Task_LastActivity(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastActivity
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastActivity
                                    select cr).ToList();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = from cu in _customerRepository.Table
                                where cu.LastActivityDateUtc < reminder.LastUpdateDate
                                && (!String.IsNullOrEmpty(cu.Email))
                                select cu;

                foreach (var customer in customers)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                    where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                    select hc).ToList();
                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {

                                    if (DateTime.UtcNow > customer.LastActivityDateUtc.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                                    {
                                        if (CheckConditions(reminder, customer))
                                        {
                                            var send = SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                UpdateHistory(customer, reminder, level.Id, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {
                            if (DateTime.UtcNow > customer.LastActivityDateUtc.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes))
                            {
                                if (CheckConditions(reminder, customer))
                                {
                                    var send = SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Task_LastPurchase(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastPurchase
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastPurchase
                                    select cr).ToList();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = from cu in _customerRepository.Table
                                where cu.LastPurchaseDateUtc < reminder.LastUpdateDate || cu.LastPurchaseDateUtc == null
                                && (!String.IsNullOrEmpty(cu.Email))
                                && !cu.IsSystemAccount
                                select cu;

                foreach (var customer in customers)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                    where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                    select hc).ToList();
                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {
                                    DateTime lastpurchaseDate = customer.LastPurchaseDateUtc.HasValue ? customer.LastPurchaseDateUtc.Value.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes) : DateTime.MinValue;
                                    if (DateTime.UtcNow > lastpurchaseDate)
                                    {
                                        if (CheckConditions(reminder, customer))
                                        {
                                            var send = SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                UpdateHistory(customer, reminder, level.Id, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {
                            DateTime lastpurchaseDate = customer.LastPurchaseDateUtc.HasValue ? customer.LastPurchaseDateUtc.Value.AddDays(level.Day).AddHours(level.Hour).AddMinutes(level.Minutes) : DateTime.MinValue;
                            if (DateTime.UtcNow > lastpurchaseDate)
                            {
                                if (CheckConditions(reminder, customer))
                                {
                                    var send = SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Task_Birthday(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.Birthday
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.Birthday
                                    select cr).ToList();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                string dateDDMM = DateTime.Now.AddDays(day).ToString("-MM-dd");

                var customers = from cu in _customerRepository.Table
                                where (!String.IsNullOrEmpty(cu.Email))
                                && cu.GenericAttributes.Any(x => x.Key == "DateOfBirth" && x.Value.Contains(dateDDMM))
                                select cu;

                foreach (var customer in customers)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                    where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                    select hc).ToList();
                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {
                                    if (CheckConditions(reminder, customer))
                                    {
                                        var send = SendEmail(customer, reminder, level.Id);
                                        if (send)
                                            UpdateHistory(customer, reminder, level.Id, null);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {
                            if (CheckConditions(reminder, customer))
                            {
                                var send = SendEmail(customer, reminder, level.Id);
                                if (send)
                                    UpdateHistory(customer, reminder, level.Id, null);
                            }
                        }
                    }
                }

                var activehistory = (from hc in _customerReminderHistoryRepository.Table
                                        where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                        select hc).ToList();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.CustomerId);
                    if (reminderLevel != null && customer != null)
                    {
                        if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                        {
                            var send = SendEmail(customer, reminder, reminderLevel.Id);
                            if (send)
                                UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                        }
                    }
                    else
                    {
                        CloseHistoryReminder(reminder, activereminderhistory);
                    }
                }
            }

        }

        public virtual void Task_CompletedOrder(string id = "")
        {
            var dateNow = DateTime.UtcNow.Date;
            var datetimeUtcNow = DateTime.UtcNow;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.CompletedOrder
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.CompletedOrder
                                    select cr).ToList();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                var orders = from or in _orderRepository.Table
                             where or.OrderStatusId == (int)OrderStatus.Complete
                             && or.CreatedOnUtc >= reminder.LastUpdateDate && or.CreatedOnUtc >= dateNow.AddDays(-day)
                             select or;

                foreach (var order in orders)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                   where hc.BaseOrderId == order.Id && hc.CustomerReminderId == reminder.Id
                                   select hc).ToList();

                    Customer customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId);

                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, order, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {
                                    if (CheckConditions(reminder, customer, order))
                                    {
                                        var send = SendEmail(customer, order, reminder, level.Id);
                                        if (send)
                                            UpdateHistory(order, reminder, level.Id, null);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {
                            if (CheckConditions(reminder, customer, order))
                            {
                                var send = SendEmail(customer, order, reminder, level.Id);
                                if (send)
                                    UpdateHistory(order, reminder, level.Id, null);
                            }
                        }
                    }
                }

                var activehistory = (from hc in _customerReminderHistoryRepository.Table
                                     where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                     select hc).ToList();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var order = _orderRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.BaseOrderId);
                    if (reminderLevel != null && order != null)
                    {
                        var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId);
                        if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                        {
                            var send = SendEmail(customer, order, reminder, reminderLevel.Id);
                            if (send)
                                UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                        }
                    }
                    else
                    {
                        CloseHistoryReminder(reminder, activereminderhistory);
                    }
                }

            }

        }
        public virtual void Task_UnpaidOrder(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow;
            var dateNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                    && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.UnpaidOrder
                                    select cr).ToList();
            }
            else
            {
                customerReminder = (from cr in _customerReminderRepository.Table
                                    where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.UnpaidOrder
                                    select cr).ToList();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                var orders = from or in _orderRepository.Table
                             where or.PaymentStatusId == (int)PaymentStatus.Pending
                             && or.CreatedOnUtc >= reminder.LastUpdateDate && or.CreatedOnUtc >= dateNow.AddDays(-day)
                             select or;

                foreach (var order in orders)
                {
                    var history = (from hc in _customerReminderHistoryRepository.Table
                                   where hc.BaseOrderId == order.Id && hc.CustomerReminderId == reminder.Id
                                   select hc).ToList();

                    Customer customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId);

                    if (history.Any())
                    {
                        var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                        if (activereminderhistory != null)
                        {
                            var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                            var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                            if (reminderLevel != null)
                            {
                                if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                                {
                                    var send = SendEmail(customer, order, reminder, reminderLevel.Id);
                                    if (send)
                                        UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {
                                    if (CheckConditions(reminder, customer, order))
                                    {
                                        var send = SendEmail(customer, order, reminder, level.Id);
                                        if (send)
                                            UpdateHistory(order, reminder, level.Id, null);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                        if (level != null)
                        {
                            if (CheckConditions(reminder, customer, order))
                            {
                                var send = SendEmail(customer, order, reminder, level.Id);
                                if (send)
                                    UpdateHistory(order, reminder, level.Id, null);
                            }
                        }
                    }
                }
                var activehistory = (from hc in _customerReminderHistoryRepository.Table
                                     where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                     select hc).ToList();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var order = _orderRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.BaseOrderId);
                    if (reminderLevel != null && order != null)
                    {
                        if (order.PaymentStatusId == (int)PaymentStatus.Pending)
                        {
                            var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId);
                            if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                            {
                                var send = SendEmail(customer, order, reminder, reminderLevel.Id);
                                if (send)
                                    UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                            }
                        }
                        else
                            CloseHistoryReminder(reminder, activereminderhistory);

                    }
                    else
                    {
                        CloseHistoryReminder(reminder, activereminderhistory);
                    }
                }
            }
        }

        #endregion
    }

    public class SerializeCustomerReminderHistory
    {
        public string Id { get; set; }
        public string CustomerReminderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime SendDate { get; set; }
        public int Level { get; set; }
        public string OrderId { get; set; }
    }
}
