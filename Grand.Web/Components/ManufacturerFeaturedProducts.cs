using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class ManufacturerFeaturedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly ICatalogViewModelService _catalogViewModelService;
        #endregion

        #region Constructors

        public ManufacturerFeaturedProductsViewComponent(
            ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await Task.Run(() => _catalogViewModelService.PrepareManufacturerFeaturedProducts());
            if (!model.Any())
                return Content("");
            return View(model);
        }

        #endregion

    }
}
