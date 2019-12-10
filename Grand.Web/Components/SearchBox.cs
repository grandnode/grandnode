using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class SearchBoxViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public SearchBoxViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            _catalogViewModelService = catalogViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _catalogViewModelService.PrepareSearchBox();
            return View(model);
        }
    }
}