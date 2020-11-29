using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Grand.Framework.Components
{
    [BaseViewComponent(AdminAccess = true)]
    public abstract class BaseAdminViewComponent : ViewComponent
    {
        public BaseAdminViewComponent()
        {

        }
        public new ViewViewComponentResult View<TModel>(string viewName, TModel model)
        {
            return base.View<TModel>(viewName, model);
        }

        public new ViewViewComponentResult View<TModel>(TModel model)
        {
            return base.View<TModel>(model);
        }

        public new ViewViewComponentResult View(string viewName)
        {
            return base.View(viewName);
        }
    }
}
