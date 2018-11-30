using DotLiquid;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidContactUs : Drop
    {
        private string senderEmail;
        private string senderName;
        private string body;
        private string attributeDescription;

        public void SetProperties(string senderEmail, string senderName, string body, string attributeDescription)
        {
            this.senderEmail = senderEmail;
            this.senderName = senderName;
            this.body = body;
            this.attributeDescription = attributeDescription;
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
    }
}