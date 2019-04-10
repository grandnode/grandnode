using Grand.Framework.Components;
using Grand.Services.Events;
using System.Threading.Tasks;

namespace Grand.Framework.Events
{
    public static class ViewComponentEventExtensions
    {
        public static async Task ViewComponentEvent<T, U>(this IEventPublisher eventPublisher, T entity, U component) where U : BaseViewComponent
        {
            await eventPublisher.Publish(new ViewComponentEvent<T, U>(entity, component));
        }

        public static async Task ViewComponentEvent<T, U>(this IEventPublisher eventPublisher, string viewName, T entity, U component) where U : BaseViewComponent
        {
            await eventPublisher.Publish(new ViewComponentEvent<T, U>(viewName, entity, component));
        }
    }
}
