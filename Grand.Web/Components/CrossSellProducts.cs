using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class CrossSellProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Constructors

        public CrossSellProductsViewComponent(
            IProductViewModelService productViewModelService,
            ShoppingCartSettings shoppingCartSettings
)
        {
            this._productViewModelService = productViewModelService;
            this._shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (_shoppingCartSettings.CrossSellsNumber == 0)
                return Content("");

            var model = await Task.Run(() => _productViewModelService.PrepareProductsCrossSell(productThumbPictureSize, _shoppingCartSettings.CrossSellsNumber));
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion

    }
}
