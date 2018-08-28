using Grand.Framework.Components;
using Grand.Services.Events;

namespace Grand.Framework.Events
{
    public static class ViewComponentEventExtensions
    {
        public static void ViewComponentEvent<T, U>(this IEventPublisher eventPublisher, string viewName, U component) where U : BaseViewComponent
        {
            eventPublisher.Publish(new ViewComponentEvent<T, U>(viewName, component));
        }

        public static void ViewComponentEvent<T, U>(this IEventPublisher eventPublisher, T entity, U component) where U : BaseViewComponent
        {
            eventPublisher.Publish(new ViewComponentEvent<T, U>(entity, component));
        }

        public static void ViewComponentEvent<T, U>(this IEventPublisher eventPublisher, string viewName, T entity, U component) where U : BaseViewComponent
        {
            eventPublisher.Publish(new ViewComponentEvent<T, U>(viewName, entity, component));
        }
    }
}
