using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nop.Services.Messages;
using Nop.Core.Domain.Messages;
using Nop.Core;
using Nop.Services.Stores;

namespace Nop.Services.Customers
{
    public partial class CustomerReminderService : ICustomerReminderService
    {
        #region Fields

        private readonly IRepository<CustomerReminder> _customerReminderRepository;
        private readonly IRepository<CustomerReminderHistory> _customerReminderHistoryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly CustomerSettings _customerSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        #endregion

        #region Ctor

        public CustomerReminderService(
            IRepository<CustomerReminder> customerReminderRepository,
            IRepository<CustomerReminderHistory> customerReminderHistoryRepository,
            IRepository<Customer> customerRepository,
            CustomerSettings customerSettings,
            IEventPublisher eventPublisher,
            ITokenizer tokenizer,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService)
        {
            this._customerReminderRepository = customerReminderRepository;
            this._customerReminderHistoryRepository = customerReminderHistoryRepository;
            this._customerRepository = customerRepository;
            this._customerSettings = customerSettings;
            this._eventPublisher = eventPublisher;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._queuedEmailService = queuedEmailService;
            this._storeService = storeService;
        }

        #endregion

        #region Utilities

        protected bool SendEmail_AbandonedCart(Customer customer, CustomerReminder customerReminder, string reminderlevelId)
        {

            var reminderLevel = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId);
            var emailAccount = _emailAccountService.GetEmailAccountById(reminderLevel.EmailAccountId);
            var store = _storeService.GetStoreById(customer.ShoppingCartItems.FirstOrDefault().StoreId);

            //retrieve message template data
            var bcc = reminderLevel.BccEmailAddresses;
            var subject = reminderLevel.Subject;
            var body = reminderLevel.Body;

            var rtokens = AllowedTokens(CustomerReminderRuleEnum.AbandonedCart);
            var tokens = new List<Token>();

            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddShoppingCartTokens(tokens, customer);

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

            return true;

        }

        protected bool CheckConditions(CustomerReminder customerReminder, Customer customer)
        {
            if(customerReminder.Conditions.Count == 0)
                return true;

            return true;
        }

        protected void UpdateHistory(Customer customer, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            if(history!=null)
            {
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });
                if(customerReminder.Levels.Max(x=>x.Level) == 
                    customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level)
                {
                    history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
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
                history.Levels.Add(new CustomerReminderHistory.HistoryLevel()
                {
                    Level = customerReminder.Levels.FirstOrDefault(x => x.Id == reminderlevelId).Level,
                    ReminderLevelId = reminderlevelId,
                    SendDate = DateTime.UtcNow,
                });

                _customerReminderHistoryRepository.Insert(history);
            }

        }

        protected void CloseHistoryReminder(Customer customer, CustomerReminder customerReminder, string reminderlevelId, CustomerReminderHistory history)
        {
            history.Status = (int)CustomerReminderHistoryStatusEnum.CompletedReminder;
            _customerReminderHistoryRepository.Update(history);
        }

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


        /// <summary>
        /// Get allowed tokens for rule
        /// </summary>
        /// <param name="Rule">Customer Reminder Rule</param>
        public string[] AllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = new List<string>();
            allowedTokens.AddRange(
                new List<string>{ "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%"}
                );

            if(rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.Add("%Cart%");

            }
            allowedTokens.AddRange(
                new List<string>{
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%"
                });
            return allowedTokens.ToArray();
        }

        #endregion

        #region Tasks
        public virtual void Task_AbandonedCart()
        {

            var customerReminder = (from cr in _customerReminderRepository.Table
                                   where cr.Active
                                   select cr).ToList();

            if (customerReminder.Count > 0)
            {
                var customers = from cu in _customerRepository.Table
                                where cu.HasShoppingCartItems && cu.LastUpdateCartDateUtc > DateTime.UtcNow.AddDays(-_customerSettings.CartNoOlderThanXDays)
                                select cu;

                foreach (var reminder in customerReminder)
                {
                    foreach(var customer in customers)
                    {
                        var history = (from hc in _customerReminderHistoryRepository.Table
                                             where hc.CustomerId == customer.Id && hc.CustomerReminderId == reminder.Id                                             
                                             select hc).ToList();
                        if(history.Count > 0)
                        {
                            var activereminderhistory = history.FirstOrDefault(x => x.HistoryStatus == CustomerReminderHistoryStatusEnum.Started);
                            if (activereminderhistory != null)
                            {
                                var lastLevel = activereminderhistory.Levels.OrderBy(x => x.SendDate).LastOrDefault();
                                var reminderLevel = reminder.Levels.FirstOrDefault(x => x.Level > lastLevel.Level);
                                if(reminderLevel!=null)
                                {
                                    if (DateTime.UtcNow > lastLevel.SendDate.AddDays(reminderLevel.Day).AddHours(reminderLevel.Hour))
                                    {
                                        var send = SendEmail_AbandonedCart(customer, reminder, reminderLevel.Id);
                                        if (send)
                                            UpdateHistory(customer, reminder, reminderLevel.Id, activereminderhistory);
                                    }
                                }
                                else
                                {
                                    CloseHistoryReminder(customer, reminder, reminderLevel.Id, activereminderhistory);
                                }
                            }
                            else
                            {
                                if(history.Max(x=>x.EndDate).AddDays(reminder.RenewedDay)  > DateTime.UtcNow)
                                {
                                    var level = reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() != null ? reminder.Levels.OrderBy(x => x.Level).FirstOrDefault() : null;
                                    if (level!=null)
                                    {

                                        if (DateTime.UtcNow > customer.LastUpdateCartDateUtc.Value.AddDays(level.Day).AddHours(level.Hour))
                                        {
                                            if (CheckConditions(reminder, customer))
                                            {
                                                var send = SendEmail_AbandonedCart(customer, reminder, level.Id);
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

                                if (DateTime.UtcNow > customer.LastUpdateCartDateUtc.Value.AddDays(level.Day).AddHours(level.Hour))
                                {
                                    if (CheckConditions(reminder, customer))
                                    {
                                        var send = SendEmail_AbandonedCart(customer, reminder, level.Id);
                                        if (send)
                                            UpdateHistory(customer, reminder, level.Id, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        #endregion
    }
}
