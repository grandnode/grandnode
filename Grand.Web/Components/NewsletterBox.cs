using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class NewsletterBoxViewComponent : BaseViewComponent
    {
        private readonly INewsletterViewModelService _newsletterViewModelService;

        public NewsletterBoxViewComponent(INewsletterViewModelService newsletterViewModelService)
        {
            this._newsletterViewModelService = newsletterViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await Task.Run(() => _newsletterViewModelService.PrepareNewsletterBox());
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}