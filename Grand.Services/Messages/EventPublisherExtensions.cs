using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using Grand.Services.Messages.DotLiquidDrops;

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

        public static void EntityTokensAdded<T, U>(this IEventPublisher eventPublisher, T entity, System.Collections.Generic.IList<U> tokens) where T : ParentEntity
        {
            eventPublisher.Publish(new EntityTokensAddedEvent<T, U>(entity, tokens));
        }

        public static void MessageTokensAdded(this IEventPublisher eventPublisher, MessageTemplate message, /*System.Collections.Generic.IList<U> tokens*/LiquidObject liquidObject)
        {
            //TODO
            //eventPublisher.Publish(new MessageTokensAddedEvent<U>(message, tokens));
        }
    }
}