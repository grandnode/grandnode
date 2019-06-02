using Grand.Framework.Components;
using Grand.Services.Events;
using MediatR;

namespace Grand.Framework.Events
{
    public static class ViewComponentEventExtensions
    {
        public static void ViewComponentEvent<T, U>(this IMediator eventPublisher, T entity, U component) where U : BaseViewComponent
        {
            //TO DO
            //eventPublisher..Publish(new ViewComponentEvent<T, U>(entity, component));
        }

        public static void ViewComponentEvent<T, U>(this IMediator eventPublisher, string viewName, T entity, U component) where U : BaseViewComponent
        {
            //TO DO
            //eventPublisher.Publish(new ViewComponentEvent<T, U>(viewName, entity, component));
        }
    }
}
