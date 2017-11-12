using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class PopularProductTagsViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public PopularProductTagsViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke(string currentCategoryId, string currentProductId)
        {
            var model = _catalogWebService.PreparePopularProductTags();
            if (!model.Tags.Any())
                return Content("");

            return View(model);

        }
    }
}