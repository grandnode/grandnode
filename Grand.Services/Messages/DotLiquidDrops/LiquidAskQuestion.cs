using DotLiquid;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAskQuestion : Drop
    {
        private string message;
        private string email;
        private string fullName;
        private string phone;

        public void SetProperties(string message, string email, string fullName, string phone)
        {
            this.message = message;
            this.email = email;
            this.fullName = fullName;
            this.phone = phone;
        }

        public string Email
        {
            get { return email; }
        }

        public string Message
        {
            get { return message; }
        }

        public string FullName
        {
            get { return fullName; }
        }

        public string Phone
        {
            get { return phone; }
        }
    }
}