using DotLiquid;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidEmailAFriend : Drop
    {
        private string _personalMessage;
        private string _customerEmail;

        public LiquidEmailAFriend(string personalMessage, string customerEmail)
        {
            _personalMessage = personalMessage;
            _customerEmail = customerEmail;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string PersonalMessage
        {
            get { return _personalMessage; }
        }

        public string Email
        {
            get { return _customerEmail; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
