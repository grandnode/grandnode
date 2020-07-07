using MediatR;

namespace Grand.Services.Notifications.Messages
{
    public class EmailSubscribedEvent : INotification
    {
        private readonly string _email;

        public EmailSubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email {
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
}
