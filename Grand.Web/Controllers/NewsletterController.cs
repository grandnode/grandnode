using Grand.Services.Messages;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        private readonly INewsletterViewModelService _newsletterViewModelService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        public NewsletterController(INewsletterViewModelService newsletterViewModelService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INewsletterCategoryService newsletterCategoryService)
        {
            _newsletterViewModelService = newsletterViewModelService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }
       

        [HttpPost]
        public virtual async Task<IActionResult> SubscribeNewsletter(string email, bool subscribe)
        {
            var model = await _newsletterViewModelService.SubscribeNewsletter(email, subscribe);
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
        public virtual async Task<IActionResult> SaveCategories(IFormCollection form)
        {

            bool success = false;
            string message = string.Empty;

            var newsletterEmailId = form["NewsletterEmailId"].ToString();
            if (!string.IsNullOrEmpty(newsletterEmailId))
            {
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(newsletterEmailId);
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
                    await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription, false);
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


        public virtual async Task<IActionResult> SubscriptionActivation(Guid token, bool active)
        {
            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            var model = await _newsletterViewModelService.PrepareSubscriptionActivation(subscription, active);

            return View(model);
        }
    }
}
