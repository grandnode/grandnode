using Grand.Core;
using Grand.Core.Events;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public static class EventPublisherExtensions
    {
        public static async Task EntityInserted<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            await eventPublisher.PublishAsync(new EntityInserted<T>(entity));
        }

        public static async Task EntityUpdated<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            await eventPublisher.PublishAsync(new EntityUpdated<T>(entity));
        }

        public static async Task EntityDeleted<T>(this IEventPublisher eventPublisher, T entity) where T : ParentEntity
        {
            await eventPublisher.PublishAsync(new EntityDeleted<T>(entity));
        }

    }
}