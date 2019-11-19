using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class PopularProductTagsViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public PopularProductTagsViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            _catalogViewModelService = catalogViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string currentCategoryId, string currentProductId)
        {
            var model = await _catalogViewModelService.PreparePopularProductTags();
            if (!model.Tags.Any())
                return Content("");

            return View(model);

        }
    }
}