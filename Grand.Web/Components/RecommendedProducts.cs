using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class RecommendedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public RecommendedProductsViewComponent(
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings
)
        {
            _productViewModelService = productViewModelService;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.RecommendedProductsEnabled)
                return Content("");

            var model = await _productViewModelService.PrepareProductsRecommended(productThumbPictureSize);
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion

    }
}
