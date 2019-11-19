using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Interfaces;
using Grand.Web.Models.Newsletter;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class NewsletterViewModelService : INewsletterViewModelService
    {
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly CustomerSettings _customerSettings;

        public NewsletterViewModelService(INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService,
            INewsletterCategoryService newsletterCategoryService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            CustomerSettings customerSettings)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationService = localizationService;
            _newsletterCategoryService = newsletterCategoryService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _customerSettings = customerSettings;

        }

        public virtual async Task<NewsletterCategoryModel> PrepareNewsletterCategory(string id)
        {
            var model = new NewsletterCategoryModel();
            model.NewsletterEmailId = id;
            var categories = await _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in categories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory()
                {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    Description = item.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id),
                    Selected = item.Selected
                });
            }
            return model;
        }

        public virtual async Task<NewsletterBoxModel> PrepareNewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return null;

            var model = new NewsletterBoxModel()
            {
                AllowToUnsubscribe = _customerSettings.NewsletterBlockAllowToUnsubscribe
            };

            return await Task.FromResult(model);
        }
        public virtual async Task<SubscribeNewsletterResultModel> SubscribeNewsletter(string email, bool subscribe)
        {
            var model = new SubscribeNewsletterResultModel();

            if (!CommonHelper.IsValidEmail(email))
            {
                model.Result = _localizationService.GetResource("Newsletter.Email.Wrong");
            }
            else
            {
                email = email.Trim();

                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            await _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        model.Result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            await _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        model.Result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                    }
                }
                else if (subscribe)
                {
                    subscription = new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Active = false,
                        StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    await _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    await _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    var modelCategory = await PrepareNewsletterCategory(subscription.Id);
                    if (modelCategory.NewsletterCategories.Count > 0)
                    {
                        model.NewsletterCategory = modelCategory;
                    }

                }
                else
                {
                    model.Result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                }
                model.Success = true;
            }

            return model;

        }
        public virtual async Task<SubscriptionActivationModel> PrepareSubscriptionActivation(NewsLetterSubscription subscription, bool active)
        {
            var model = new SubscriptionActivationModel();

            if (active)
            {
                subscription.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            model.Result = active
                ? _localizationService.GetResource("Newsletter.ResultActivated")
                : _localizationService.GetResource("Newsletter.ResultDeactivated");

            return model;
        }
    }
}