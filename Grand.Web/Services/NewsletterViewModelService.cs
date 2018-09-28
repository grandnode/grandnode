using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Models.Newsletter;
using System;

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
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._localizationService = localizationService;
            this._newsletterCategoryService = newsletterCategoryService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._customerSettings = customerSettings;

        }

        public virtual NewsletterCategoryModel PrepareNewsletterCategory(string id)
        {
            var model = new NewsletterCategoryModel();
            model.NewsletterEmailId = id;
            var categories = _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in categories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory()
                {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name),
                    Description = item.GetLocalized(x => x.Description),
                    Selected = item.Selected
                });
            }
            return model;
        }

        public virtual NewsletterBoxModel PrepareNewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return null;

            var model = new NewsletterBoxModel()
            {
                AllowToUnsubscribe = _customerSettings.NewsletterBlockAllowToUnsubscribe
            };

            return model;
        }
        public virtual SubscribeNewsletterResultModel SubscribeNewsletter(string email, bool subscribe)
        {
            var model = new SubscribeNewsletterResultModel();

            if (!CommonHelper.IsValidEmail(email))
            {
                model.Result = _localizationService.GetResource("Newsletter.Email.Wrong");
            }
            else
            {
                email = email.Trim();

                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        model.Result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
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
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    var modelCategory = PrepareNewsletterCategory(subscription.Id);
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
        public virtual SubscriptionActivationModel PrepareSubscriptionActivation(NewsLetterSubscription subscription, bool active)
        {
            var model = new SubscriptionActivationModel();

            if (active)
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            model.Result = active
                ? _localizationService.GetResource("Newsletter.ResultActivated")
                : _localizationService.GetResource("Newsletter.ResultDeactivated");

            return model;
        }
    }
}