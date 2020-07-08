using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IMediator _mediator;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public CampaignService(IRepository<Campaign> campaignRepository,
            IRepository<CampaignHistory> campaignHistoryRepository,
            IRepository<NewsLetterSubscription> newsLetterSubscriptionRepository,
            IRepository<Customer> customerRepository,
            IEmailSender emailSender, IMessageTokenProvider messageTokenProvider,
            IQueuedEmailService queuedEmailService,
            ICustomerService customerService, IStoreService storeService,
            IMediator mediator,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _campaignRepository = campaignRepository;
            _campaignHistoryRepository = campaignHistoryRepository;
            _newsLetterSubscriptionRepository = newsLetterSubscriptionRepository;
            _customerRepository = customerRepository;
            _emailSender = emailSender;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _storeService = storeService;
            _customerService = customerService;
            _mediator = mediator;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual async Task InsertCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            await _campaignRepository.InsertAsync(campaign);

            //event notification
            await _mediator.EntityInserted(campaign);
        }

        /// <summary>
        /// Inserts a campaign history
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual async Task InsertCampaignHistory(CampaignHistory campaignhistory)
        {
            if (campaignhistory == null)
                throw new ArgumentNullException("campaignhistory");

            await _campaignHistoryRepository.InsertAsync(campaignhistory);

        }

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual async Task UpdateCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            await _campaignRepository.UpdateAsync(campaign);

            //event notification
            await _mediator.EntityUpdated(campaign);
        }

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual async Task DeleteCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            await _campaignRepository.DeleteAsync(campaign);

            //event notification
            await _mediator.EntityDeleted(campaign);
        }

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        public virtual Task<Campaign> GetCampaignById(string campaignId)
        {
            return _campaignRepository.GetByIdAsync(campaignId);

        }

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <returns>Campaigns</returns>
        public virtual async Task<IList<Campaign>> GetAllCampaigns()
        {

            var query = from c in _campaignRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            return await query.ToListAsync();
        }

        public virtual async Task<IPagedList<CampaignHistory>> GetCampaignHistory(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var query = from c in _campaignHistoryRepository.Table
                        where c.CampaignId == campaign.Id
                        orderby c.CreatedDateUtc descending
                        select c;
            return await PagedList<CampaignHistory>.Create(query, pageIndex, pageSize);
        }
        public virtual async Task<IPagedList<NewsLetterSubscription>> CustomerSubscriptions(Campaign campaign, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var model = new PagedList<NewsLetterSubscription>();
            if (campaign.CustomerCreatedDateFrom.HasValue || campaign.CustomerCreatedDateTo.HasValue ||
                campaign.CustomerHasShoppingCartCondition != CampaignCondition.All || campaign.CustomerHasShoppingCartCondition != CampaignCondition.All ||
                campaign.CustomerLastActivityDateFrom.HasValue || campaign.CustomerLastActivityDateTo.HasValue ||
                campaign.CustomerLastPurchaseDateFrom.HasValue || campaign.CustomerLastPurchaseDateTo.HasValue ||
                campaign.CustomerTags.Count > 0 || campaign.CustomerRoles.Count > 0)
            {

                var query = from o in _newsLetterSubscriptionRepository.Table
                            where o.Active && o.CustomerId != "" && (o.StoreId == campaign.StoreId || String.IsNullOrEmpty(campaign.StoreId))
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
                if (campaign.CustomerHasShoppingCartCondition == CampaignCondition.True)
                    query = query.Where(x => x.HasShoppingCartItems);
                if (campaign.CustomerHasShoppingCartCondition == CampaignCondition.False)
                    query = query.Where(x => !x.HasShoppingCartItems);

                //customer has order
                if (campaign.CustomerHasOrdersCondition == CampaignCondition.True)
                    query = query.Where(x => x.IsHasOrders);
                if (campaign.CustomerHasOrdersCondition == CampaignCondition.False)
                    query = query.Where(x => !x.IsHasOrders);

                //tags
                if (campaign.CustomerTags.Count > 0)
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
                        query = query.Where(x => x.CustomerRoles.Any(z => z.Id == item));
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
                model = await PagedList<NewsLetterSubscription>.Create(query.Select(x => new NewsLetterSubscription() { CustomerId = x.CustomerId, Email = x.Email, NewsLetterSubscriptionGuid = x.NewsLetterSubscriptionGuid }), pageIndex, pageSize);
            }
            else
            {
                var query = from o in _newsLetterSubscriptionRepository.Table
                            where o.Active && (o.StoreId == campaign.StoreId || string.IsNullOrEmpty(campaign.StoreId))
                            select o;

                if (campaign.NewsletterCategories.Count > 0)
                {
                    foreach (var item in campaign.NewsletterCategories)
                    {
                        query = query.Where(x => x.Categories.Contains(item));
                    }
                }
                model = await PagedList<NewsLetterSubscription>.Create(query, pageIndex, pageSize);
            }

            return await Task.FromResult(model);
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
        public virtual async Task<int> SendCampaign(Campaign campaign, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            int totalEmailsSent = 0;
            var language = await _languageService.GetLanguageById(campaign.LanguageId);
            if (language == null)
                language = (await _languageService.GetAllLanguages()).FirstOrDefault();

            foreach (var subscription in subscriptions)
            {
                Customer customer = null;

                if (!String.IsNullOrEmpty(subscription.CustomerId))
                {
                    customer = await _customerService.GetCustomerById(subscription.CustomerId);
                }

                if (customer == null)
                {
                    customer = await _customerService.GetCustomerByEmail(subscription.Email);
                }

                //ignore deleted or inactive customers when sending newsletter campaigns
                if (customer != null && (!customer.Active || customer.Deleted))
                    continue;

                var liquidObject = new LiquidObject();
                var store = await _storeService.GetStoreById(campaign.StoreId);
                if (store == null)
                    store = (await _storeService.GetAllStores()).FirstOrDefault();

                await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
                await _messageTokenProvider.AddNewsLetterSubscriptionTokens(liquidObject, subscription, store);
                if (customer != null)
                {
                    await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
                    await _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, store, language);
                }

                var body = LiquidExtensions.Render(liquidObject, campaign.Body);
                var subject = LiquidExtensions.Render(liquidObject, campaign.Subject);

                var email = new QueuedEmail {
                    Priority = QueuedEmailPriority.Low,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = subscription.Email,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };
                await _queuedEmailService.InsertQueuedEmail(email);
                await InsertCampaignHistory(new CampaignHistory() { CampaignId = campaign.Id, CustomerId = subscription.CustomerId, Email = subscription.Email, CreatedDateUtc = DateTime.UtcNow, StoreId = campaign.StoreId });

                //activity log
                if (customer != null)
                    await _customerActivityService.InsertActivity("CustomerReminder.SendCampaign", campaign.Id, _localizationService.GetResource("ActivityLog.SendCampaign"), customer, campaign.Name);
                else
                    await _customerActivityService.InsertActivity("CustomerReminder.SendCampaign", campaign.Id, _localizationService.GetResource("ActivityLog.SendCampaign"), campaign.Name + " - " + subscription.Email);

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
        public virtual async Task SendCampaign(Campaign campaign, EmailAccount emailAccount, string email)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            var language = await _languageService.GetLanguageById(campaign.LanguageId);
            if (language == null)
                language = (await _languageService.GetAllLanguages()).FirstOrDefault();

            var store = await _storeService.GetStoreById(campaign.StoreId);
            if (store == null)
                store = (await _storeService.GetAllStores()).FirstOrDefault();

            var liquidObject = new LiquidObject();
            await _messageTokenProvider.AddStoreTokens(liquidObject, store, language, emailAccount);
            var customer = await _customerService.GetCustomerByEmail(email);
            if (customer != null)
            {
                await _messageTokenProvider.AddCustomerTokens(liquidObject, customer, store, language);
                await _messageTokenProvider.AddShoppingCartTokens(liquidObject, customer, store, language);
            }

            var body = LiquidExtensions.Render(liquidObject, campaign.Body);
            var subject = LiquidExtensions.Render(liquidObject, campaign.Subject);

            await _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, email, null);
        }
    }
}