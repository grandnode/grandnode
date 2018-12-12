using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;

namespace Grand.Services.Messages
{
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterSubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailSubscribedEvent(email));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterUnsubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailUnsubscribedEvent(email));
        }

        public static void EntityTokensAdded<T>(this IEventPublisher eventPublisher, T entity, Drop liquidDrop, LiquidObject liquidObject) where T : ParentEntity
        {
            eventPublisher.Publish(new EntityTokensAddedEvent<T>(entity, liquidDrop, liquidObject));
        }

        public static void MessageTokensAdded(this IEventPublisher eventPublisher, MessageTemplate message, LiquidObject liquidObject)
        {
            eventPublisher.Publish(new MessageTokensAddedEvent(message, liquidObject));
        }
    }
}