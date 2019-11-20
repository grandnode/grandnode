using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class RelatedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        #endregion

        #region Constructors

        public RelatedProductsViewComponent(
            IProductViewModelService productViewModelService)
        {
            _productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string productId, int? productThumbPictureSize)
        {
            var model = await _productViewModelService.PrepareProductsRelated(productId, productThumbPictureSize);
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion
    }
}
