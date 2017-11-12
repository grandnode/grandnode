using System.Linq;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class HomePageCategoriesViewComponent : ViewComponent
    {
        #region Fields
        private readonly ICatalogWebService _catalogWebService;
        #endregion

        #region Constructors

        public HomePageCategoriesViewComponent(
            ICatalogWebService catalogWebService
)
        {
            this._catalogWebService = catalogWebService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareHomepageCategory();
            if (!model.Any())
                return Content("");
            return View(model);
        }

        #endregion

    }
}
