using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageBestSellersViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public HomePageBestSellersViewComponent(
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings
)
        {
            this._catalogSettings = catalogSettings;
            this._productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            var model = await Task.Run(() => _productViewModelService.PrepareProductsHomePageBestSellers(productThumbPictureSize).ToList());

            if (!model.Any())
                return Content("");

            return View(model);

        }

        #endregion

    }
}
