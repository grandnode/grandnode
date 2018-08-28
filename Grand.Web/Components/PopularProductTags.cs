using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class PopularProductTagsViewComponent : BaseViewComponent
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