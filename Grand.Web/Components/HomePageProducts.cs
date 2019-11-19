using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductViewModelService _productViewModelService;
        #endregion

        #region Constructors

        public HomePageProductsViewComponent(
            IProductViewModelService productViewModelService)
        {
            _productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            var model = await _productViewModelService.PrepareProductsDisplayedOnHomePage(productThumbPictureSize);
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion

    }
}
