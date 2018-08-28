using Grand.Core.Infrastructure;
using Grand.Services.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Grand.Framework.Events;

namespace Grand.Framework.Components
{
    public abstract class BaseViewComponent : ViewComponent
    {
        public new ViewViewComponentResult View<TModel>(string viewName, TModel model)
        {
            EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(viewName, model, this);
            return base.View<TModel>(viewName, model);
        }

        public new ViewViewComponentResult View<TModel>(TModel model)
        {
            EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(model, this);
            return base.View<TModel>(model);
        }

        public new ViewViewComponentResult View(string viewName)
        {
            EngineContext.Current.Resolve<IEventPublisher>().ViewComponentEvent(viewName, this);
            return base.View(viewName);
        }

    }
}
