using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HomePageManufacturersViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public HomePageManufacturersViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            _catalogViewModelService = catalogViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _catalogViewModelService.PrepareHomepageManufacturers();
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}