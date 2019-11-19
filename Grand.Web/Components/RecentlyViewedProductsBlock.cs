using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class RecentlyViewedProductsBlockViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public RecentlyViewedProductsBlockViewComponent(
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings
)
        {
            _catalogSettings = catalogSettings;
            _productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var model = await _productViewModelService.PrepareProductsRecentlyViewed(productThumbPictureSize, preparePriceModel);
            return View(model);
        }

        #endregion

    }
}
