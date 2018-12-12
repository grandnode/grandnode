using DotLiquid;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidContactUs : Drop
    {
        private string senderEmail;
        private string senderName;
        private string body;
        private string attributeDescription;

        public LiquidContactUs(string senderEmail, string senderName, string body, string attributeDescription)
        {
            this.senderEmail = senderEmail;
            this.senderName = senderName;
            this.body = body;
            this.attributeDescription = attributeDescription;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string SenderEmail
        {
            get { return senderEmail; }
        }

        public string SenderName
        {
            get { return senderName; }
        }

        public string Body
        {
            get { return body; }
        }

        public string AttributeDescription
        {
            get { return attributeDescription; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}