using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class NewsletterBoxViewComponent : BaseViewComponent
    {
        private readonly INewsletterViewModelService _newsletterViewModelService;

        public NewsletterBoxViewComponent(INewsletterViewModelService newsletterViewModelService)
        {
            this._newsletterViewModelService = newsletterViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _newsletterViewModelService.PrepareNewsletterBox();
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}