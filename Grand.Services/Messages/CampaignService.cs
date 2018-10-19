using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Messages
{
    public partial class CampaignService : ICampaignService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignHistory> _campaignHistoryRepository;
        private readonly IRepository<NewsLetterSubscription> _newsLetterSubscriptionRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public CampaignService(IRepository<Campaign> campaignRepository,
            IRepository<CampaignHistory> campaignHistoryRepository,
            IRepository<NewsLetterSubscription> newsLetterSubscriptionRepository,
            IRepository<Customer> customerRepository,
            IEmailSender emailSender, IMessageTokenProvider messageTokenProvider,
            ITokenizer tokenizer, IQueuedEmailService queuedEmailService,
            ICustomerService customerService, IStoreContext storeContext,
            IEventPublisher eventPublisher,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            this._campaignRepository = campaignRepository;
            this._campaignHistoryRepository = campaignHistoryRepository;
            this._newsLetterSubscriptionRepository = newsLetterSubscriptionRepository;
            this._customerRepository = customerRepository;
            this._emailSender = emailSender;
            this._messageTokenProvider = messageTokenProvider;
            this._tokenizer = tokenizer;
            this._queuedEmailService = queuedEmailService;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._eventPublisher = eventPublisher;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
        }

        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual void InsertCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Insert(campaign);

            //event notification
            _eventPublisher.EntityInserted(campaign);
        }

        /// <summary>
        /// Inserts a campaign history
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual void InsertCampaignHistory(CampaignHistory campaignhistory)
        {
            if (campaignhistory == null)
                throw new ArgumentNullException("campaignhistory");

            _campaignHistoryRepository.Insert(campaignhistory);

        }

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void UpdateCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Update(campaign);

            //event notification
            _eventPublisher.EntityUpdated(campaign);
        }

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void DeleteCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Delete(campaign);

            //event notification
            _eventPublisher.EntityDeleted(campaign);
        }

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        public virtual Campaign GetCampaignById(string campaignId)
        {
            return _campaignRepository.GetById(campaignId);

        }

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <returns>Campaigns</returns>
        public virtual IList<Campaign> GetAllCampaigns()
        {

            var query = from c in _campaignRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            var campaigns = query.ToList();

            return campaigns;
        }
        public virtual IPagedList<CampaignHistory> GetCampaignHistory(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var query = from c in _campaignHistoryRepository.Table
                        where c.CampaignId == campaign.Id
                        orderby c.CreatedDateUtc descending
                        select c;
            var list = new PagedList<CampaignHistory>(query, pageIndex, pageSize);
            return list;

        }
        public virtual IPagedList<NewsLetterSubscription> CustomerSubscriptions(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var model = new PagedList<NewsLetterSubscription>();
            if (campaign.CustomerCreatedDateFrom.HasValue || campaign.CustomerCreatedDateTo.HasValue ||
                campaign.CustomerHasShoppingCartCondition != CampaignCondition.All || campaign.CustomerHasShoppingCartCondition != CampaignCondition.All ||
                campaign.CustomerLastActivityDateFrom.HasValue || campaign.CustomerLastActivityDateTo.HasValue ||
                campaign.CustomerLastPurchaseDateFrom.HasValue || campaign.CustomerLastPurchaseDateTo.HasValue ||
                campaign.CustomerTags.Count > 0 || campaign.CustomerRoles.Count > 0 || campaign.NewsletterCategories.Count > 0 )
            {

                var query = from o in _newsLetterSubscriptionRepository.Table
                            where o.Active && o.CustomerId!="" && (o.StoreId == campaign.StoreId || String.IsNullOrEmpty(campaign.StoreId))
                            join c in _customerRepository.Table on o.CustomerId equals c.Id into joined
                            from customers in joined
                            select new CampaignCustomerHelp() {
                                CustomerEmail = customers.Email,
                                Email = o.Email,
                                CustomerId = customers.Id,
                                CreatedOnUtc = customers.CreatedOnUtc,
                                CustomerTags = customers.CustomerTags,
                                CustomerRoles = customers.CustomerRoles,
                                NewsletterCategories = o.Categories,
                                HasShoppingCartItems = customers.ShoppingCartItems.Any(),
                                LastActivityDateUtc = customers.LastActivityDateUtc,
                                LastPurchaseDateUtc = customers.LastPurchaseDateUtc,
                                NewsLetterSubscriptionGuid = o.NewsLetterSubscriptionGuid
                            };

                //create date
                if (campaign.CustomerCreatedDateFrom.HasValue)
                    query = query.Where(x => x.CreatedOnUtc >= campaign.CustomerCreatedDateFrom.Value);
                if (campaign.CustomerCreatedDateTo.HasValue)
                    query = query.Where(x => x.CreatedOnUtc <= campaign.CustomerCreatedDateTo.Value);

                //last activity
                if (campaign.CustomerLastActivityDateFrom.HasValue)
                    query = query.Where(x => x.LastActivityDateUtc >= campaign.CustomerLastActivityDateFrom.Value);
                if (campaign.CustomerLastActivityDateTo.HasValue)
                    query = query.Where(x => x.LastActivityDateUtc <= campaign.CustomerLastActivityDateTo.Value);

                //last purchase
                if (campaign.CustomerLastPurchaseDateFrom.HasValue)
                    query = query.Where(x => x.LastPurchaseDateUtc >= campaign.CustomerLastPurchaseDateFrom.Value);
                if (campaign.CustomerLastPurchaseDateTo.HasValue)
                    query = query.Where(x => x.LastPurchaseDateUtc <= campaign.CustomerLastPurchaseDateTo.Value);

                //customer has shopping carts
                if(campaign.CustomerHasShoppingCartCondition == CampaignCondition.True)
                    query = query.Where(x => x.HasShoppingCartItems);
                if (campaign.CustomerHasShoppingCartCondition == CampaignCondition.False)
                    query = query.Where(x => !x.HasShoppingCartItems);

                //customer has order
                if (campaign.CustomerHasOrdersCondition == CampaignCondition.True)
                    query = query.Where(x => x.IsHasOrders);
                if (campaign.CustomerHasOrdersCondition == CampaignCondition.False)
                    query = query.Where(x => !x.IsHasOrders);

                //tags
                if(campaign.CustomerTags.Count > 0)
                {
                    foreach (var item in campaign.CustomerTags)
                    {
                        query = query.Where(x => x.CustomerTags.Contains(item));
                    }
                }
                //roles
                if (campaign.CustomerRoles.Count > 0)
                {
                    foreach (var item in campaign.CustomerRoles)
                    {
                        query = query.Where(x => x.CustomerRoles.Any(z=>z.Id == item));
                    }
                }
                //categories news
                if (campaign.NewsletterCategories.Count > 0)
                {
                    foreach (var item in campaign.NewsletterCategories)
                    {
                        query = query.Where(x => x.NewsletterCategories.Contains(item));
                    }
                }

                model = new PagedList<NewsLetterSubscription>(query.Select(x => new NewsLetterSubscription() { CustomerId = x.CustomerId, Email = x.Email, NewsLetterSubscriptionGuid = x.NewsLetterSubscriptionGuid }), pageIndex, pageSize);
            }
            else
            {
                var query = from o in _newsLetterSubscriptionRepository.Table
                            where o.Active && (o.StoreId == campaign.StoreId || String.IsNullOrEmpty(campaign.StoreId))
                            select o;
                model = new PagedList<NewsLetterSubscription>(query, pageIndex, pageSize);
            }

            return model;
        }

        private class CampaignCustomerHelp
        {
            public CampaignCustomerHelp()
            {
                CustomerRoles = new List<CustomerRole>();
            }
            public string CustomerId { get; set; }
            public string CustomerEmail { get; set; }
            public string Email { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public DateTime LastActivityDateUtc { get; set; }
            public DateTime? LastPurchaseDateUtc { get; set; }
            public bool HasShoppingCartItems { get; set; }
            public bool IsHasOrders { get; set; }
            public ICollection<string> CustomerTags { get; set; }
            public ICollection<string> NewsletterCategories { get; set; }
            public ICollection<CustomerRole> CustomerRoles { get; set; }
            public Guid NewsLetterSubscriptionGuid { get; set; }
        }

        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        public virtual int SendCampaign(Campaign campaign, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            int totalEmailsSent = 0;

            foreach (var subscription in subscriptions)
            {
                Customer customer = null;

                if(!String.IsNullOrEmpty(subscription.CustomerId))
                {
                    customer = _customerService.GetCustomerById(subscription.CustomerId);
                }

                if(customer == null)
                {
                    customer = _customerService.GetCustomerByEmail(subscription.Email);
                }

                //ignore deleted or inactive customers when sending newsletter campaigns
                if (customer != null && (!customer.Active || customer.Deleted))
                    continue;

                var tokens = new List<Token>();
                _messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore, emailAccount);
                _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);
                if (customer != null)
                {
                    _messageTokenProvider.AddCustomerTokens(tokens, customer);
                    _messageTokenProvider.AddShoppingCartTokens(tokens, customer);
                    _messageTokenProvider.AddRecommendedProductsTokens(tokens, customer);
                    _messageTokenProvider.AddRecentlyViewedProductsTokens(tokens, customer);
                }

                string subject = _tokenizer.Replace(campaign.Subject, tokens, false);
                string body = _tokenizer.Replace(campaign.Body, tokens, true);

                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.Low,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = subscription.Email,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };
                _queuedEmailService.InsertQueuedEmail(email);
                InsertCampaignHistory(new CampaignHistory() { CampaignId = campaign.Id, CustomerId = subscription.CustomerId, Email = subscription.Email, CreatedDateUtc = DateTime.UtcNow, StoreId = campaign.StoreId });

                //activity log
                if(customer!=null)
                    _customerActivityService.InsertActivity("CustomerReminder.SendCampaign", campaign.Id, _localizationService.GetResource("ActivityLog.SendCampaign"), customer, campaign.Name);
                else
                    _customerActivityService.InsertActivity("CustomerReminder.SendCampaign", campaign.Id, _localizationService.GetResource("ActivityLog.SendCampaign"), campaign.Name + " - " + subscription.Email);

                totalEmailsSent++;
            }
            return totalEmailsSent;
        }

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="email">Email</param>
        public virtual void SendCampaign(Campaign campaign, EmailAccount emailAccount, string email)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore, emailAccount);
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer != null)
            {
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
                _messageTokenProvider.AddShoppingCartTokens(tokens, customer);
                _messageTokenProvider.AddRecommendedProductsTokens(tokens, customer);
                _messageTokenProvider.AddRecentlyViewedProductsTokens(tokens, customer);
            }

            string subject = _tokenizer.Replace(campaign.Subject, tokens, false);
            string body = _tokenizer.Replace(campaign.Body, tokens, true);

            _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, email, null);
        }
    }
}
