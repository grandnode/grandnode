using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class SuggestedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public SuggestedProductsViewComponent(
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings
)
        {
            this._productViewModelService = productViewModelService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.SuggestedProductsEnabled || _catalogSettings.SuggestedProductsNumber == 0)
                return Content("");

            var model = await Task.Run(() => _productViewModelService.PrepareProductsSuggested(productThumbPictureSize));

            if (!model.Any())
                return Content("");
            return View(model);

        }

        #endregion

    }
}
