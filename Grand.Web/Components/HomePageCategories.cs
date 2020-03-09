using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageCategoriesViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly ICatalogViewModelService _catalogViewModelService;
        #endregion

        #region Constructors

        public HomePageCategoriesViewComponent(
            ICatalogViewModelService catalogViewModelService
)
        {
            _catalogViewModelService = catalogViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _catalogViewModelService.PrepareHomepageCategory();
            if (!model.Any())
                return Content("");
            return View(model);
        }

        #endregion

    }
}
