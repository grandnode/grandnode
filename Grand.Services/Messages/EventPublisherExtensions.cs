using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static async Task PublishNewsletterSubscribe(this IEventPublisher eventPublisher, string email)
        {
            await eventPublisher.PublishAsync(new EmailSubscribedEvent(email));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static async Task PublishNewsletterUnsubscribe(this IEventPublisher eventPublisher, string email)
        {
            await eventPublisher.PublishAsync(new EmailUnsubscribedEvent(email));
        }

        public static async Task EntityTokensAdded<T>(this IEventPublisher eventPublisher, T entity, Drop liquidDrop, LiquidObject liquidObject) where T : ParentEntity
        {
            await eventPublisher.PublishAsync(new EntityTokensAddedEvent<T>(entity, liquidDrop, liquidObject));
        }

        public static async Task MessageTokensAdded(this IEventPublisher eventPublisher, MessageTemplate message, LiquidObject liquidObject)
        {
            await eventPublisher.PublishAsync(new MessageTokensAddedEvent(message, liquidObject));
        }
    }
}