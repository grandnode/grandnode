using System;
using Microsoft.AspNetCore.Mvc;
using Grand.Services.Messages;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        private readonly INewsletterWebService _newsletterWebService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        public NewsletterController(INewsletterWebService newsletterWebService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INewsletterCategoryService newsletterCategoryService)
        {
            this._newsletterWebService = newsletterWebService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
        }
       

        [HttpPost]
        public virtual IActionResult SubscribeNewsletter(string email, bool subscribe)
        {
            var model = _newsletterWebService.SubscribeNewsletter(email, subscribe);
            if(model.NewsletterCategory!=null)
            {
                model.ShowCategories = true;
                model.ResultCategory = this.RenderPartialViewToString("NewsletterCategory", model.NewsletterCategory);
            }
            return Json(new
            {
                Success = model.Success,
                Result = model.Result,
                Showcategories = model.ShowCategories,
                ResultCategory = model.ResultCategory,
            });
        }

        [HttpPost]
        public virtual IActionResult SaveCategories(IFormCollection form)
        {

            bool success = false;
            string message = string.Empty;

            var newsletterEmailId = form["NewsletterEmailId"].ToString();
            if (!string.IsNullOrEmpty(newsletterEmailId))
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(newsletterEmailId);
                if(subscription!=null)
                {
                    foreach (string formKey in form.Keys)
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
            });
        }


        public virtual IActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            var model = _newsletterWebService.PrepareSubscriptionActivation(subscription, active);

            return View(model);
        }
    }
}
