using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class NewsletterBoxViewComponent : ViewComponent
    {
        private readonly INewsletterWebService _newsletterWebService;

        public NewsletterBoxViewComponent(INewsletterWebService newsletterWebService)
        {
            this._newsletterWebService = newsletterWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _newsletterWebService.PrepareNewsletterBox();
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}