using Grand.Core;
using Grand.Core.Events;

namespace Grand.Services.Events
{
    public static class EventPublisherExtensions
    {
        public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            System.Threading.Tasks.Task.Run(() => {
                eventPublisher.Publish(new EntityInserted<T>(entity));
            });
        }

        public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            System.Threading.Tasks.Task.Run(() => {
                eventPublisher.Publish(new EntityUpdated<T>(entity));
            });
        }

        public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            System.Threading.Tasks.Task.Run(() => {
                eventPublisher.Publish(new EntityDeleted<T>(entity));
            });
        }

    }
}