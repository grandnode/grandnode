using DotLiquid;
using Grand.Domain.Forums;
using Grand.Services.Forums;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidPrivateMessage : Drop
    {
        private PrivateMessage _privateMessage;

        public LiquidPrivateMessage(PrivateMessage privateMessage)
        {
            _privateMessage = privateMessage;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Subject
        {
            get { return _privateMessage.Subject; }
        }

        public string Text
        {
            get { return _privateMessage.FormatPrivateMessageText(); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}