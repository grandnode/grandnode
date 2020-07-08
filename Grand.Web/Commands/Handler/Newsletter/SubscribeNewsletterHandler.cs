using Grand.Core;
using Grand.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Newsletter;
using Grand.Web.Models.Newsletter;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Newsletter
{
    public class SubscribeNewsletterHandler : IRequestHandler<SubscribeNewsletterCommand, SubscribeNewsletterResultModel>
    {
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly INewsletterCategoryService _newsletterCategoryService;


        public SubscribeNewsletterHandler(INewsLetterSubscriptionService newsLetterSubscriptionService, ILocalizationService localizationService,
            IStoreContext storeContext, IWorkflowMessageService workflowMessageService, IWorkContext workContext, INewsletterCategoryService newsletterCategoryService)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _newsletterCategoryService = newsletterCategoryService;
        }

        public async Task<SubscribeNewsletterResultModel> Handle(SubscribeNewsletterCommand request, CancellationToken cancellationToken)
        {
            var model = new SubscribeNewsletterResultModel();
            var email = request.Email;

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
                    if (request.Subscribe)
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
                else if (request.Subscribe)
                {
                    subscription = new NewsLetterSubscription {
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

        private async Task<NewsletterCategoryModel> PrepareNewsletterCategory(string id)
        {
            var model = new NewsletterCategoryModel();
            model.NewsletterEmailId = id;
            var categories = await _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in categories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory() {
                    Id = item.Id,
                    Name = item.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    Description = item.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id),
                    Selected = item.Selected
                });
            }
            return model;
        }

    }
}
