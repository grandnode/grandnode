using Microsoft.AspNetCore.Mvc;

namespace Grand.Framework.Components
{
    [BaseViewComponent(AdminAccess = true)]
    public abstract class BaseAdminViewComponent : ViewComponent
    {
        public BaseAdminViewComponent()
        {

        }
        public new IViewComponentResult View<TModel>(string viewName, TModel model)
        {
            return base.View<TModel>(viewName, model);
        }

        public new IViewComponentResult View<TModel>(TModel model)
        {
            return base.View<TModel>(model);
        }

        public new IViewComponentResult View(string viewName)
        {
            return base.View(viewName);
        }
    }
}
