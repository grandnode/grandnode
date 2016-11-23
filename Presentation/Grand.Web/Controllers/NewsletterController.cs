using System;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Models.Newsletter;

namespace Grand.Web.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IStoreContext _storeContext;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly CustomerSettings _customerSettings;

        public NewsletterController(ILocalizationService localizationService,
            IWorkContext workContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService,
            IStoreContext storeContext,
            INewsletterCategoryService newsletterCategoryService,
            CustomerSettings customerSettings)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;
            this._storeContext = storeContext;
            this._newsletterCategoryService = newsletterCategoryService;
            this._customerSettings = customerSettings;
        }

        protected virtual NewsletterCategoryModel PrepareNewsletterCategoryModel(string id)
        {
            var model = new NewsletterCategoryModel();
            model.NewsletterEmailId = id;
            var categories = _newsletterCategoryService.GetNewsletterCategoriesByStore(_storeContext.CurrentStore.Id);
            foreach (var item in categories)
            {
                model.NewsletterCategories.Add(new NewsletterSimpleCategory()
                {
                    Id = item.Id,
                    Name = item.GetLocalized(x=>x.Name),
                    Description = item.GetLocalized(x=>x.Description),
                    Selected = item.Selected
                });
            }
            return model;
        }

        [ChildActionOnly]
        public ActionResult NewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            var model = new NewsletterBoxModel()
            {
                AllowToUnsubscribe = _customerSettings.NewsletterBlockAllowToUnsubscribe
            };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SubscribeNewsletter(string email, bool subscribe)
        {
            string result;
            string resultCategory = string.Empty;
            bool success = false;
            bool showcategories = false;

            if (!CommonHelper.IsValidEmail(email))
            {
                result = _localizationService.GetResource("Newsletter.Email.Wrong");
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
                        result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
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

                    result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    var model = PrepareNewsletterCategoryModel(subscription.Id);
                    if (model.NewsletterCategories.Count > 0)
                    {
                        showcategories = true;
                        resultCategory = this.RenderPartialViewToString("NewsletterCategory", model);
                    }

                }
                else
                {
                    result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                }
                success = true;
            }

            return Json(new
            {
                Success = success,
                Result = result,
                Showcategories = showcategories,
                ResultCategory = resultCategory,
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveCategories(FormCollection form)
        {

            bool success = false;
            string message = string.Empty;

            var newsletterEmailId = form["NewsletterEmailId"];
            if (newsletterEmailId != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(newsletterEmailId);
                if(subscription!=null)
                {
                    foreach (string formKey in form.AllKeys)
                    {
                        if(formKey.Contains("Category_"))
                        {
                            try
                            {
                                var category = formKey.Split('_')[1];
                                subscription.Categories.Add(category);
                            }
                            catch(Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                    }
                    success = true;
                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription, false);
                }
                else
                {
                    message = "Email not exist";
                }
            }
            else
                message = "Empty NewsletterEmailId";

            return Json(new
            {
                Success = success,
                Message = message
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            var model = new SubscriptionActivationModel();

            if (active)
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            model.Result = active 
                ?  _localizationService.GetResource("Newsletter.ResultActivated")
                : _localizationService.GetResource("Newsletter.ResultDeactivated");

            return View(model);
        }
    }
}
