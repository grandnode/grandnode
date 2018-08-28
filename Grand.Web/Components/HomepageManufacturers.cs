using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class HomePageManufacturersViewComponent : BaseViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public HomePageManufacturersViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareHomepageManufacturers();
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}