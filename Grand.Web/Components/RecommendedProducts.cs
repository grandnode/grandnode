using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            this._productViewModelService = productViewModelService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!_catalogSettings.RecommendedProductsEnabled)
                return Content("");

            var model = _productViewModelService.PrepareProductsRecommended(productThumbPictureSize);
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion

    }
}
