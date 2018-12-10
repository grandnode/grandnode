using DotLiquid;
using Grand.Core.Domain.Forums;
using Grand.Services.Forums;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidPrivateMessage : Drop
    {
        private PrivateMessage _privateMessage;

        public void SetProperties(PrivateMessage privateMessage)
        {
            this._privateMessage = privateMessage;
        }

        public string Subject
        {
            get { return _privateMessage.Subject; }
        }

        public string Text
        {
            get { return _privateMessage.FormatPrivateMessageText(); }
        }
    }
}