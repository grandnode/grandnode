using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Grand.Framework.Components
{
    public abstract class BaseViewComponent : ViewComponent
    {
        public new ViewViewComponentResult View<TModel>(string viewName, TModel model)
        {
            //TO DO
            //EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(viewName, model, this);
            return base.View<TModel>(viewName, model);
        }

        public new ViewViewComponentResult View<TModel>(TModel model)
        {
            //TO DO
            //EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(model, this);
            return base.View<TModel>(model);
        }

        public new ViewViewComponentResult View(string viewName)
        {
            //TO DO
            //EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(viewName, this);
            return base.View(viewName);
        }
    }
}
