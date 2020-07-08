using Grand.Core;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial class CustomerReminderService : ICustomerReminderService
    {
        #region Fields

        private readonly IRepository<CustomerReminder> _customerReminderRepository;
        private readonly IRepository<CustomerReminderHistory> _customerReminderHistoryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IMediator _mediator;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly IProductService _productService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public CustomerReminderService(
            IRepository<CustomerReminder> customerReminderRepository,
            IRepository<CustomerReminderHistory> customerReminderHistoryRepository,
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            IMediator mediator,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IProductService productService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _customerReminderRepository = customerReminderRepository;
            _customerReminderHistoryRepository = customerReminderHistoryRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _storeService = storeService;
            _customerAttributeParser = customerAttributeParser;
            _productService = productService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        protected async Task<bool> SendEmail(Customer customer, CustomerReminder customerReminder, string reminderlevelId)
        {
            var reminderLevel = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId);
            var emailAccount = await _emailAccountService.GetEmailAccountById(reminderLevel.EmailAccountId);
            var store = customer.ShoppingCartItems.Count > 0 ? await _storeService.GetStoreById(customer.ShoppingCartItems.FirstOrDefault().StoreId) : (await _storeService.GetAllStores()).FirstOrDefault();

            //retrieve message template data
            var bcc = reminderLevel.BccEmailAddresses;
            var languages = await _languageService.GetAllLanguages();
            var langId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, store?.Id);
            if (string.IsNullOrEmpty(langId))
                langId = languages.FirstOrDefault().Id;

            var language = languages.FirstOrDefault(x => x.Id == langId);
            if (language == null)
                language = languages.FirstOrDefault();

            LiquidObject liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, store, language);

            var body = LiquidExtensions.Render(liquidObject, reminderLevel.Body);
            var subject = LiquidExtensions.Render(liquidObject, reminderLevel.Subject);

            //limit name length
            var toName = CommonHelper.EnsureMaximumLength(customer.GetFullName(), 300);
            var email = new QueuedEmail {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = toName,
                ReplyTo = string.Empty,
                ReplyToName = string.Empty,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subject,
                Body = body,
                AttachmentFilePath = "",
                AttachmentFileName = "",
                AttachedDownloads = null,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
            };

            await _queuedEmailService.InsertQueuedEmail(email);
            //activity log
            await _customerActivityService.InsertActivity(string.Format("CustomerReminder.{0}", customerReminder.ReminderRule.ToString()), customer.Id, _localizationService.GetResource(string.Format("ActivityLog.{0}", customerReminder.ReminderRule.ToString())), customer, customerReminder.Name);

            return true;
        }

        protected async Task<bool> SendEmail(Customer customer, Order order, CustomerReminder customerReminder, string reminderlevelId)
        {
            var reminderLevel = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId);
            var emailAccount = await _emailAccountService.GetEmailAccountById(reminderLevel.EmailAccountId);
            var store = await _storeService.GetStoreById(customer.StoreId);
            if (order != null)
            {
                store = await _storeService.GetStoreById(order.StoreId);
            }
            if (store == null)
            {
                store = (await _storeService.GetAllStores()).FirstOrDefault();
            }

            //retrieve message template data
            var bcc = reminderLevel.BccEmailAddresses;
            Language language = null;
            if (order != null)
            {
                language = await _languageService.GetLanguageById(order.CustomerLanguageId);
            }
            else
            {
                var customerLanguageId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);
                if (!string.IsNullOrEmpty(customerLanguageId))
                    language = await _languageService.GetLanguageById(customerLanguageId);
            }
            if (language == null)
            {
                language = (await _languageService.GetAllLanguages()).FirstOrDefault();
            }

            var liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, store, language);
            await _messageTokenProvider.AddOrderTokens(liquidObject, order, customer, await _storeService.GetStoreById(order.StoreId));

            var body = LiquidExtensions.Render(liquidObject, reminderLevel.Body);
            var subject = LiquidExtensions.Render(liquidObject, reminderLevel.Subject);

            //limit name length
            var toName = CommonHelper.EnsureMaximumLength(customer.GetFullName(), 300);
            var email = new QueuedEmail {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = toName,
                ReplyTo = string.Empty,
                ReplyToName = string.Empty,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subject,
                Body = body,
                AttachmentFilePath = "",
                AttachmentFileName = "",
                AttachedDownloads = null,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
            };

            await _queuedEmailService.InsertQueuedEmail(email);
            //activity log
            await _customerActivityService.InsertActivity(string.Format("CustomerReminder.{0}", customerReminder.ReminderRule.ToString()), customer.Id, string.Format("ActivityLog.{0}", customerReminder.ReminderRule.ToString()), customer, customerReminder.Name);

            return true;
        }


        #region Conditions
        protected async Task<bool> CheckConditions(CustomerReminder customerReminder, Customer customer)
        {
            if (customerReminder.Conditions.Count == 0)
                return true;


            bool cond = false;
            foreach (var item in customerReminder.Conditions)
            {
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Category)
                {
                    cond = await ConditionCategory(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Product)
                {
                    cond = ConditionProducts(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Manufacturer)
                {
                    cond = await ConditionManufacturer(item, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(x => x.ProductId).ToList());
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
                    cond = await ConditionCustomerAttribute(item, customer);
                }
            }

            return cond;
        }
        protected async Task<bool> CheckConditions(CustomerReminder customerReminder, Customer customer, Order order)
        {
            if (customerReminder.Conditions.Count == 0)
                return true;


            bool cond = false;
            foreach (var item in customerReminder.Conditions)
            {
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Category)
                {
                    cond = await ConditionCategory(item, order.OrderItems.Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Product)
                {
                    cond = ConditionProducts(item, order.OrderItems.Select(x => x.ProductId).ToList());
                }
                if (item.ConditionType == CustomerReminderConditionTypeEnum.Manufacturer)
                {
                    cond = await ConditionManufacturer(item, order.OrderItems.Select(x => x.ProductId).ToList());
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
                    cond = await ConditionCustomerAttribute(item, customer);
                }
            }

            return cond;
        }
        protected async Task<bool> ConditionCategory(CustomerReminder.ReminderCondition condition, ICollection<string> products)
        {
            bool cond = false;
            if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var item in condition.Categories)
                {
                    foreach (var product in products)
                    {
                        var pr = await _productService.GetProductById(product);
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
                        var pr = await _productService.GetProductById(product);
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
        protected async Task<bool> ConditionManufacturer(CustomerReminder.ReminderCondition condition, ICollection<string> products)
        {
            bool cond = false;
            if (condition.Condition == CustomerReminderConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var item in condition.Manufacturers)
                {
                    foreach (var product in products)
                    {
                        var pr = await _productService.GetProductById(product);
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
                        var pr = await _productService.GetProductById(product);
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
        protected async Task<bool> ConditionCustomerAttribute(CustomerReminder.ReminderCondition condition, Customer customer)
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
                            var selectedValues = await _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
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
                            var selectedValues = await _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
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

        protected async Task UpdateHistory(Customer customer, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            if (history != null)
            {
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel() {
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
                await _customerReminderHistoryRepository.UpdateAsync(history);
            }
            else
            {
                history = new CustomerReminderHistory();
                history.CustomerId = customer.Id;
                history.Status = (int)CustomerReminderHistoryStatusEnum.Started;
                history.StartDate = DateTime.UtcNow;
                history.CustomerReminderId = customerReminder.Id;
                history.ReminderRuleId = customerReminder.ReminderRuleId;
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel() {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });

                await _customerReminderHistoryRepository.InsertAsync(history);
            }

        }

        protected async Task UpdateHistory(Order order, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            if (history != null)
            {
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel() {
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
                await _customerReminderHistoryRepository.UpdateAsync(history);
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
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel() {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });

                await _customerReminderHistoryRepository.InsertAsync(history);
            }

        }
        protected async Task CloseHistoryReminder(CustomerReminder customerReminder, CustomerReminderHistory history)
        {
            history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
            history.EndDate = DateTime.UtcNow;
            await _customerReminderHistoryRepository.UpdateAsync(history);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets customer reminder
        /// </summary>
        /// <param name="id">Customer reminder identifier</param>
        /// <returns>Customer reminder</returns>
        public virtual Task<CustomerReminder> GetCustomerReminderById(string id)
        {
            return _customerReminderRepository.GetByIdAsync(id);
        }


        /// <summary>
        /// Gets all customer reminders
        /// </summary>
        /// <returns>Customer reminders</returns>
        public virtual async Task<IList<CustomerReminder>> GetCustomerReminders()
        {
            var query = from p in _customerReminderRepository.Table
                        orderby p.DisplayOrder
                        select p;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual async Task InsertCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            await _customerReminderRepository.InsertAsync(customerReminder);

            //event notification
            await _mediator.EntityInserted(customerReminder);

        }

        /// <summary>
        /// Delete a customer reminder
        /// </summary>
        /// <param name="customerReminder">Customer reminder</param>
        public virtual async Task DeleteCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            await _customerReminderRepository.DeleteAsync(customerReminder);

            //event notification
            await _mediator.EntityDeleted(customerReminder);

        }

        /// <summary>
        /// Updates the customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual async Task UpdateCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            await _customerReminderRepository.UpdateAsync(customerReminder);

            //event notification
            await _mediator.EntityUpdated(customerReminder);
        }



        public virtual async Task<IPagedList<SerializeCustomerReminderHistory>> GetAllCustomerReminderHistory(string customerReminderId, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = from h in _customerReminderHistoryRepository.Table
                        from l in h.Levels
                        select new SerializeCustomerReminderHistory() { CustomerId = h.CustomerId, Id = h.Id, CustomerReminderId = h.CustomerReminderId, Level = l.Level, SendDate = l.SendDate, OrderId = h.OrderId };

            query = from p in query
                    where p.CustomerReminderId == customerReminderId
                    select p;
            return await PagedList<SerializeCustomerReminderHistory>.Create(query, pageIndex, pageSize);
        }

        #endregion

        #region Tasks

        public virtual async Task Task_AbandonedCart(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.AbandonedCart
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.AbandonedCart
                                          select cr).ToListAsync();
            }

            foreach (var reminder in customerReminder)
            {
                var customers = await (from cu in _customerRepository.Table
                                       where cu.ShoppingCartItems.Any() && cu.LastUpdateCartDateUtc > reminder.LastUpdateDate && cu.Active && !cu.Deleted
                                       && (!String.IsNullOrEmpty(cu.Email))
                                       select cu).ToListAsync();

                foreach (var customer in customers)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();
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
                                    var send = await SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                await CloseHistoryReminder(reminder, activereminderhistory);
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
                                        if (await CheckConditions(reminder, customer))
                                        {
                                            var send = await SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(customer, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer))
                                {
                                    var send = await SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Task_RegisteredCustomer(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.RegisteredCustomer
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.RegisteredCustomer
                                          select cr).ToListAsync();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = await (from cu in _customerRepository.Table
                                       where cu.CreatedOnUtc > reminder.LastUpdateDate && cu.Active && !cu.Deleted
                                       && (!String.IsNullOrEmpty(cu.Email))
                                       && !cu.IsSystemAccount
                                       select cu).ToListAsync();

                foreach (var customer in customers)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();
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
                                    var send = await SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                await CloseHistoryReminder(reminder, activereminderhistory);
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
                                        if (await CheckConditions(reminder, customer))
                                        {
                                            var send = await SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(customer, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer))
                                {
                                    var send = await SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Task_LastActivity(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastActivity
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastActivity
                                          select cr).ToListAsync();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = await (from cu in _customerRepository.Table
                                       where cu.LastActivityDateUtc < reminder.LastUpdateDate && cu.Active && !cu.Deleted
                                       && (!String.IsNullOrEmpty(cu.Email))
                                       select cu).ToListAsync();

                foreach (var customer in customers)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();
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
                                    var send = await SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                await CloseHistoryReminder(reminder, activereminderhistory);
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
                                        if (await CheckConditions(reminder, customer))
                                        {
                                            var send = await SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(customer, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer))
                                {
                                    var send = await SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Task_LastPurchase(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastPurchase
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.LastPurchase
                                          select cr).ToListAsync();
            }
            foreach (var reminder in customerReminder)
            {
                var customers = await (from cu in _customerRepository.Table
                                       where cu.LastPurchaseDateUtc < reminder.LastUpdateDate || cu.LastPurchaseDateUtc == null
                                       && (!String.IsNullOrEmpty(cu.Email)) && cu.Active && !cu.Deleted
                                       && !cu.IsSystemAccount
                                       select cu).ToListAsync();

                foreach (var customer in customers)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();
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
                                    var send = await SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                await CloseHistoryReminder(reminder, activereminderhistory);
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
                                        if (await CheckConditions(reminder, customer))
                                        {
                                            var send = await SendEmail(customer, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(customer, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer))
                                {
                                    var send = await SendEmail(customer, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Task_Birthday(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.Birthday
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.Birthday
                                          select cr).ToListAsync();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                string dateDDMM = DateTime.Now.AddDays(day).ToString("-MM-dd");

                var customers = await (from cu in _customerRepository.Table
                                       where (!String.IsNullOrEmpty(cu.Email)) && cu.Active && !cu.Deleted
                                       && cu.GenericAttributes.Any(x => x.Key == "DateOfBirth" && x.Value.Contains(dateDDMM))
                                       select cu).ToListAsync();

                foreach (var customer in customers)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();
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
                                    var send = await SendEmail(customer, reminder, reminderLevel.Id);
                                    if (send)
                                        await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                await CloseHistoryReminder(reminder, activereminderhistory);
                            }
                        }
                        else
                        {
                            if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                            {
                                var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                if (level != null)
                                {
                                    if (await CheckConditions(reminder, customer))
                                    {
                                        var send = await SendEmail(customer, reminder, level.Id);
                                        if (send)
                                            await UpdateHistory(customer, reminder, level.Id, null);
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
                            if (await CheckConditions(reminder, customer))
                            {
                                var send = await SendEmail(customer, reminder, level.Id);
                                if (send)
                                    await UpdateHistory(customer, reminder, level.Id, null);
                            }
                        }
                    }
                }

                var activehistory = await (from hc in _customerReminderHistoryRepository.Table
                                           where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                           select hc).ToListAsync();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.CustomerId);
                    if (reminderLevel != null && customer != null && customer.Active && !customer.Deleted)
                    {
                        if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                        {
                            var send = await SendEmail(customer, reminder, reminderLevel.Id);
                            if (send)
                                await UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                        }
                    }
                    else
                    {
                        await CloseHistoryReminder(reminder, activereminderhistory);
                    }
                }
            }

        }

        public virtual async Task Task_CompletedOrder(string id = "")
        {
            var dateNow = DateTime.UtcNow.Date;
            var datetimeUtcNow = DateTime.UtcNow;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.CompletedOrder
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.CompletedOrder
                                          select cr).ToListAsync();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                var orders = await (from or in _orderRepository.Table
                                    where or.OrderStatusId == (int)OrderStatus.Complete
                                    && or.CreatedOnUtc >= reminder.LastUpdateDate && or.CreatedOnUtc >= dateNow.AddDays(-day)
                                    select or).ToListAsync();

                foreach (var order in orders)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.BaseOrderId == order.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();

                    Customer customer = await _customerRepository.Table.FirstOrDefaultAsync(x => x.Id == order.CustomerId && x.Active && !x.Deleted);
                    if (customer != null)
                    {
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
                                        var send = await SendEmail(customer, order, reminder, reminderLevel.Id);
                                        if (send)
                                            await UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                                    }
                                }
                                else
                                {
                                    await CloseHistoryReminder(reminder, activereminderhistory);
                                }
                            }
                            else
                            {
                                if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                                {
                                    var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                    if (level != null)
                                    {
                                        if (await CheckConditions(reminder, customer, order))
                                        {
                                            var send = await SendEmail(customer, order, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(order, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer, order))
                                {
                                    var send = await SendEmail(customer, order, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(order, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }

                var activehistory = await (from hc in _customerReminderHistoryRepository.Table
                                           where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                           select hc).ToListAsync();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var order = _orderRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.BaseOrderId);
                    var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId && x.Active && !x.Deleted);
                    if (reminderLevel != null && order != null && customer != null)
                    {
                        if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                        {
                            var send = await SendEmail(customer, order, reminder, reminderLevel.Id);
                            if (send)
                                await UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                        }
                    }
                    else
                    {
                        await CloseHistoryReminder(reminder, activereminderhistory);
                    }
                }

            }

        }
        public virtual async Task Task_UnpaidOrder(string id = "")
        {
            var datetimeUtcNow = DateTime.UtcNow;
            var dateNow = DateTime.UtcNow.Date;
            var customerReminder = new List<CustomerReminder>();
            if (String.IsNullOrEmpty(id))
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Active && datetimeUtcNow >= cr.StartDateTimeUtc && datetimeUtcNow <= cr.EndDateTimeUtc
                                          && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.UnpaidOrder
                                          select cr).ToListAsync();
            }
            else
            {
                customerReminder = await (from cr in _customerReminderRepository.Table
                                          where cr.Id == id && cr.ReminderRuleId == (int)CustomerReminderRuleEnum.UnpaidOrder
                                          select cr).ToListAsync();
            }

            foreach (var reminder in customerReminder)
            {
                int day = 0;
                if (reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null)
                    day = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault().Day;

                var orders = await (from or in _orderRepository.Table
                                    where or.PaymentStatusId == (int)PaymentStatus.Pending
                                    && or.CreatedOnUtc >= reminder.LastUpdateDate && or.CreatedOnUtc >= dateNow.AddDays(-day)
                                    select or).ToListAsync();

                foreach (var order in orders)
                {
                    var history = await (from hc in _customerReminderHistoryRepository.Table
                                         where hc.BaseOrderId == order.Id && hc.CustomerReminderId == reminder.Id
                                         select hc).ToListAsync();

                    Customer customer = await _customerRepository.Table.FirstOrDefaultAsync(x => x.Id == order.CustomerId && x.Active && !x.Deleted);
                    if (customer != null)
                    {
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
                                        var send = await SendEmail(customer, order, reminder, reminderLevel.Id);
                                        if (send)
                                            await UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                                    }
                                }
                                else
                                {
                                    await CloseHistoryReminder(reminder, activereminderhistory);
                                }
                            }
                            else
                            {
                                if (DateTime.UtcNow > history.Max(x => x.EndDate).AddDays(reminder.RenewedDay) && reminder.AllowRenew)
                                {
                                    var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                    if (level != null)
                                    {
                                        if (await CheckConditions(reminder, customer, order))
                                        {
                                            var send = await SendEmail(customer, order, reminder, level.Id);
                                            if (send)
                                                await UpdateHistory(order, reminder, level.Id, null);
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
                                if (await CheckConditions(reminder, customer, order))
                                {
                                    var send = await SendEmail(customer, order, reminder, level.Id);
                                    if (send)
                                        await UpdateHistory(order, reminder, level.Id, null);
                                }
                            }
                        }
                    }
                }
                var activehistory = await (from hc in _customerReminderHistoryRepository.Table
                                           where hc.CustomerReminderId == reminder.Id && hc.Status == (int)CustomerReminderHistoryStatusEnum.Started
                                           select hc).ToListAsync();

                foreach (var activereminderhistory in activehistory)
                {
                    var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                    var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                    var order = _orderRepository.Table.FirstOrDefault(x => x.Id == activereminderhistory.BaseOrderId);
                    var customer = _customerRepository.Table.FirstOrDefault(x => x.Id == order.CustomerId && x.Active && !x.Deleted);
                    if (reminderLevel != null && order != null && customer != null)
                    {
                        if (order.PaymentStatusId == (int)PaymentStatus.Pending)
                        {
                            if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour).AddMinutes(reminderLevel.Minutes))
                            {
                                var send = await SendEmail(customer, order, reminder, reminderLevel.Id);
                                if (send)
                                    await UpdateHistory(order, reminder, reminderLevel.Id, activereminderhistory);
                            }
                        }
                        else
                            await CloseHistoryReminder(reminder, activereminderhistory);

                    }
                    else
                    {
                        await CloseHistoryReminder(reminder, activereminderhistory);
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