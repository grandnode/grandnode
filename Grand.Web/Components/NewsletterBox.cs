using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class NewsletterBoxViewComponent : BaseViewComponent
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