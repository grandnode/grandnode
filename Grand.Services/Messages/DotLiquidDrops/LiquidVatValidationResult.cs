using DotLiquid;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVatValidationResult : Drop
    {
        private string name;
        private string address;

        public LiquidVatValidationResult(string name, string address)
        {
            this.name = name;
            this.address = address;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Name
        {
            get { return name; }
        }

        public string Address
        {
            get { return address; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}