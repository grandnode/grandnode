using System;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Models.Newsletter;
using Grand.Web.Services;

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
       

        [ChildActionOnly]
        public virtual ActionResult NewsletterBox()
        {
            var model = _newsletterWebService.PrepareNewsletterBox();
            if (model == null)
                return Content("");

            return PartialView(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult SubscribeNewsletter(string email, bool subscribe)
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
        [ValidateInput(false)]
        public virtual ActionResult SaveCategories(FormCollection form)
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


        public virtual ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            var model = _newsletterWebService.PrepareSubscriptionActivation(subscription, active);

            return View(model);
        }
    }
}
