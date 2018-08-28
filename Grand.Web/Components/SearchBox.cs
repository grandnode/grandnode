using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class SearchBoxViewComponent : BaseViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public SearchBoxViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareSearchBox();
            return View(model);
        }
    }
}