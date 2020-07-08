using DotLiquid;
using Grand.Domain;
using Grand.Domain.Messages;
using Grand.Services.Notifications.Messages;
using MediatR;
using System.Threading.Tasks;

namespace Grand.Services.Events.Extensions
{
    public static class MessagesEventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="mediator">mediator</param>
        /// <param name="email">The email.</param>
        public static async Task PublishNewsletterSubscribe(this IMediator mediator, string email)
        {
            await mediator.Publish(new EmailSubscribedEvent(email));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="mediator">Mediator</param>
        /// <param name="email">The email.</param>
        public static async Task PublishNewsletterUnsubscribe(this IMediator mediator, string email)
        {
            await mediator.Publish(new EmailUnsubscribedEvent(email));
        }

        public static async Task EntityTokensAdded<T>(this IMediator mediator, T entity, Drop liquidDrop, LiquidObject liquidObject) where T : ParentEntity
        {
            await mediator.Publish(new EntityTokensAddedEvent<T>(entity, liquidDrop, liquidObject));
        }

        public static async Task MessageTokensAdded(this IMediator mediator, MessageTemplate message, LiquidObject liquidObject)
        {
            await mediator.Publish(new MessageTokensAddedEvent(message, liquidObject));
        }
    }
}