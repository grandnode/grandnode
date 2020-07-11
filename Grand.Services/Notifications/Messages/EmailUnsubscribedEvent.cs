using MediatR;

namespace Grand.Services.Notifications.Messages
{
    public class EmailUnsubscribedEvent : INotification
    {
        private readonly string _email;

        public EmailUnsubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email {
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

}
