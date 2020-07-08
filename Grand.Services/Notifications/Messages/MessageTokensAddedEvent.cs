using Grand.Domain.Messages;
using MediatR;

namespace Grand.Services.Notifications.Messages
{
    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    public class MessageTokensAddedEvent : INotification
    {
        private readonly MessageTemplate _message;
        private readonly LiquidObject _liquidObject;

        public MessageTokensAddedEvent(MessageTemplate message, LiquidObject liquidObject)
        {
            _message = message;
            _liquidObject = liquidObject;
        }

        public MessageTemplate Message { get { return _message; } }
        public LiquidObject LiquidObject { get { return _liquidObject; } }
    }
}
