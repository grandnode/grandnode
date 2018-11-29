using DotLiquid;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidEmailAFriend : Drop
    {
        private string _personalMessage;
        private string _customerEmail;

        public void SetProperties(string personalMessage, string customerEmail)
        {
            this._personalMessage = personalMessage;
            this._customerEmail = customerEmail;
        }

        public string PersonalMessage
        {
            get { return _personalMessage; }
        }

        public string Email
        {
            get { return _customerEmail; }
        }
    }
}
