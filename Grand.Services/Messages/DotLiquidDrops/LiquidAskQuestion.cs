using DotLiquid;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAskQuestion : Drop
    {
        private string message;
        private string email;
        private string fullName;
        private string phone;

        public LiquidAskQuestion(string message, string email, string fullName, string phone)
        {
            this.message = message;
            this.email = email;
            this.fullName = fullName;
            this.phone = phone;

            AdditionalTokens = new Dictionary<string, string>();
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}