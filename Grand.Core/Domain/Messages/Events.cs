using DotLiquid;

namespace Grand.Core.Domain.Messages
{
    public class EmailSubscribedEvent
    {
        private readonly string _email;

        public EmailSubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email
        {
            get { return _email; }
        }

        public bool Equals(EmailSubscribedEvent other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other._email, _email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(EmailSubscribedEvent))
                return false;
            return Equals((EmailSubscribedEvent)obj);
        }

        public override int GetHashCode()
        {
            return (_email != null ? _email.GetHashCode() : 0);
        }
    }

    public class EmailUnsubscribedEvent
    {
        private readonly string _email;

        public EmailUnsubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email
        {
            get { return _email; }
        }

        public bool Equals(EmailUnsubscribedEvent other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other._email, _email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(EmailUnsubscribedEvent))
                return false;
            return Equals((EmailUnsubscribedEvent)obj);
        }

        public override int GetHashCode()
        {
            return (_email != null ? _email.GetHashCode() : 0);
        }
    }

    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="liquidDrop">DotLiquid Drop, e.g. LiquidOrder</param>
    /// <param name="liquidObject">An object that acumulates all DotLiquid Drops</param>
    public class EntityTokensAddedEvent<T> where T : ParentEntity
    {
        private readonly T _entity;
        private readonly Drop _liquidDrop;
        private readonly LiquidObject _liquidObject;

        public EntityTokensAddedEvent(T entity, Drop liquidDrop, LiquidObject liquidObject)
        {
            _entity = entity;
            _liquidDrop = liquidDrop;
            _liquidObject = liquidObject;
        }

        public T Entity { get { return _entity; } }
        public Drop LiquidDrop { get { return _liquidDrop; } }
        public LiquidObject LiquidObject { get { return _liquidObject; } }
    }

    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    public class MessageTokensAddedEvent
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